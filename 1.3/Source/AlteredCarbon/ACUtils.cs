using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace AlteredCarbon
{

    [StaticConstructorOnStartup]
    internal static class ACUtils
    {
        public static RecipeDef UT_ConvertFilledCorticalStackToIdeo;
        static ACUtils()
        {
            if (ModsConfig.IdeologyActive)
            {
                UT_ConvertFilledCorticalStackToIdeo = DefDatabase<RecipeDef>.GetNamed("UT_ConvertFilledCorticalStackToIdeo");
            }
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
            return AlteredCarbonManager.Instance.stacksIndex.ContainsKey(pawn.thingIDNumber) || AlteredCarbonManager.Instance.PawnsWithStacks.Contains(pawn);
        }

        public static Hediff MakeHediff(HediffDef hediffDef, Pawn pawn, BodyPartRecord part)
        {
            if (ModCompatibility.RimJobWorldIsActive)
            {
                return rjw.SexPartAdder.MakePart(hediffDef, pawn, part);
            }
            return HediffMaker.MakeHediff(hediffDef, pawn, part);
        }

        public static void Resurrect(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                Log.Error("Tried to resurrect a pawn who is not dead: " + pawn.ToStringSafe());
                return;
            }
            if (pawn.Discarded)
            {
                Log.Error("Tried to resurrect a discarded pawn: " + pawn.ToStringSafe());
                return;
            }
            Corpse corpse = pawn.Corpse;
            bool flag = false;
            IntVec3 loc = IntVec3.Invalid;
            Map map = null;
            if (corpse != null)
            {
                flag = corpse.Spawned;
                loc = corpse.Position;
                map = corpse.Map;
                corpse.InnerPawn = null;
                corpse.Destroy();
            }
            if (flag && pawn.IsWorldPawn())
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            pawn.ForceSetStateToUnspawned();
            Traverse.Create(pawn.health).Field("healthState").SetValue(PawnHealthState.Mobile);
            if (flag)
            {
                GenSpawn.Spawn(pawn, loc, map);
                if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
                {
                    LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction), pawn.Map, Gen.YieldSingle(pawn));
                }
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int j = 0; j < wornApparel.Count; j++)
                    {
                        wornApparel[j].Notify_PawnResurrected();
                    }
                }
            }

            if (pawn.royalty != null)
            {
                pawn.royalty.Notify_Resurrected();
            }
        }
    }
}

