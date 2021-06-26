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
	public class SpecialThingFilterWorker_StacksStranger : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			Log.Message("T: " + t);
			return true;
		}
	}
}

