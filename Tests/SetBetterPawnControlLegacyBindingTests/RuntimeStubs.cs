using System.Collections.Generic;
using Verse;

namespace BetterPawnControl
{
	public class Policy
	{
		public string label;

		public Policy(string label)
		{
			this.label = label;
		}
	}

	public class AssignManager
	{
		public static List<Policy> policies = new List<Policy>();
		private static Policy activePolicy;
		public static List<Pawn> colonists = new List<Pawn>();
		public static List<string> saves = new List<string>();

		public static List<Pawn> Colonists()
		{
			return colonists;
		}

		public static void SaveCurrentState(List<Pawn> pawns)
		{
			saves.Add(GetActivePolicy().label + ":" + pawns.Count);
		}

		public static void LoadState(Policy policy)
		{
			activePolicy = policy;
		}

		public static Policy GetActivePolicy()
		{
			return activePolicy ?? policies[0];
		}
	}
}
