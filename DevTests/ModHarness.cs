using System;
using System.Collections.Generic;
using RimWorldDevTests;

namespace GearUpAndGo.DevTests;

public sealed class ModHarness : HarnessSettings
{
    public override string DisplayName => "Gear Up and Go Reforked";
    public override string ArgPrefix => "guag";
    public override string PackageId => "kylemhb.gearupandgo";
    public override Type ProductionType => typeof(global::GearUpAndGo.Mod);
    public override string EvidenceFolder => "GearUpAndGoReforkedTests";
    public override string[] RuntimeFolders => new[] { "About", "Assemblies", "Defs", "Languages", "Textures" };
    public override string[] RuntimeFiles => Array.Empty<string>();
    public override IEnumerable<Batch> CreateBatches() => new Batch[] { new GearBatch() };
    public override IEnumerable<string> RunAll => new[] { "guag-gear" };
}
