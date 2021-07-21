using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class TryGiveJob_Patch
    {
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result is null && pawn.Faction != Faction.OfPlayer)
            {
                var jbg = new JobGiver_TakeStackWhenClose();
                var result = jbg.TryIssueJobPackage(pawn, default(JobIssueParams));
                if (result.Job != null)
                {
                    __result = result.Job;
                }
            }
        }
    }
}

