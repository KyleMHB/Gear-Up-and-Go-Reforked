using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GearUpAndGo
{
	public class JobDriver_GearUpAndGo : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn pawn = toil.actor;
				LocalTargetInfo target = TargetA;
				int iteration = job.count < 0 ? 0 : job.count;

				if (!GearUpJobPlanner.Validate(pawn, target))
					return;

				if (iteration >= GearUpJobPlanner.MaxIterations)
				{
					GearUpJobPlanner.Reject(pawn, $"{pawn.LabelShort} could not finish gearing up. Gear Up and Go stopped to avoid repeating jobs.");
					return;
				}

				if (iteration == 0)
				{
					Job outfitStandJob = OutfitStandsPlusIntegration.TryCreateUseStandJob(pawn);
					if (outfitStandJob != null)
					{
						GearUpJobQueue.QueueNextGearPass(pawn, outfitStandJob, target, iteration + 1);
						return;
					}
				}

				Job gearJob = GearUpJobPlanner.NextGearJob(pawn);
				if (gearJob != null)
				{
					GearUpJobQueue.QueueNextGearPass(pawn, gearJob, target, iteration + 1);
					return;
				}

				Job gotoJob = GearUpJobPlanner.FinalGotoJob(pawn, target);
				if (gotoJob == null)
				{
					GearUpJobPlanner.Reject(pawn, $"{pawn.LabelShort} cannot reach the Gear Up and Go target.");
					return;
				}

				GearUpJobQueue.StartFinalGoto(pawn, gotoJob);
				FleckMaker.Static(gotoJob.targetA.Cell, pawn.Map, FleckDefOf.FeedbackGoto);
			};
			yield return toil;
		}
	}
}
