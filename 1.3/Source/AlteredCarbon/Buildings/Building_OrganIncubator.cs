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
	public class Building_OrganIncubator : Building, IThingHolder, IOpenable
	{
		protected ThingOwner innerContainer;

		protected bool contentsKnown;

		public int runningOutPowerInTicks;

		public bool isRunningOutPower;

		public bool isRunningOutFuel;
		public bool HasAnyContents => this.InnerThing != null;
		public bool CanOpen => HasAnyContents && !this.active;

		public Building_OrganIncubator()
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void TickRare()
		{
			base.TickRare();
			innerContainer.ThingOwnerTickRare();
		}
		public virtual void Open()
		{
			if (HasAnyContents)
			{
				EjectContents();
			}
		}
		public override bool ClaimableBy(Faction fac)
		{
			if (innerContainer.Any)
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i].Faction == fac)
					{
						return true;
					}
				}
				return false;
			}
			return base.ClaimableBy(fac);
		}
		public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (!Accepts(thing))
			{
				return false;
			}
			bool flag = false;
			if (thing.holdingOwner != null)
			{
				thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
				flag = true;
			}
			else
			{
				flag = innerContainer.TryAdd(thing);
			}
			if (flag)
			{
				if (thing.Faction != null && thing.Faction.IsPlayer)
				{
					contentsKnown = true;
				}
				return true;
			}
			return false;
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize) && !this.active
				&& this.curTicksToGrow == this.totalTicksToGrow)
			{
				EjectContents();
			}
			innerContainer.ClearAndDestroyContents();
			base.Destroy(mode);
		}
		public bool IsOperating
		{
			get
			{
				CompPowerTrader compPowerTrader = this.powerTrader;
				if (compPowerTrader == null || compPowerTrader.PowerOn)
				{
					CompBreakdownable compBreakdownable = this.breakdownable;
					return compBreakdownable == null || !compBreakdownable.BrokenDown;
				}
				return false;
			}
		}

		public bool Accepts(Thing thing)
		{
			return this.InnerThing is null;
		}
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				contentsKnown = true;
			}
			this.powerTrader = base.GetComp<CompPowerTrader>();
			this.breakdownable = base.GetComp<CompBreakdownable>();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}

			if (base.Faction == Faction.OfPlayer)
			{
				if (InnerThing != null && this.active)
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
				if (Prefs.DevMode && active)
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
				floatList.Add(new FloatMenuOption(recipe.LabelCap, delegate 
				{
					var newOrgan = ThingMaker.MakeThing(recipe.ProducedThingDef);
					StartGrowth(newOrgan, (int)recipe.workAmount, (int)(recipe.workAmount * 0.0012f));
				}));
            }
			Find.WindowStack.Add(new FloatMenu(floatList));
		}
		public override string GetInspectString()
		{
			if (this.InnerThing != null)
			{
				return base.GetInspectString() + "\n" + "AlteredCarbon.GrowthProgress".Translate() +
					Math.Round(((float)this.curTicksToGrow / this.totalTicksToGrow) * 100f, 2).ToString() + "%";
			}
			else
			{
				return base.GetInspectString();
			}
		}
        public int OpenTicks => 200;

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


		public Thing InnerThing
        {
            get
            {
				return this.innerContainer.FirstOrDefault();
            }
        }
		public void CancelGrowing()
		{
			this.active = false;
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
			this.active = true;
			this.runningOutPowerInTicks = 0;
		}
		public void InstantGrowth()
		{
			this.curTicksToGrow = this.totalTicksToGrow;
			FinishGrowth();
		}
		public void FinishGrowth()
		{
			this.active = false;
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
				if (this.active && base.GetComp<CompRefuelable>().HasFuel && powerTrader.PowerOn)
				{
					if (runningOutPowerInTicks > 0) runningOutPowerInTicks = 0;
					var fuelCost = this.totalGrowthCost / (float)this.totalTicksToGrow;
					base.GetComp<CompRefuelable>().ConsumeFuel(fuelCost);
					if (this.curTicksToGrow < totalTicksToGrow)
					{
						curTicksToGrow++;
					}
					else
					{
						this.FinishGrowth();
					}
				}
				else if (this.active && !powerTrader.PowerOn && runningOutPowerInTicks < 60000)
				{
					runningOutPowerInTicks++;
				}
				else if (runningOutPowerInTicks >= 60000 && this.active)
				{
					this.active = false;
					Messages.Message("AlteredCarbon.OrganSpoiled".Translate(), this, MessageTypeDefOf.NegativeEvent);
				}
				if (!powerTrader.PowerOn && !isRunningOutPower)
				{
					Messages.Message("AlteredCarbon.OrganIncubatorIsRunningOutPower".Translate(), this, MessageTypeDefOf.NegativeEvent);
					this.isRunningOutPower = true;
				}
				if (!base.GetComp<CompRefuelable>().HasFuel && !isRunningOutFuel)
				{
					Messages.Message("AlteredCarbon.OrganIncubatorIsRunningOutFuel".Translate(), this, MessageTypeDefOf.NegativeEvent);
					this.isRunningOutFuel = true;
				}
			}
		}

		public void EjectContents()
		{
			if (!base.Destroyed)
			{
				SoundStarter.PlayOneShot(SoundDefOf.CryptosleepCasket_Eject, SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), 0));
			}
			this.innerContainer.TryDrop(this.InnerThing, ThingPlaceMode.Near, 1, out Thing resultingThing);
			this.contentsKnown = true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look<int>(ref this.totalTicksToGrow, "totalTicksToGrow", 0, true);
			Scribe_Values.Look<int>(ref this.curTicksToGrow, "curTicksToGrow", 0, true);
			Scribe_Values.Look<float>(ref this.totalGrowthCost, "totalGrowthCost", 0f, true);
			Scribe_Values.Look<bool>(ref this.contentsKnown, "contentsKnown", false, true);
			Scribe_Values.Look<bool>(ref this.active, "active", false, true);
			Scribe_Values.Look<bool>(ref this.isRunningOutPower, "isRunningOutPower", false, true);
			Scribe_Values.Look<bool>(ref this.isRunningOutFuel, "isRunningOutFuel", false, true);
			Scribe_Values.Look<int>(ref this.runningOutPowerInTicks, "runningOutPowerInTicks", 0, true);
		}

		public int totalTicksToGrow = 0;
		public int curTicksToGrow = 0;

		public float totalGrowthCost = 0;
		public bool active;

		private CompPowerTrader powerTrader;
		private CompBreakdownable breakdownable;
	}
}