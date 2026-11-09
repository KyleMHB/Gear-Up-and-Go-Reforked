using System;
using System.Collections.Generic;
using GearUpAndGo;
using static GearUpAndGo.Tests.TestAssertions;
using Verse;
using BetterPawnControl;

internal static class Program
{
	private static int Main()
	{
		try
		{
			ModsConfig.ActiveModsInLoadOrder.Add(new ModContentPack
			{
				PackageIdPlayerFacing = "VouLT.BetterPawnControl"
			});
			AssignManager.policies = new List<Policy>
			{
				new Policy("Everyday"),
				new Policy("Combat")
			};

			Assert(SetBetterPawnControl.working, "legacy BPC binding should continue to accept the policies field");
			AssertSequence(SetBetterPawnControl.PolicyList(), "Everyday", "Combat");
			AssignManager.colonists = new List<Pawn> { new Pawn(), new Pawn() };
			Assert(!SetBetterPawnControl.SetPawnControlPolicy("Missing"), "a missing policy should report that nothing was switched");
			Assert(SetBetterPawnControl.CurrentPolicy() == "Everyday", "a missing policy should leave the active policy unchanged");
			Assert(AssignManager.saves.Count == 0, "a missing policy should not save the current state");
			Assert(SetBetterPawnControl.SetPawnControlPolicy("combat"), "an existing policy should report the switch, ignoring case");
			AssertSequence(AssignManager.saves, "Everyday:2");
			Assert(SetBetterPawnControl.CurrentPolicy() == "Combat", "the active legacy policy should be read after selection");

			Console.WriteLine("PASS: Better Pawn Control legacy-field binding");
			return 0;
		}
		catch (Exception e)
		{
			Console.Error.WriteLine(e.Message);
			return 1;
		}
	}

}
