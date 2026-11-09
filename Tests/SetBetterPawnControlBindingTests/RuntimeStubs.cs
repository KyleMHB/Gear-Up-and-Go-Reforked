using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BetterPawnControlForked
{
	public class Policy
	{
		public string label;

		public Policy(string label)
		{
			this.label = label;
		}
	}

	public class Manager
	{
		public static List<Policy> policies { get; set; } = new List<Policy>();
		public static List<Pawn> colonists = new List<Pawn>();

		internal static IEnumerable<Pawn> Colonists()
		{
			return colonists.Where(p => p != null);
		}
	}

	public class AssignManager : Manager
	{
		private static Policy activePolicy;
		public static List<string> saves = new List<string>();

		internal static void SaveCurrentState(List<Pawn> pawns)
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
