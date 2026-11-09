using System;
using System.Collections.Generic;
using GearUpAndGo;
using static GearUpAndGo.Tests.TestAssertions;
using Verse;
using BetterPawnControlForked;

internal static class Program
{
	private static int Main()
	{
		try
		{
			ModsConfig.ActiveModsInLoadOrder.Add(new ModContentPack
			{
				PackageIdPlayerFacing = "KyleMHB.BetterPawnControlForked"
			});
			Manager.policies = new List<Policy>
			{
				new Policy("Everyday"),
				new Policy("Combat")
			};

			Assert(SetBetterPawnControl.Active(), "the fork package should be detected");
			Assert(SetBetterPawnControl.working, "BPC binding should accept an inherited policies property");
			AssertSequence(SetBetterPawnControl.PolicyList(), "Everyday", "Combat");
			Manager.colonists = new List<Pawn> { new Pawn(), new Pawn() };
			Assert(!SetBetterPawnControl.SetPawnControlPolicy("Missing"), "a missing policy should report that nothing was switched");
			Assert(SetBetterPawnControl.CurrentPolicy() == "Everyday", "a missing policy should leave the active policy unchanged");
			Assert(AssignManager.saves.Count == 0, "a missing policy should not save the current state");
			Assert(SetBetterPawnControl.SetPawnControlPolicy("combat"), "an existing policy should report the switch, ignoring case");
			AssertSequence(AssignManager.saves, "Everyday:2");
			Assert(SetBetterPawnControl.CurrentPolicy() == "Combat", "the active policy should be read through GetActivePolicy");

			Console.WriteLine("PASS: Better Pawn Control inherited-property binding");
			return 0;
		}
		catch (Exception e)
		{
			Console.Error.WriteLine(e.Message);
			return 1;
		}
	}

}
