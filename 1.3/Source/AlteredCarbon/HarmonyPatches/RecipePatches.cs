using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	[HarmonyPatch(typeof(RecipeDef), "AvailableOnNow")]
	public static class AvailableOnNow_Patch
	{
		public static HashSet<ThingDef> unstackableRaces = InitCache();
		static HashSet<ThingDef> InitCache()
		{
			if (ModCompatibility.AlienRacesIsActive)
			{
				HashSet<ThingDef> excludedRaces = new HashSet<ThingDef>();
				foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
				{
					if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
					{
						if (!props.acceptsStacks)
						{
							excludedRaces.Add(def);
						}
					}
				}
				return excludedRaces;
			}
			else
            {
				return new HashSet<ThingDef>();
            }
		}
		private static bool Prefix(RecipeDef __instance, Thing thing, ref bool __result)
		{
			if (__instance == AC_DefOf.UT_InstallEmptyCorticalStack && thing is Pawn pawn)
			{
				if (unstackableRaces.Contains(pawn.def) || pawn.IsEmptySleeve())
				{
					__result = false;
					return false;
				}
			}

			else if (__instance == AC_DefOf.UT_InstallCorticalStack && thing is Pawn pawn2)
            {
				if (unstackableRaces.Contains(pawn2.def))
				{
					__result = false;
					return false;
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Bill), "IsFixedOrAllowedIngredient", new Type[] { typeof(Thing) })]
	public static class IsFixedOrAllowedIngredient_Patch
	{
		private static bool Prefix(ref bool __result, Bill __instance, Thing thing)
		{
			if (__instance is Bill_InstallStack installStack && thing is CorticalStack stack)
            {
				if (stack != installStack.stackToInstall)
				{
					__result = false;
				}
				else
				{
					__result = true;
				}
				return false;
			}
			return true;
		}
	}
}

