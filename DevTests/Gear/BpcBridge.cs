using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GearUpAndGo.DevTests;

// Better Pawn Control Forked's Assign manager and Gear Up and Go's SetBetterPawnControl are both internal, and this project has
// no compile reference to BPC, so everything is reached by reflection. Create() is called only inside the BPC batch's segments,
// so the BPC-free mod set never touches these types.
internal sealed class BpcBridge
{
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private Type policyType;
    private PropertyInfo policiesProp, linksProp;
    private MethodInfo getActive, saveState, loadStateFull;
    private FieldInfo policyId, policyLabel, linkZone, linkColonist, linkOutfit;
    private FieldInfo guagWorking;
    private MethodInfo guagPolicyList;

    public static BpcBridge Create(out string missing)
    {
        var b = new BpcBridge();
        var gaps = new List<string>();
        T Need<T>(T member, string name) where T : class
        {
            if (member == null) gaps.Add(name);
            return member;
        }

        var manager = Need(AccessTools.TypeByName("BetterPawnControlForked.AssignManager"), "BetterPawnControlForked.AssignManager");
        var link = Need(AccessTools.TypeByName("BetterPawnControlForked.AssignLink"), "BetterPawnControlForked.AssignLink");
        var baseLink = Need(AccessTools.TypeByName("BetterPawnControlForked.Link"), "BetterPawnControlForked.Link");
        b.policyType = Need(AccessTools.TypeByName("BetterPawnControlForked.Policy"), "BetterPawnControlForked.Policy");
        var guag = Need(AccessTools.TypeByName("GearUpAndGo.SetBetterPawnControl"), "GearUpAndGo.SetBetterPawnControl");
        if (gaps.Count == 0)
        {
            var assignBase = AccessTools.TypeByName("BetterPawnControlForked.Manager`1")?.MakeGenericType(link);
            b.policiesProp = Need(assignBase?.GetProperty("policies", Static), "Manager<AssignLink>.policies");
            b.linksProp = Need(assignBase?.GetProperty("links", Static), "Manager<AssignLink>.links");
            b.getActive = Need(assignBase?.GetMethod("GetActivePolicy", Static, null, Type.EmptyTypes, null), "Manager<AssignLink>.GetActivePolicy()");
            b.saveState = Need(manager.GetMethod("SaveCurrentState", Static, null, new[] { typeof(List<Pawn>) }, null), "AssignManager.SaveCurrentState");
            var listOfLinks = typeof(List<>).MakeGenericType(link);
            b.loadStateFull = Need(manager.GetMethod("LoadState", Static, null, new[] { listOfLinks, typeof(List<Pawn>), b.policyType }, null), "AssignManager.LoadState(links, pawns, policy)");
            b.policyId = Need(AccessTools.Field(b.policyType, "id"), "Policy.id");
            b.policyLabel = Need(AccessTools.Field(b.policyType, "label"), "Policy.label");
            b.linkZone = Need(AccessTools.Field(baseLink, "zone"), "Link.zone");
            b.linkColonist = Need(AccessTools.Field(link, "colonist"), "AssignLink.colonist");
            b.linkOutfit = Need(AccessTools.Field(link, "outfit"), "AssignLink.outfit");
            b.guagWorking = Need(AccessTools.Field(guag, "working"), "SetBetterPawnControl.working");
            b.guagPolicyList = Need(AccessTools.Method(guag, "PolicyList"), "SetBetterPawnControl.PolicyList");
        }
        missing = string.Join(", ", gaps);
        return gaps.Count == 0 ? b : null;
    }

    public IList Policies => (IList)policiesProp.GetValue(null);
    private IList Links => (IList)linksProp.GetValue(null);
    public object Active => getActive.Invoke(null, null);
    public int Id(object policy) => (int)policyId.GetValue(policy);
    public string Label(object policy) => policy == null ? "none" : (string)policyLabel.GetValue(policy);
    public object Find(int id) => Policies.Cast<object>().FirstOrDefault(p => Id(p) == id);

    public bool GuagWorking => (bool)guagWorking.GetValue(null);
    public List<string> GuagPolicyList() => (List<string>)guagPolicyList.Invoke(null, null);

    public object AddPolicy(string label)
    {
        int id = Policies.Cast<object>().Max(Id) + 1;
        var policy = Activator.CreateInstance(policyType, id, label);
        Policies.Add(policy);
        return policy;
    }

    // The Assign tab's active-policy button saves the current state, then the chosen menu option loads the policy.
    public void SelectPolicy(List<Pawn> pawns, object policy)
    {
        saveState.Invoke(null, new object[] { pawns });
        loadStateFull.Invoke(null, new[] { Links, pawns, policy });
    }

    // The outfit BPC has saved for this pawn under the given policy, or null if no link exists.
    public string SavedOutfit(Pawn pawn, int zone)
    {
        foreach (var link in Links)
            if (link != null && (int)linkZone.GetValue(link) == zone && linkColonist.GetValue(link) == pawn)
                return ((ApparelPolicy)linkOutfit.GetValue(link))?.label ?? "null outfit";
        return null;
    }
}
