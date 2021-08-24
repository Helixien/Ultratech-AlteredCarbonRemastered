using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static Harmony harmonyInstance;
        static HarmonyInit()
        {
            harmonyInstance = new Harmony("Altered.Carbon");
            harmonyInstance.PatchAll();
        }
    }

    //HarmonyPatch(typeof(MemoryThoughtHandler), "RemoveMemory")]
    //ublic static class RemoveMemory_Patch
    //
    //   public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory th)
    //   {
    //       Log.Message("Removing " + th.def + " from " + __instance.pawn);
    //   }
    //
    //
    //HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new Type[]
    //
    //   typeof(Thought_Memory),
    //   typeof(Pawn)
    //)]
    //ublic static class TryGainMemory_Patch
    //
    //   private static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought, Pawn otherPawn)
    //   {
    //       Log.Message("Adding " + newThought.def + " to " + __instance.pawn);
    //   }
    //
    //
    //HarmonyPatch(typeof(IndividualThoughtToAdd), "Add")]
    //ublic static class IndividualThoughtToAdd_Patch
    //
    //   public static void Postfix(IndividualThoughtToAdd __instance)
    //   {
    //       Log.Message("2 Adding " + __instance.thought.def + " to " + __instance.addTo);
    //   }
    //
}

