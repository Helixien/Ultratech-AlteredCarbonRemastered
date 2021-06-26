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
        public static bool IsCopy(this Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_CorticalStack) as Hediff_CorticalStack;
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

