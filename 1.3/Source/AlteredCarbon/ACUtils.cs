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
            Log.Message(" - Resurrect - if (!pawn.Dead) - 1", true);
            if (!pawn.Dead)
            {
                Log.Message(" - Resurrect - Log.Error(\"Tried to resurrect a pawn who is not dead: \" + pawn.ToStringSafe()); - 2", true);
                Log.Error("Tried to resurrect a pawn who is not dead: " + pawn.ToStringSafe());
                Log.Message(" - Resurrect - return; - 3", true);
                return;
            }
            Log.Message(" - Resurrect - if (pawn.Discarded) - 4", true);
            if (pawn.Discarded)
            {
                Log.Message(" - Resurrect - Log.Error(\"Tried to resurrect a discarded pawn: \" + pawn.ToStringSafe()); - 5", true);
                Log.Error("Tried to resurrect a discarded pawn: " + pawn.ToStringSafe());
                Log.Message(" - Resurrect - return; - 6", true);
                return;
            }
            Corpse corpse = pawn.Corpse;
            Log.Message(" - Resurrect - bool flag = false; - 8", true);
            bool flag = false;
            Log.Message(" - Resurrect - IntVec3 loc = IntVec3.Invalid; - 9", true);
            IntVec3 loc = IntVec3.Invalid;
            Log.Message(" - Resurrect - Map map = null; - 10", true);
            Map map = null;
            Log.Message(" - Resurrect - if (corpse != null) - 11", true);
            if (corpse != null)
            {
                Log.Message(" - Resurrect - flag = corpse.Spawned; - 12", true);
                flag = corpse.Spawned;
                Log.Message(" - Resurrect - loc = corpse.Position; - 13", true);
                loc = corpse.Position;
                Log.Message(" - Resurrect - map = corpse.Map; - 14", true);
                map = corpse.Map;
                Log.Message(" - Resurrect - corpse.InnerPawn = null; - 15", true);
                corpse.InnerPawn = null;
                Log.Message(" - Resurrect - corpse.Destroy(); - 16", true);
                corpse.Destroy();
            }
            Log.Message(" - Resurrect - if (flag && pawn.IsWorldPawn()) - 17", true);
            if (flag && pawn.IsWorldPawn())
            {
                Log.Message(" - Resurrect - Find.WorldPawns.RemovePawn(pawn); - 18", true);
                Find.WorldPawns.RemovePawn(pawn);
            }
            pawn.ForceSetStateToUnspawned();

            if (flag)
            {
                Log.Message(" - Resurrect - GenSpawn.Spawn(pawn, loc, map); - 25", true);
                GenSpawn.Spawn(pawn, loc, map);
                Log.Message(" - Resurrect - if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer)) - 26", true);
                if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
                {
                    Log.Message(" - Resurrect - LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction), pawn.Map, Gen.YieldSingle(pawn)); - 27", true);
                    LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction), pawn.Map, Gen.YieldSingle(pawn));
                }
                Log.Message(" - Resurrect - if (pawn.apparel != null) - 28", true);
                if (pawn.apparel != null)
                {
                    Log.Message(" - Resurrect - List<Apparel> wornApparel = pawn.apparel.WornApparel; - 29", true);
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int j = 0; j < wornApparel.Count; j++)
                    {
                        Log.Message(" - Resurrect - wornApparel[j].Notify_PawnResurrected(); - 30", true);
                        wornApparel[j].Notify_PawnResurrected();
                    }
                }
            }

            Log.Message(" - Resurrect - if (pawn.royalty != null) - 31", true);
            if (pawn.royalty != null)
            {
                Log.Message(" - Resurrect - pawn.royalty.Notify_Resurrected(); - 32", true);
                pawn.royalty.Notify_Resurrected();
            }
        }
    }
}

