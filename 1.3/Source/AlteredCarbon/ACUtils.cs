using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{

	[StaticConstructorOnStartup]
	internal static class ACUtils
	{
        static ACUtils()
        {
            foreach (var li in AC_DefOf.UT_HackBiocodedThings.ingredients)
            {
                li.filter = new ThingFilterBiocodable();
                var list = Traverse.Create(li.filter).Field("thingDefs").GetValue<List<ThingDef>>();
                if (list is null)
                {
                    list = new List<ThingDef>();
                    Traverse.Create(li.filter).Field("thingDefs").SetValue(list);
                }
                foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
                {
                    li.filter.SetAllow(thingDef, true);
                    list.Add(thingDef);
                }
            }
            AC_DefOf.UT_HackBiocodedThings.fixedIngredientFilter = new ThingFilterBiocodable();
            var list2 = Traverse.Create(AC_DefOf.UT_HackBiocodedThings.fixedIngredientFilter).Field("thingDefs").GetValue<List<ThingDef>>();
            if (list2 is null)
            {
                list2 = new List<ThingDef>();
                Traverse.Create(AC_DefOf.UT_HackBiocodedThings.fixedIngredientFilter).Field("thingDefs").SetValue(list2);
            }

            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
            {
                list2.Add(thingDef);
                AC_DefOf.UT_HackBiocodedThings.fixedIngredientFilter.SetAllow(thingDef, true);
            }


            AC_DefOf.UT_HackBiocodedThings.defaultIngredientFilter = new ThingFilterBiocodable();
            var list3 = Traverse.Create(AC_DefOf.UT_HackBiocodedThings.defaultIngredientFilter).Field("thingDefs").GetValue<List<ThingDef>>();
            if (list3 is null)
            {
                list3 = new List<ThingDef>();
                Traverse.Create(AC_DefOf.UT_HackBiocodedThings.defaultIngredientFilter).Field("thingDefs").SetValue(list2);
            }

            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
            {
                list3.Add(thingDef);
                AC_DefOf.UT_HackBiocodedThings.defaultIngredientFilter.SetAllow(thingDef, true);
            }

            foreach (var li in AC_DefOf.UT_WipeFilledCorticalStack.ingredients)
            {
                li.filter.SetAllow(AC_DefOf.UT_AllowStacksColonist, false);
            }
            AC_DefOf.UT_WipeFilledCorticalStack.defaultIngredientFilter.SetAllow(AC_DefOf.UT_AllowStacksColonist, false);


            foreach (var li in AC_DefOf.UT_InstallCorticalStack.ingredients)
            {
                li.filter.SetAllow(AC_DefOf.UT_AllowStacksColonist, true);
                li.filter.SetAllow(AC_DefOf.UT_AllowStacksStranger, true);
                li.filter.SetAllow(AC_DefOf.UT_AllowStacksHostile, true);
            }

            AC_DefOf.UT_InstallCorticalStack.fixedIngredientFilter.SetAllow(AC_DefOf.UT_AllowStacksColonist, true);
            AC_DefOf.UT_InstallCorticalStack.fixedIngredientFilter.SetAllow(AC_DefOf.UT_AllowStacksStranger, true);
            AC_DefOf.UT_InstallCorticalStack.fixedIngredientFilter.SetAllow(AC_DefOf.UT_AllowStacksHostile, true);

            AC_DefOf.UT_InstallCorticalStack.defaultIngredientFilter.SetAllow(AC_DefOf.UT_AllowStacksColonist, true);
            AC_DefOf.UT_InstallCorticalStack.defaultIngredientFilter.SetAllow(AC_DefOf.UT_AllowStacksStranger, true);
            AC_DefOf.UT_InstallCorticalStack.defaultIngredientFilter.SetAllow(AC_DefOf.UT_AllowStacksHostile, true);
        }

        public static bool IsUltraTech(this Thing thing)
        {
            return thing.def == AC_DefOf.UT_SleeveIncubator || thing.def == AC_DefOf.UT_OrganIncubator
                || thing.def == AC_DefOf.UT_SleeveCasket || thing.def == AC_DefOf.UT_SleeveCasket
                || thing.def == AC_DefOf.UT_CorticalStackStorage || thing.def == AC_DefOf.UT_DecryptionBench;
        }
        public static bool IsCopy(this Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
            if (hediff != null && AlteredCarbonManager.Instance.stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out var stackData))
            {
                if (stackData.originalPawn != null && pawn != stackData.originalPawn)
                {
                    return true;
                }
                if (stackData.copiedPawns != null)
                {
                    foreach (var copiedPawn in stackData.copiedPawns)
                    {
                        if (pawn == copiedPawn)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (AlteredCarbonManager.Instance.stacksRelationships != null)
            {
                foreach (var stackGroup in AlteredCarbonManager.Instance.stacksRelationships)
                {
                    if (stackGroup.Value.copiedPawns != null)
                    {
                        foreach (var copiedPawn in stackGroup.Value.copiedPawns)
                        {
                            if (pawn == copiedPawn)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsEmptySleeve(this Pawn pawn)
        {
            if (AlteredCarbonManager.Instance.emptySleeves != null && AlteredCarbonManager.Instance.emptySleeves.Contains(pawn))
            {
                return true;
            }
            return false;
        }

        public static bool HasStack(this Pawn pawn)
        {
            return AlteredCarbonManager.Instance.stacksIndex.ContainsKey(pawn.thingIDNumber) || AlteredCarbonManager.Instance.pawnsWithStacks.Contains(pawn);
        }
	}
}

