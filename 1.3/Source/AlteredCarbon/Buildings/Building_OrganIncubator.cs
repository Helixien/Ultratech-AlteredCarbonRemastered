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
	public class Building_OrganIncubator : Building_Incubator
	{
		public override int OpenTicks => 200;
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}

			if (base.Faction == Faction.OfPlayer)
			{
				if (InnerThing != null && this.incubatorState == IncubatorState.Growing)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.action = this.CancelGrowing;
					command_Action.defaultLabel = "AlteredCarbon.CancelOrganGrowing".Translate();
					command_Action.defaultDesc = "AlteredCarbon.CancelOrganGrowingDesc".Translate();
					command_Action.hotKey = KeyBindingDefOf.Misc8;
					command_Action.icon = ContentFinder<Texture2D>.Get("UI/Icons/CancelSleeve");
					yield return command_Action;
				}
				else if (this.InnerThing == null)
				{
					Command_Action createOrgan = new Command_Action();
					createOrgan.action = new Action(this.CreateOrgan);
					createOrgan.defaultLabel = "AlteredCarbon.CreateOrgan".Translate();
					createOrgan.defaultDesc = "AlteredCarbon.CreateOrganDesc".Translate();
					createOrgan.hotKey = KeyBindingDefOf.Misc8;
					createOrgan.icon = ContentFinder<Texture2D>.Get("UI/Icons/CreateSleeve", true);
					yield return createOrgan;
				}
				if (Prefs.DevMode && incubatorState == IncubatorState.Growing)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "Debug: Instant grow";
					command_Action.action = InstantGrowth;
					yield return command_Action;
				}
			}
			yield break;
		}
		public void CreateOrgan()
		{
			var floatList = new List<FloatMenuOption>();
			foreach (var recipe in this.def.recipes)
            {
				if (recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished)
				{
					continue;
				}
				if (recipe.researchPrerequisites != null)
                {
					foreach (var research in recipe.researchPrerequisites)
                    {
						if (!research.IsFinished)
                        {
							continue;
						}
					}
                }
				floatList.Add(new FloatMenuOption(recipe.LabelCap, delegate 
				{
					var newOrgan = ThingMaker.MakeThing(recipe.ProducedThingDef);
					StartGrowth(newOrgan, (int)recipe.workAmount, (int)(recipe.workAmount * 0.0012f));
				}));
            }
			Find.WindowStack.Add(new FloatMenu(floatList));
		}

		[TweakValue("0AC", -5f, 5f)] public static float yOffset;
		[TweakValue("0AC", -5f, 5f)] public static float zOffset;
		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			//if (this.InnerThing != null)
			//{
			//	Vector3 newPos = drawLoc;
			//	newPos.y += yOffset;
			//	newPos.z += zOffset;
			//	this.InnerThing.Rotation = Rot4.South;
			//	this.InnerThing.DrawAt(newPos, flip);
			//}
			base.Comps_PostDraw();
		}
		public void CancelGrowing()
		{
			this.incubatorState = IncubatorState.Inactive;
			this.totalGrowthCost = 0;
			this.totalTicksToGrow = 0;
			this.curTicksToGrow = 0;
			this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
		}
		public void StartGrowth(Thing newThing, int totalTicksToGrow, int totalGrowthCost)
		{
			var thing = this.InnerThing;
			if (thing != null)
			{
				this.innerContainer.Remove(this.InnerThing);
				thing.Destroy(DestroyMode.Vanish);
			}
			this.TryAcceptThing(newThing);
			this.totalTicksToGrow = totalTicksToGrow;
			this.curTicksToGrow = 0;
			this.totalGrowthCost = totalGrowthCost;
			this.incubatorState = IncubatorState.ToBeActivated;
			this.runningOutPowerInTicks = 0;
		}
		public void InstantGrowth()
		{
			this.curTicksToGrow = this.totalTicksToGrow;
			FinishGrowth();
		}
		public void FinishGrowth()
		{
			this.incubatorState = IncubatorState.Inactive;
		}

        public override void EjectContents()
        {
            base.EjectContents();
			this.innerContainer.TryDrop(this.InnerThing, ThingPlaceMode.Near, 1, out Thing resultingThing);
		}
		public override void KillInnerThing()
        {
            base.KillInnerThing();
			this.InnerThing.Destroy();
        }
        public override void Tick()
		{
			base.Tick();
			if (this.InnerThing == null && this.curTicksToGrow > 0)
			{
				curTicksToGrow = 0;
			}
			if (this.InnerThing != null)
			{
				if (this.incubatorState == IncubatorState.Growing)
                {
					if (refuelable.HasFuel && powerTrader.PowerOn)
					{
						if (runningOutPowerInTicks > 0) runningOutPowerInTicks = 0;
						var fuelCost = this.totalGrowthCost / (float)this.totalTicksToGrow;
						refuelable.ConsumeFuel(fuelCost);
						if (this.curTicksToGrow < totalTicksToGrow)
						{
							curTicksToGrow++;
						}
						else
						{
							this.FinishGrowth();
						}
					}
					else if (!powerTrader.PowerOn && runningOutPowerInTicks < 60000)
					{
						runningOutPowerInTicks++;
					}
					else if (runningOutPowerInTicks >= 60000)
					{
						this.incubatorState = IncubatorState.Inactive;
						this.KillInnerThing();
						Messages.Message("AlteredCarbon.OrganSpoiled".Translate(), this, MessageTypeDefOf.NegativeEvent);
					}
				}

				if (!powerTrader.PowerOn && !isRunningOutPower)
				{
					Messages.Message("AlteredCarbon.OrganIncubatorIsRunningOutPower".Translate(), this, MessageTypeDefOf.NegativeEvent);
					this.isRunningOutPower = true;
				}
				if (!refuelable.HasFuel && !isRunningOutFuel)
				{
					Messages.Message("AlteredCarbon.OrganIncubatorIsRunningOutFuel".Translate(), this, MessageTypeDefOf.NegativeEvent);
					this.isRunningOutFuel = true;
				}
			}
		}
	}
}