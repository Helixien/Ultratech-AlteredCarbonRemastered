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
		public static List<ThingDef> unstackableRaces = InitCache();
		static List<ThingDef> InitCache()
		{
			if (ModCompatibility.AlienRacesIsActive)
			{
				List<ThingDef> excludedRaces = new List<ThingDef>();
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
				return new List<ThingDef>();
            }
		}
		private static bool Prefix(RecipeDef __instance, Thing thing, ref bool __result)
		{
			if (__instance == AlteredCarbonDefOf.AC_InstallEmptyCorticalStack && thing is Pawn pawn)
			{
				if (unstackableRaces.Contains(pawn.def) || pawn.IsEmptySleeve())
				{
					__result = false;
					return false;
				}
			}
			else if (__instance == AlteredCarbonDefOf.AC_InstallCorticalStack && thing is Pawn pawn2)
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
}

