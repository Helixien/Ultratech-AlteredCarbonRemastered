using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Hediff), "PostAdd")]
    public static class Hediff_PostAdd_Patch
    {
        public static void Prefix(Hediff __instance, DamageInfo? dinfo)
        {
            if (__instance.Part?.def == BodyPartDefOf.Neck && __instance is Hediff_MissingPart)
            {
                var stackHediff = __instance.pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == AC_DefOf.AC_CorticalStack) as Hediff_CorticalStack;
                if (stackHediff != null)
                {
                    stackHediff.TryRecoverOrSpawnOnGround();
                    if (!__instance.pawn.Dead)
                    {
                        __instance.pawn.Kill(null);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange", null)]
    public static class CheckForStateChange_Patch
    {
        private static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff)
        {
            if (!___pawn.health.hediffSet.GetNotMissingParts().Any(x => x.def == BodyPartDefOf.Neck))
            {
                var stackHediff = ___pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == AC_DefOf.AC_CorticalStack) as Hediff_CorticalStack;
                if (stackHediff != null)
                {
                    stackHediff.TryRecoverOrSpawnOnGround();
                    if (!___pawn.Dead)
                    {
                        ___pawn.Kill(null);
                    }
                }
            }
        }
    }
}

