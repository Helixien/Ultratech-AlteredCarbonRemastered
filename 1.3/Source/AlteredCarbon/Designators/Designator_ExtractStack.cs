using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AlteredCarbon
{
	public class Designator_ExtractStack : Designator
	{
		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => AC_DefOf.UT_ExtractStackDesignation;

		public Designator_ExtractStack()
		{
			defaultLabel = "AC.DesignatorExtractStack".Translate();
			defaultDesc = "AC.DesignatorExtractStackDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Icons/ExtractStacks");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Claim;
			hotKey = KeyBindingDefOf.Misc11;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!PawnsWithStacksInCell(c).Any())
			{
				return "AC.MessageMustDesignateHasStack".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			foreach (Thing item in PawnsWithStacksInCell(c))
			{
				DesignateThing(item);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (base.Map.designationManager.DesignationOn(t, Designation) != null)
			{
				return false;
			}
			if (t is Corpse corpse && corpse.InnerPawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) != null)
            {
				return true;
            }
			return false;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		}

		private IEnumerable<Thing> PawnsWithStacksInCell(IntVec3 c)
		{
			if (c.Fogged(base.Map))
			{
				yield break;
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (CanDesignateThing(thingList[i]).Accepted)
				{
					yield return thingList[i];
				}
			}
		}
	}
}

