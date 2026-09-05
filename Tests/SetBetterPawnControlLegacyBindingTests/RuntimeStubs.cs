using System.Collections.Generic;

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
