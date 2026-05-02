using System;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace GearUpAndGo
{
	internal static class OutfitStandsPlusIntegration
	{
		private const string packageId = "khamenman.OutfitStandsPLus";
		private static readonly MethodInfo setAllowHaulingMethod =
			typeof(Building_OutfitStand).GetMethod("SetAllowHauling", BindingFlags.NonPublic | BindingFlags.Instance);

		public static bool Active()
		{
			return ModsConfig.ActiveModsInLoadOrder.Any(m =>
				string.Equals(m.PackageIdPlayerFacing, packageId, StringComparison.OrdinalIgnoreCase));
		}

		public static Job TryCreateUseStandJob(Pawn pawn)
		{
			if (!Active() || pawn?.Map == null)
				return null;

			Building_OutfitStand stand = AssignedStand(pawn);
			if (stand == null || !StandHasUsefulGear(stand, pawn))
				return null;

			if (!pawn.CanReserveAndReach(stand, PathEndMode.InteractionCell, Danger.Deadly))
				return null;

			DisableStandHauling(stand);
			return JobMaker.MakeJob(JobDefOf.UseOutfitStand, stand);
		}

		private static Building_OutfitStand AssignedStand(Pawn pawn)
		{
			return pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>()
				.FirstOrDefault(stand =>
				{
					CompAssignableToPawn comp = stand.GetComp<CompAssignableToPawn>();
					return comp?.AssignedPawnsForReading?.Contains(pawn) == true;
				});
		}

		private static bool StandHasUsefulGear(Building_OutfitStand stand, Pawn pawn)
		{
			return stand.HeldItems.Any(thing => IsUsefulApparel(thing as Apparel, pawn)) ||
				IsUsefulWeapon(stand.HeldWeapon, pawn);
		}

		private static bool IsUsefulApparel(Apparel apparel, Pawn pawn)
		{
			if (apparel == null || pawn.apparel == null || pawn.outfits?.CurrentApparelPolicy?.filter == null)
				return false;

			if (!pawn.outfits.CurrentApparelPolicy.filter.Allows(apparel))
				return false;

			if (pawn.apparel.WornApparel.Any(worn => worn.def == apparel.def))
				return false;

			return pawn.apparel.WornApparel.All(worn =>
				worn == apparel || ApparelUtility.CanWearTogether(apparel.def, worn.def, pawn.RaceProps.body));
		}

		private static bool IsUsefulWeapon(ThingWithComps weapon, Pawn pawn)
		{
			if (weapon == null || pawn.equipment == null || pawn.equipment.Primary?.def == weapon.def)
				return false;

			return EquipmentUtility.CanEquip(weapon, pawn, out string _);
		}

		private static void DisableStandHauling(Building_OutfitStand stand)
		{
			if (setAllowHaulingMethod == null)
				return;

			try
			{
				setAllowHaulingMethod.Invoke(stand, new object[] { false });
			}
			catch (Exception e)
			{
				Verse.Log.Warning($"Gear Up and Go could not disable hauling on outfit stand before use: {e}");
			}
		}
	}
}
