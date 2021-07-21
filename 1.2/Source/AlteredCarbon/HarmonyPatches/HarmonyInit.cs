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
}

