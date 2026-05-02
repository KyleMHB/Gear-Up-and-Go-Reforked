using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;
using Verse.AI;
using HarmonyLib;

namespace GearUpAndGo
{
	public class Command_GearUpAndGo : Command
	{
		public Command_GearUpAndGo() : base()
		{
			defaultLabel = "TD.GearAndGo".Translate();
			defaultDesc = "TD.GearAndGoDesc".Translate();
			alsoClickIfOtherInGroupClicked = false;
		}

		public static void Target(string policy = null)
		{
			Find.Targeter.BeginTargeting(new TargetingParameters() { canTargetLocations = true },
				(LocalTargetInfo target) => Go(target, policy));
		}

		public static void Go(LocalTargetInfo target, string policy)
		{
			if (!Event.current.alt)
				GearUpPolicyComp.Instance?.Set(policy ?? Mod.settings?.betterPawnControlBattlePolicy);

			foreach (Pawn p in Find.Selector.SelectedObjects
				.Where(o => o is Pawn p && p.IsColonistPlayerControlled && p.jobs != null).Cast<Pawn>())
			{
				Job job = JobMaker.MakeJob(GearUpAndGoJobDefOf.GearUpAndGo, target);
				job.count = 0;
				p.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
			}
		}

		public static void End()
		{
			GearUpPolicyComp.Instance?.Revert();
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			if (ev.shift && GearUpPolicyComp.Instance?.IsOn() == true)
				End();
			else
				Target();
		}

		public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
				var list = SetBetterPawnControl.PolicyList();
				if (list == null) yield break;
				foreach (string policy in list)
					yield return new FloatMenuOption(policy, () => Target(policy));
			}
		}
	}

	[StaticConstructorOnStartup]
	public class TexGearUpAndGo
	{
		public static readonly Texture2D guagIcon = ContentFinder<Texture2D>.Get("CommandGearUpAndGo");
		public static readonly Texture2D guagIconActive = ContentFinder<Texture2D>.Get("CommandGearUpAndGoActive");
	}

	public class GearUpPolicyComp : GameComponent
	{
		public static GearUpPolicyComp comp;

		public string lastPolicy = "";

		public GearUpPolicyComp(Game game) {
			comp = this;
		}

		public static GearUpPolicyComp Instance => comp ?? Current.Game?.GetComponent<GearUpPolicyComp>();

		public override void ExposeData()
		{
			Scribe_Values.Look(ref lastPolicy, "lastPolicy", "");
		}
		public void Set(string policy)
		{
			if (string.IsNullOrWhiteSpace(policy)) return;
			if (lastPolicy == "")
			{
				lastPolicy = SetBetterPawnControl.CurrentPolicy();
			}
			SetBetterPawnControl.SetPawnControlPolicy(policy);
		}
		public void Revert()
		{
			if (lastPolicy == "") return;

			SetBetterPawnControl.SetPawnControlPolicy(lastPolicy);

			lastPolicy = "";
		}

		public bool IsOn()
		{
			return lastPolicy != "";
		}

		public string CurrentGearUpPolicy()
		{
			if (!string.IsNullOrWhiteSpace(lastPolicy))
				return SetBetterPawnControl.CurrentPolicy();

			return Mod.settings?.betterPawnControlBattlePolicy ?? "";
		}
	}


	[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
	public static class GearUpAndGizGo
	{
		//	public override IEnumerable<Gizmo> GetGizmos()
		public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
		{
			if (__instance.IsColonistPlayerControlled)
			{
				yield return new Command_GearUpAndGo()
				{
					icon = GearUpPolicyComp.Instance?.IsOn() == true ? TexGearUpAndGo.guagIconActive : TexGearUpAndGo.guagIcon
				};
			}

			foreach (var r in __result)
				yield return r;
		}
	}

	// backcompat dummy so it doesn't log error loading old saves
	public class CompGearUpAndGizGo : ThingComp { }
}
