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

