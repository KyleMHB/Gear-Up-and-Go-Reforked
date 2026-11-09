using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorldDevTests;
using Verse;
using Verse.AI;

namespace GearUpAndGo.DevTests;

// Expected behaviour (README "Features"/"Usage"/"Settings"): with Better Pawn Control loaded, Gear & Go switches to the chosen
// BPC preset before the pawns gear up (the name match ignores case), and restoring brings back the previous preset. When the
// named preset cannot be found, gearing continues without changing the preset, and the switch stays off. Like BPC's own preset
// switch, Gear & Go saves the colonists' current settings into the active preset first, so restoring keeps changes BPC had not
// saved yet.
// Mechanical setup: Command_GearUpAndGo.Go reads Event.current and the selection, so the cases mirror it: GearUpPolicyComp.Set
// with an explicit preset name (the right-click menu's path), then the GearUpAndGo job ordered directly. Restoring calls
// Command_GearUpAndGo.End, which the alert click and Shift-click run. The targeter, right-click menu, Alt and the mod-settings
// default preset are not exercised. BPC's own presets are set up through its Assign-tab flow (save, then load), by reflection.
public class BpcBatch : Batch
{
    private const string PeaceOutfit = "GUAG test peace outfit", BattleOutfit = "GUAG test battle outfit", ChangedOutfit = "GUAG test changed outfit";
    private const string BattlePolicy = "GUAG Test Battle";
    private const string BattleRequest = "guag test BATTLE"; // differs in case from the preset label
    private const string MissingPolicy = "GUAG test missing preset";
    private const int GearTicks = 6000;

    public override string Id => "guag-bpc";
    public override int SegmentCount => 2;
    public override int MapSize => 75;
    public override IReadOnlyList<PlannedCase> Plan { get; } = new[]
    {
        "guag.bpc-bind", "guag.bpc-missing", "guag.bpc-switch", "guag.bpc-gear", "guag.bpc-reload", "guag.bpc-restore",
    }.Select(id => new PlannedCase(id, Levels.Mechanical)).ToArray();

    private static string Outfit(Pawn p) => p.outfits?.CurrentApparelPolicy?.label ?? "none";

    public override IEnumerable<Wait> Segment(int index, BatchContext ctx)
    {
        var bpc = BpcBridge.Create(out var missing);
        if (bpc == null)
        {
            Kit.SkipRemaining(ctx, this, "Better Pawn Control Forked or Gear Up and Go members not found: " + missing, CoverageReasons.SetupFailed);
            yield break;
        }
        if (index == 1)
        {
            foreach (var w in Reloaded(ctx, bpc)) yield return w;
            yield break;
        }

        ctx.Setup($"{ModsConfig.ActiveModsInLoadOrder.Count()} active mods; GUAG BPC detection {SetBetterPawnControlActive()}");
        var comp = GearUpPolicyComp.Instance;
        var policy0 = bpc.Find(0);
        Kit.RemoveStartingColonists(ctx);
        var area = Kit.Area(ctx, 24, 10);
        IntVec3 At(int x, int z) => area.Min + new IntVec3(x, 0, z);

        // Two apparel policies that differ only in whether the duster is allowed.
        var dusterDef = Kit.Def("Apparel_Duster");
        var peace = Current.Game.outfitDatabase.MakeNewOutfit();
        peace.label = PeaceOutfit;
        peace.filter.SetAllow(dusterDef, false);
        var battle = Current.Game.outfitDatabase.MakeNewOutfit();
        battle.label = BattleOutfit;
        battle.filter.SetAllow(dusterDef, true);
        var changed = Current.Game.outfitDatabase.MakeNewOutfit();
        changed.label = ChangedOutfit;
        changed.filter.SetAllow(dusterDef, false);

        var pawn = Kit.Worker(ctx, At(2, 2), WorkTypeDefOf.Hauling);
        pawn.apparel.DestroyAll();
        var pawns = new List<Pawn> { pawn };

        // The player's BPC setup: policy 0 keeps the peace outfit, the battle preset keeps the battle outfit.
        pawn.outfits.CurrentApparelPolicy = peace;
        var battlePolicy = bpc.AddPolicy(BattlePolicy);
        int battleId = bpc.Id(battlePolicy);
        bpc.SelectPolicy(pawns, battlePolicy);
        pawn.outfits.CurrentApparelPolicy = battle;
        bpc.SelectPolicy(pawns, policy0);
        ctx.Setup($"Developer made apparel policies '{PeaceOutfit}' (duster disallowed) and '{BattleOutfit}' (duster allowed), " +
            $"added BPC Assign preset '{BattlePolicy}' (id {battleId}) and, through BPC's save-then-load flow, saved {pawn.LabelShort} " +
            $"with the peace outfit under '{bpc.Label(policy0)}' and the battle outfit under '{BattlePolicy}'; active preset is now '{bpc.Label(bpc.Active)}'");

        var policies = bpc.GuagPolicyList() ?? new List<string>();
        bool listed = policies.Contains(BattlePolicy) && policies.Contains(bpc.Label(policy0));
        ctx.Begin("guag.bpc-bind").Expect("working", true).Expect("policyComp", true).Expect("listsBothPresets", true)
            .Observe("working", bpc.GuagWorking).Observe("policyComp", comp != null).Observe("policyList", string.Join(", ", policies))
            .Observe("listsBothPresets", listed)
            .Check(bpc.GuagWorking && comp != null && listed, "Gear Up and Go did not bind to Better Pawn Control or does not list its Assign presets");
        if (comp == null || !bpc.GuagWorking) { Kit.SkipRemaining(ctx, this, "Gear Up and Go is not bound to Better Pawn Control", CoverageReasons.SetupFailed); yield break; }

        // A preset name that does not exist leaves BPC's preset and the colonist's outfit as they were.
        if (Kit.Require(ctx, "guag.bpc-missing",
                (bpc.Id(bpc.Active) == 0, $"BPC preset 0 is active (is {bpc.Label(bpc.Active)})"),
                (Outfit(pawn) == PeaceOutfit, $"{pawn.LabelShort} has the peace outfit (has {Outfit(pawn)})"),
                (!comp.IsOn(), "no Gear & Go preset switch is in effect")))
        {
            comp.Set(MissingPolicy);
            ctx.Begin("guag.bpc-missing").Expect("activePolicy", bpc.Label(policy0)).Expect("outfit", PeaceOutfit).Expect("switchOn", false)
                .Expect("lastPolicy", "").Subject(pawn)
                .Observe("activePolicy", bpc.Label(bpc.Active)).Observe("outfit", Outfit(pawn))
                .Observe("switchOn", comp.IsOn()).Observe("lastPolicy", comp.lastPolicy).Observe("working", bpc.GuagWorking)
                .Check(bpc.Id(bpc.Active) == 0 && Outfit(pawn) == PeaceOutfit && !comp.IsOn() && comp.lastPolicy == "",
                    "asking for a missing preset changed BPC's preset or the colonist's outfit, or turned the Gear & Go switch on");
        }
        if (comp.IsOn())
        {
            comp.lastPolicy = "";
            ctx.Setup("Developer cleared GearUpPolicyComp.lastPolicy left by the missing-preset case");
        }

        // A change the player made under the previous preset that BPC has not saved yet (no Assign-tab visit since).
        pawn.outfits.CurrentApparelPolicy = changed;
        ctx.Setup($"Developer set {pawn.LabelShort}'s outfit to '{ChangedOutfit}' (duster disallowed) without BPC saving it");

        // Switch to the battle preset, as the right-click menu does, with a differently cased name.
        if (!Kit.Require(ctx, "guag.bpc-switch",
                (bpc.SavedOutfit(pawn, 0) == PeaceOutfit, $"BPC still has the peace outfit, not the unsaved change, under preset 0 (saved {bpc.SavedOutfit(pawn, 0) ?? "nothing"})"),
                (bpc.SavedOutfit(pawn, battleId) == BattleOutfit, $"BPC saved the battle outfit under '{BattlePolicy}' (saved {bpc.SavedOutfit(pawn, battleId) ?? "nothing"})"),
                (bpc.Id(bpc.Active) == 0, "BPC preset 0 is active"),
                (!changed.filter.Allows(dusterDef) && battle.filter.Allows(dusterDef), "only the battle outfit allows the duster")))
        {
            Kit.SkipRemaining(ctx, this, "the BPC presets were not set up", CoverageReasons.SetupFailed);
            yield break;
        }
        string before = bpc.Label(bpc.Active);
        comp.Set(BattleRequest);
        ctx.Begin("guag.bpc-switch").Expect("activePolicy", BattlePolicy).Expect("outfit", BattleOutfit).Expect("switchOn", true).Expect("lastPolicy", before)
            .Subject(pawn)
            .Observe("activePolicy", bpc.Label(bpc.Active)).Observe("outfit", Outfit(pawn)).Observe("switchOn", comp.IsOn())
            .Observe("lastPolicy", comp.lastPolicy).Observe("working", bpc.GuagWorking)
            .Check(bpc.Id(bpc.Active) == battleId && Outfit(pawn) == BattleOutfit && comp.IsOn() && comp.lastPolicy == before,
                "Gear & Go did not switch BPC to the requested preset, apply its outfit or remember the previous preset");

        // Gear up under the battle outfit: the duster is allowed only because of the switch.
        var stockpile = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, ctx.Map.zoneManager);
        ctx.Map.zoneManager.RegisterZone(stockpile);
        stockpile.AddCell(At(5, 2));
        var duster = (Apparel)ThingMaker.MakeThing(dusterDef, GenStuff.DefaultStuffFor(dusterDef));
        GenSpawn.Spawn(duster, At(5, 2), ctx.Map);
        duster.SetForbidden(false, false);
        var destination = At(20, 7);
        var job = JobMaker.MakeJob(GearUpAndGoJobDefOf.GearUpAndGo, destination);
        job.count = 0;
        bool taken = pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
        ctx.Setup($"Developer spawned unforbidden {duster.ThingID} in a one-cell stockpile at {duster.Position} (in storage {duster.IsInAnyStorage()}) " +
            $"and ordered GearUpAndGo to {destination} for {pawn.LabelShort} (accepted {taken})");
        yield return Wait.Until(() => pawn.apparel.WornApparel.Contains(duster) && pawn.Drafted && !pawn.pather.Moving && pawn.Position.DistanceTo(destination) <= 3f, GearTicks);
        bool worn = pawn.apparel.WornApparel.Contains(duster);
        float distance = pawn.Position.DistanceTo(destination);
        ctx.Begin("guag.bpc-gear").Expect("wearingDuster", true).Expect("drafted", true).Expect("distanceToTarget", "<= 3").Subject(pawn)
            .Observe("wearingDuster", worn).Observe("drafted", pawn.Drafted).Observe("distanceToTarget", distance.ToString("0.0"))
            .Observe("outfit", Outfit(pawn)).Observe("activePolicy", bpc.Label(bpc.Active)).Observe("curJob", pawn.CurJobDef?.defName ?? "none")
            .Check(worn && pawn.Drafted && distance <= 3f, "under the battle preset the colonist did not equip the duster, draft and arrive");

        ctx.Remember("pawn", pawn.ThingID);
        ctx.Remember("battleId", battleId.ToString());
        ctx.Remember("before", before);
        yield return Wait.Reload();
    }

    private IEnumerable<Wait> Reloaded(BatchContext ctx, BpcBridge bpc)
    {
        var comp = GearUpPolicyComp.Instance;
        var pawn = ctx.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(p => p.ThingID == ctx.Recall("pawn"));
        int battleId = int.Parse(ctx.Recall("battleId"));
        string before = ctx.Recall("before");
        bool ok = comp != null && pawn != null && comp.IsOn() && comp.lastPolicy == before && bpc.Id(bpc.Active) == battleId && Outfit(pawn) == BattleOutfit;
        ctx.Begin("guag.bpc-reload").Expect("pawnFound", true).Expect("switchOn", true).Expect("lastPolicy", before)
            .Expect("activePolicy", BattlePolicy).Expect("outfit", BattleOutfit)
            .Observe("pawnFound", pawn != null).Observe("switchOn", comp?.IsOn() ?? false).Observe("lastPolicy", comp?.lastPolicy ?? "no comp")
            .Observe("activePolicy", bpc.Label(bpc.Active)).Observe("outfit", pawn == null ? "no pawn" : Outfit(pawn)).Observe("working", bpc.GuagWorking)
            .Check(ok, "the Gear & Go switch, the remembered preset or BPC's active battle preset was lost across save and reload");
        if (comp == null || pawn == null)
        {
            Kit.SkipRemaining(ctx, this, "the colonist or the policy component was not found after reload", CoverageReasons.SetupFailed);
            yield break;
        }

        // Restore, as the alert click does: BPC returns to the previous preset with the outfit the colonist had at the switch, which
        // Gear & Go saved into that preset before switching.
        Command_GearUpAndGo.End();
        bool restored = bpc.Label(bpc.Active) == before && Outfit(pawn) == ChangedOutfit && !comp.IsOn();
        ctx.Begin("guag.bpc-restore").Expect("activePolicy", before).Expect("outfit", ChangedOutfit).Expect("switchOn", false).Subject(pawn)
            .Observe("activePolicy", bpc.Label(bpc.Active)).Observe("outfit", Outfit(pawn)).Observe("switchOn", comp.IsOn())
            .Observe("lastPolicy", comp.lastPolicy).Observe("working", bpc.GuagWorking)
            .Check(restored, "restoring did not return BPC to the previous preset with the outfit the colonist had before the switch, or left the switch on");
    }

    private static bool SetBetterPawnControlActive() =>
        (bool)HarmonyLib.AccessTools.Method("GearUpAndGo.SetBetterPawnControl:Active").Invoke(null, null);
}
