using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
	public class CompProperties_BrainTemplate : CompProperties
	{
		public List<string> hediffs;

		public List<SkillOffsets> skillsOffsets;

		public List<SkillOffsets> skillPassionsOffsets;
		public CompProperties_BrainTemplate()
		{
			this.compClass = typeof(CompBrainTemplate);
		}
	}
}
