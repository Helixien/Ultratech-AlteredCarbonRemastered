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
    [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
    public static class InitDesignators_Patch
    {
        public static void Postfix(ref List<Designator> ___desList)
        {
            ___desList.Add(new Designator_ExtractStack());
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Character), "PawnToShowInfoAbout", MethodType.Getter)]
    public static class ITab_Pawn_Character_PawnToShowInfoAbout_Patch
    {
        public static bool Prefix(ref Pawn __result)
        {
            if (Find.Selector.SingleSelectedThing is CorticalStack stack && stack.PersonaData.hasPawn)
            {
                __result = stack.PersonaData.GetDummyPawn;
                return false;
            }
            return true;
        }
    }
}

