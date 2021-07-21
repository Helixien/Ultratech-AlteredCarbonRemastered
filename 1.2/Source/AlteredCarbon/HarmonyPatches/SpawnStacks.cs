using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(CompRottable), "Stage", MethodType.Getter)]
    internal static class SpawnStacks_Patch
    {
        public static void Postfix(CompRottable __instance, RotStage __result)
        {
            if (__result == RotStage.Dessicated && __instance.parent is Corpse corpse
                && (corpse.InnerPawn?.health?.hediffSet?.HasHediff(AlteredCarbonDefOf.AC_CorticalStack) ?? true))
            {
                try
                {
                    var corticalStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_FilledCorticalStack) as CorticalStack;
                    corticalStack.SavePawnToCorticalStack(corpse.InnerPawn);
                    GenPlace.TryPlaceThing(corticalStack, corpse.Position, corpse.Map, ThingPlaceMode.Near);
                    corpse.InnerPawn.health.hediffSet.hediffs.RemoveAll(x => x.def == AlteredCarbonDefOf.AC_CorticalStack);
                }
                catch { }
            }
        }
    }
    [HarmonyPatch(typeof(Fire), "DoFireDamage")]
    internal static class DoFireDamage_Patch
    {
        public static void Prefix(Fire __instance, Thing targ)
        {
            if (targ is Corpse corpse && targ.HitPoints <= 3 && (corpse.InnerPawn?.health?.hediffSet?.HasHediff(AlteredCarbonDefOf.AC_CorticalStack) ?? true))
            {
                var corticalStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_FilledCorticalStack) as CorticalStack;
                corticalStack.SavePawnToCorticalStack(corpse.InnerPawn);
                GenPlace.TryPlaceThing(corticalStack, corpse.Position, corpse.Map, ThingPlaceMode.Direct);
                corpse.InnerPawn.health.hediffSet.hediffs.RemoveAll(x => x.def == AlteredCarbonDefOf.AC_CorticalStack);
                __instance.Destroy(DestroyMode.Vanish);
            }
            else if (targ is Pawn pawn && pawn.health.summaryHealth.SummaryHealthPercent < 0.001f
                && (pawn.health?.hediffSet?.HasHediff(AlteredCarbonDefOf.AC_CorticalStack) ?? true))
            {
                var corticalStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_FilledCorticalStack) as CorticalStack;
                corticalStack.SavePawnToCorticalStack(pawn);
                GenPlace.TryPlaceThing(corticalStack, pawn.Position, pawn.Map, ThingPlaceMode.Direct);
                pawn.health.hediffSet.hediffs.RemoveAll(x => x.def == AlteredCarbonDefOf.AC_CorticalStack);
                __instance.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
