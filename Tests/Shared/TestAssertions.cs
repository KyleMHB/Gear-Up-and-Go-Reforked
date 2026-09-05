using System;
using System.Collections.Generic;

namespace GearUpAndGo.Tests
{
	public static class TestAssertions
	{
		public static void Assert(bool condition, string message)
		{
			if (!condition)
				throw new InvalidOperationException("FAIL: " + message);
		}

		public static void AssertSequence(List<string> actual, params string[] expected)
		{
			Assert(actual != null, "the policy list should be available after binding");
			Assert(actual.Count == expected.Length, "the policy list should contain every discovered policy");
			for (int i = 0; i < expected.Length; i++)
				Assert(actual[i] == expected[i], "the policy list should preserve policy labels");
		}
	}
}
