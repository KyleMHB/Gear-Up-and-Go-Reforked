using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace GearUpAndGo
{
	internal static class GearUpJobPlanner
	{
		public const int MaxIterations = 16;

		private static readonly JobGiver_OptimizeApparel fallbackApparelOptimizer = new JobGiver_OptimizeApparel();
		private static readonly Type ceLoadoutGiverType;
		private static readonly MethodInfo ceLoadoutGetter;
		private static readonly MethodInfo tryIssueJobPackageInfo;

		static GearUpJobPlanner()
		{
			ceLoadoutGiverType = AccessTools.TypeByName("JobGiver_UpdateLoadout");
			tryIssueJobPackageInfo = AccessTools.Method(typeof(ThinkNode), nameof(ThinkNode.TryIssueJobPackage));
			MethodInfo getter = AccessTools.Method(typeof(Pawn_Thinker), nameof(Pawn_Thinker.TryGetMainTreeThinkNode));
			if (ceLoadoutGiverType != null && getter != null)
				ceLoadoutGetter = getter.MakeGenericMethod(ceLoadoutGiverType);
		}

		public static bool Validate(Pawn pawn, LocalTargetInfo target)
		{
			if (pawn?.Map == null || pawn.jobs == null)
				return false;

			if (pawn.Downed || pawn.Dead)
			{
				Reject(pawn, "IsIncapped".Translate(pawn.LabelShort, pawn));
				return false;
			}

			if (!target.IsValid || !target.Cell.IsValid || !target.Cell.InBounds(pawn.Map))
			{
				Reject(pawn, "Gear Up and Go target is invalid.");
				return false;
			}

			return true;
		}

		public static Job NextGearJob(Pawn pawn)
		{
			if (pawn?.thinker == null)
				return null;

			pawn.mindState?.Notify_OutfitChanged();
			ThinkResult apparelResult = ApparelJob(pawn);
			if (apparelResult != ThinkResult.NoJob && IsUsableGearJob(pawn, apparelResult.Job))
				return apparelResult.Job;

			ThinkResult ceResult = CombatExtendedLoadoutJob(pawn);
			if (ceResult != ThinkResult.NoJob && IsUsableGearJob(pawn, ceResult.Job))
				return ceResult.Job;

			return null;
		}

		public static Job FinalGotoJob(Pawn pawn, LocalTargetInfo target)
		{
			IntVec3 destination = RCellFinder.BestOrderedGotoDestNear(target.Cell, pawn);
			if (!destination.IsValid || !destination.InBounds(pawn.Map))
				return null;

			if (!pawn.CanReach(destination, PathEndMode.OnCell, Danger.Deadly))
				return null;

			Job job = JobMaker.MakeJob(JobDefOf.Goto, destination);
			job.exitMapOnArrival = pawn.Map.exitMapGrid.IsExitCell(target.Cell);
			return job;
		}

		private static ThinkResult ApparelJob(Pawn pawn)
		{
			JobGiver_OptimizeApparel optimizer = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>() ?? fallbackApparelOptimizer;
			return optimizer.TryIssueJobPackage(pawn, new JobIssueParams());
		}

		private static ThinkResult CombatExtendedLoadoutJob(Pawn pawn)
		{
			if (ceLoadoutGiverType == null || ceLoadoutGetter == null || tryIssueJobPackageInfo == null)
				return ThinkResult.NoJob;

			try
			{
				object loadoutGiver = ceLoadoutGetter.Invoke(pawn.thinker, Array.Empty<object>());
				if (loadoutGiver == null)
					return ThinkResult.NoJob;

				return (ThinkResult)tryIssueJobPackageInfo.Invoke(loadoutGiver, new object[] { pawn, new JobIssueParams() });
			}
			catch (Exception e)
			{
				Verse.Log.Warning($"Gear Up and Go could not ask Combat Extended for a loadout job: {e}");
				return ThinkResult.NoJob;
			}
		}

		private static bool IsUsableGearJob(Pawn pawn, Job job)
		{
			if (job?.def == null)
				return false;

			Thing targetThing = job.targetA.Thing;
			if (targetThing == null || !targetThing.Spawned)
				return true;

			return pawn.CanReserveAndReach(targetThing, PathEndMode.Touch, Danger.Deadly);
		}

		public static void Reject(Pawn pawn, string message)
		{
			if (pawn?.Faction == Faction.OfPlayer)
				Messages.Message(message, MessageTypeDefOf.RejectInput, historical: false);
		}
	}

	internal static class GearUpJobQueue
	{
		public static void QueueNextGearPass(Pawn pawn, Job gearJob, LocalTargetInfo target, int nextIteration)
		{
			RemoveQueuedGearUpJobs(pawn);
			Job nextGearUp = JobMaker.MakeJob(GearUpAndGoJobDefOf.GearUpAndGo, target);
			nextGearUp.count = nextIteration;
			pawn.jobs.jobQueue.EnqueueFirst(nextGearUp, JobTag.DraftedOrder);
			pawn.jobs.jobQueue.EnqueueFirst(gearJob, JobTag.DraftedOrder);
		}

		public static void StartFinalGoto(Pawn pawn, Job gotoJob)
		{
			if (!pawn.Drafted && pawn.drafter != null)
			{
				List<QueuedJob> queue = DrainQueue(pawn);
				pawn.drafter.Drafted = true;
				pawn.jobs.StartJob(gotoJob, JobCondition.Succeeded);
				RestoreQueue(pawn, queue);
				return;
			}

			pawn.jobs.StartJob(gotoJob, JobCondition.Succeeded);
		}

		private static void RemoveQueuedGearUpJobs(Pawn pawn)
		{
			List<QueuedJob> queue = DrainQueue(pawn);
			foreach (QueuedJob queuedJob in queue.Where(q => q.job?.def != GearUpAndGoJobDefOf.GearUpAndGo))
				pawn.jobs.jobQueue.EnqueueLast(queuedJob.job, queuedJob.tag);
		}

		private static List<QueuedJob> DrainQueue(Pawn pawn)
		{
			List<QueuedJob> queue = new List<QueuedJob>();
			while (pawn.jobs.jobQueue.Count > 0)
				queue.Add(pawn.jobs.jobQueue.Dequeue());
			return queue;
		}

		private static void RestoreQueue(Pawn pawn, List<QueuedJob> queue)
		{
			foreach (QueuedJob queuedJob in queue)
				pawn.jobs.jobQueue.EnqueueLast(queuedJob.job, queuedJob.tag);
		}
	}
}
