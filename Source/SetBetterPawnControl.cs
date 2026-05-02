using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace GearUpAndGo
{
	[StaticConstructorOnStartup]
	static class SetBetterPawnControl
	{
		private const string packageId = "VouLT.BetterPawnControl";
		public static bool working;
		private static readonly HashSet<string> warnedMissingPolicies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public static bool Active()
		{
			return ModsConfig.ActiveModsInLoadOrder.Any(m =>
				string.Equals(m.PackageIdPlayerFacing, packageId, StringComparison.OrdinalIgnoreCase));
		}

		static SetBetterPawnControl()
		{
			if (Active())
			{
				working = true;//assume it works till it fails
			}
		}
		public static void Stop(Exception e)
		{
			working = false;

			Verse.Log.Error($"Hey, dang, Gear Up and Go failed to use Better Pawn Control:\n\n{e}");
		}

		public static void SetPawnControlPolicy(string policyName)
		{
			if (!working) return;

			try
			{
				SetPawnControlPolicyEx(policyName);
			}
			catch (Exception e)
			{
				Stop(e);
			}
		}

		private static void SetPawnControlPolicyEx(string policyName)
		{
			if (string.IsNullOrWhiteSpace(policyName))
				return;

			BetterPawnControl.Policy policy = BetterPawnControl.AssignManager.policies
				.FirstOrDefault(p => string.Equals(p.label, policyName, StringComparison.OrdinalIgnoreCase));

			if (policy != null)
			{
				BetterPawnControl.AssignManager.LoadState(policy);
				return;
			}

			if (warnedMissingPolicies.Add(policyName))
				Verse.Log.Warning($"Gear Up and Go could not find Better Pawn Control policy '{policyName}'. Gearing will continue without changing policy.");
		}

		public static string CurrentPolicy()
		{
			if (!working) return "";

			try
			{
				return CurrentPolicyEx();
			}
			catch (Exception e)
			{
				Stop(e);
				return "";
			}
		}

		private static string CurrentPolicyEx() =>
			BetterPawnControl.AssignManager.GetActivePolicy()?.label ?? "";

		public static List<string> PolicyList()
		{
			if (!working) return null;

			try
			{
				return PolicyListEx().ToList();
			}
			catch (Exception e)
			{
				Stop(e);
				return null;
			}
		}

		private static List<string> PolicyListEx()
		{
			//This oneliner apparently causes problems on load when BCP not active.?
			//BetterPawnControl.AssignManager.policies.Select(p => p.label);

			List<string> result = new();
			foreach (BetterPawnControl.Policy p in BetterPawnControl.AssignManager.policies)
				result.Add(p.label);

			return result;
		}
	}
}
