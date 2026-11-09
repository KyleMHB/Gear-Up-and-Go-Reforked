using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorldDevTests;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GearUpAndGo.DevTests;

// Expected behaviour (README "Features"/"Usage"): the Gear & Go command drafts the selected colonist, equips gear allowed by
// the active outfit policy, then sends them to the chosen location; repeated gear jobs stop instead of cycling indefinitely.
// Mechanical setup: the job the command issues (GearUpAndGo, count 0, DraftedOrder) is ordered directly; the gizmo and the
// targeter are not clicked. Better Pawn Control is not loaded, so no policy switch takes place.
public class GearBatch : Batch
{
    private const string HarmonyId = "uuugggg.rimworld.GearUpAndGo.main";
    private const int IterationCap = 16; // GearUpJobPlanner.MaxIterations (internal).
    private const int GearTicks = 6000;

    private static readonly string[] TextKeys = { "TD.GearAndGo", "TD.GearAndGoDesc", "TD.GearUpAndGoSettingsName", "TD.GearUpPolicySetAlert" };
    private static readonly string[] Textures = { "CommandGearUpAndGo", "CommandGearUpAndGoActive" };

    public override string Id => "guag-gear";
    public override int SegmentCount => 2;
    public override int MapSize => 75;
    public override IReadOnlyList<PlannedCase> Plan { get; } = new[]
    {
        "guag.defs", "guag.gizmo", "guag.gear-and-go", "guag.iteration-cap", "guag.reload",
    }.Select(id => new PlannedCase(id, Levels.Mechanical)).ToArray();

    public override IEnumerable<Wait> Segment(int index, BatchContext ctx)
    {
        if (index == 1)
        {
            Reloaded(ctx);
            yield break;
        }

        ctx.Setup($"{ModsConfig.ActiveModsInLoadOrder.Count()} active mods; Better Pawn Control active {ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageIdPlayerFacing.ToLowerInvariant().Contains("betterpawncontrol"))}");

        // Defs, text, art, Harmony patch and game component.
        var jobDef = GearUpAndGoJobDefOf.GearUpAndGo;
        var missingText = TextKeys.Where(k => !k.CanTranslate()).ToList();
        var missingTex = Textures.Where(p => ContentFinder<Texture2D>.Get(p, false) == null).ToList();
        var gizmoPatch = Harmony.GetPatchInfo(AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)))?.Postfixes.Any(p => p.owner == HarmonyId) == true;
        var comp = Current.Game.GetComponent<GearUpPolicyComp>();
        bool defsOk = jobDef?.driverClass == typeof(JobDriver_GearUpAndGo) && missingText.Count == 0 && missingTex.Count == 0 && gizmoPatch && comp != null;
        ctx.Begin("guag.defs").Expect("jobDriver", nameof(JobDriver_GearUpAndGo)).Expect("missingKeys", "none").Expect("missingTextures", "none")
            .Expect("gizmoPatch", true).Expect("policyComp", true)
            .Observe("jobDriver", jobDef?.driverClass?.Name ?? "unbound")
            .Observe("missingKeys", missingText.Count == 0 ? "none" : string.Join(", ", missingText))
            .Observe("missingTextures", missingTex.Count == 0 ? "none" : string.Join(", ", missingTex))
            .Observe("gizmoPatch", gizmoPatch).Observe("policyComp", comp != null)
            .Check(defsOk, "the job def, a text key, a texture, the GetGizmos patch or the policy game component is missing");

        Kit.RemoveStartingColonists(ctx);
        var area = Kit.Area(ctx, 24, 10);
        IntVec3 At(int x, int z) => area.Min + new IntVec3(x, 0, z);

        // A naked colonist next to a single unforbidden duster; the default outfit policy allows it.
        var pawn = Kit.Worker(ctx, At(2, 2), WorkTypeDefOf.Hauling);
        pawn.apparel.DestroyAll();
        var dusterDef = Kit.Def("Apparel_Duster");
        var duster = (Apparel)ThingMaker.MakeThing(dusterDef, GenStuff.DefaultStuffFor(dusterDef));
        // JobGiver_OptimizeApparel (1.7) only considers apparel for which StoreUtility.IsInAnyStorage is true, so the duster sits in a stockpile.
        var stockpile = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, ctx.Map.zoneManager);
        ctx.Map.zoneManager.RegisterZone(stockpile);
        stockpile.AddCell(At(5, 2));
        GenSpawn.Spawn(duster, At(5, 2), ctx.Map);
        duster.SetForbidden(false, false);
        ctx.Setup($"Developer removed {pawn.LabelShort}'s apparel and spawned unforbidden {duster.ThingID} at {duster.Position} in a one-cell default stockpile (in storage {duster.IsInAnyStorage()}); outfit policy {pawn.outfits?.CurrentApparelPolicy?.label ?? "none"}");

        var gizmo = pawn.GetGizmos().OfType<Command_GearUpAndGo>().Any();
        ctx.Begin("guag.gizmo").Expect("commandShown", true).Subject(pawn).Observe("commandShown", gizmo)
            .Check(gizmo, "a player-controlled colonist has no Gear & Go command");

        // Gear up, then go: the duster must be worn before the pawn is drafted and sent to the destination.
        var destination = At(20, 7);
        int wornAt = -1, draftedAt = -1;
        var job = JobMaker.MakeJob(GearUpAndGoJobDefOf.GearUpAndGo, destination);
        job.count = 0;
        bool taken = pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
        ctx.Setup($"Developer ordered GearUpAndGo to {destination} for {pawn.LabelShort} (accepted {taken})");
        yield return Wait.Until(() => wornAt >= 0 && draftedAt >= 0 && !pawn.pather.Moving && pawn.Position.DistanceTo(destination) <= 3f, GearTicks, () =>
        {
            if (wornAt < 0 && pawn.apparel.WornApparel.Contains(duster)) wornAt = ctx.Now;
            if (draftedAt < 0 && pawn.Drafted) draftedAt = ctx.Now;
        });
        bool worn = pawn.apparel.WornApparel.Contains(duster);
        float distance = pawn.Position.DistanceTo(destination);
        bool order = wornAt >= 0 && draftedAt >= wornAt;
        ctx.Begin("guag.gear-and-go").Expect("wearingDuster", true).Expect("drafted", true).Expect("gearedBeforeDraft", true).Expect("distanceToTarget", "<= 3")
            .Subject(pawn)
            .Observe("wearingDuster", worn).Observe("drafted", pawn.Drafted).Observe("gearedBeforeDraft", order)
            .Observe("distanceToTarget", distance.ToString("0.0")).Observe("wornTick", wornAt).Observe("draftedTick", draftedAt)
            .Observe("curJob", pawn.CurJobDef?.defName ?? "none")
            .Check(worn && pawn.Drafted && order && distance <= 3f, "the pawn did not equip the duster, draft and arrive at the destination in that order");

        // A gear-up pass at the iteration cap must stop: no draft, no ordered goto and no requeued GearUpAndGo job.
        // The undrafted pawn may wander afterwards, so movement itself is not the measurement.
        var capped = Kit.Worker(ctx, At(2, 8), WorkTypeDefOf.Hauling);
        var capTarget = At(20, 2);
        var capJob = JobMaker.MakeJob(GearUpAndGoJobDefOf.GearUpAndGo, capTarget);
        capJob.count = IterationCap;
        capped.jobs.TryTakeOrderedJob(capJob, JobTag.DraftedOrder);
        ctx.Setup($"Developer ordered GearUpAndGo with count {IterationCap} for {capped.LabelShort}");
        bool everDrafted = false, everGoto = false;
        yield return Wait.Ticks(120, () =>
        {
            everDrafted |= capped.Drafted;
            everGoto |= capped.CurJobDef == JobDefOf.Goto && capped.CurJob.targetA.Cell.DistanceTo(capTarget) <= 3f;
        });
        bool queued = capped.CurJobDef == GearUpAndGoJobDefOf.GearUpAndGo || capped.jobs.jobQueue.Any(q => q.job?.def == GearUpAndGoJobDefOf.GearUpAndGo);
        ctx.Begin("guag.iteration-cap").Expect("everDrafted", false).Expect("gearUpJobRemaining", false).Expect("gotoTarget", false).Subject(capped)
            .Observe("everDrafted", everDrafted).Observe("gearUpJobRemaining", queued).Observe("gotoTarget", everGoto)
            .Observe("curJob", capped.CurJobDef?.defName ?? "none")
            .Check(!everDrafted && !queued && !everGoto, "a gear-up pass at the iteration cap kept running, drafted the pawn or sent it to the target");

        ctx.Remember("pawn", pawn.ThingID);
        ctx.Remember("duster", duster.ThingID);
        yield return Wait.Reload();
    }

    private static void Reloaded(BatchContext ctx)
    {
        var pawn = ctx.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(p => p.ThingID == ctx.Recall("pawn"));
        bool wearing = pawn?.apparel?.WornApparel.Any(a => a.ThingID == ctx.Recall("duster")) == true;
        bool comp = Current.Game.GetComponent<GearUpPolicyComp>() != null;
        ctx.Begin("guag.reload").Expect("pawnFound", true).Expect("wearingDuster", true).Expect("policyComp", true)
            .Observe("pawnFound", pawn != null).Observe("wearingDuster", wearing).Observe("drafted", pawn?.Drafted ?? false).Observe("policyComp", comp)
            .Check(pawn != null && wearing && comp, "the geared pawn, its duster or the policy component was lost across save and reload");
    }
}
