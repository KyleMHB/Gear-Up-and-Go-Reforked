using System.Collections.Generic;

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
	}

	public class AssignManager : Manager
	{
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
