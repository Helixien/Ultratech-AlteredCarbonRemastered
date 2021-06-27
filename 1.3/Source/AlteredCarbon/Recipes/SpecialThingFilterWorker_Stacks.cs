using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{

	public abstract class SpecialThingFilterWorker_Stacks : SpecialThingFilterWorker
	{
        public override bool AlwaysMatches(ThingDef def)
        {
			return def == AC_DefOf.UT_AncientStack || def == AC_DefOf.UT_FilledCorticalStack || def == AC_DefOf.UT_EmptyCorticalStack;
		}

        public override bool Matches(Thing t)
        {
			if (t.def == AC_DefOf.UT_AncientStack)
			{
				return false;
			}
			return true;
		}
        public override bool CanEverMatch(ThingDef def)
        {
			return def == AC_DefOf.UT_AncientStack || def == AC_DefOf.UT_FilledCorticalStack || def == AC_DefOf.UT_EmptyCorticalStack;
		}
	}
}

