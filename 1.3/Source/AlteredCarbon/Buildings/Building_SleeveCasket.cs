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
	public class Building_SleeveCasket : Building_Bed
	{
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			base.Comps_PostDraw();
		}

        public override void Tick()
        {
            base.Tick();
			foreach (var occupant in this.CurOccupants)
            {
				if (occupant.IsEmptySleeve() && occupant.needs.food.CurLevel < 1f)
                {
					occupant.needs.food.CurLevel += 0.001f;
					if (ModCompatibility.DubsBadHygieneActive)
                    {
						ModCompatibility.FillThirstNeed(occupant, 0.001f);
						ModCompatibility.FillHygieneNeed(occupant, 0.001f);
					}
					var malnutrition = occupant.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
					if (malnutrition != null)
                    {
						occupant.health.RemoveHediff(malnutrition);
                    }
					var dehydration = occupant.health.hediffSet.hediffs.FirstOrDefault(x => x.def.defName == "DBHDehydration");
					if (dehydration != null)
                    {
						occupant.health.RemoveHediff(dehydration);
					}
				}
            }

        }
        public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				if (gizmo is Command_Toggle toggle)
				{
					if (toggle.defaultLabel == "CommandBedSetForPrisonersLabel".Translate() || toggle.defaultLabel == "CommandBedSetAsMedicalLabel".Translate())
                    {
						continue;
					}
				}
				else if (gizmo is Command_SetBedOwnerType)
                {
					continue;
                }
				yield return gizmo;
			}
		}
	}
}

