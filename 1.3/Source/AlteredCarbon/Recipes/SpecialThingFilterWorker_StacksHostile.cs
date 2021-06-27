﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	public class SpecialThingFilterWorker_StacksHostile : SpecialThingFilterWorker_Stacks
	{
		public override bool Matches(Thing t)
		{
			return base.Matches(t) && t is CorticalStack stack && stack.PersonaData.hasPawn && stack.PersonaData.faction != null && stack.PersonaData.faction.HostileTo(Faction.OfPlayer);
		}
	}
}

