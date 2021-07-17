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
	public enum IncubatorState
    {
		Inactive,
		ToBeCanceled,
		ToBeActivated,
		Growing,
    };
	public class Building_Incubator : Building, IThingHolder, IOpenable
	{
		public Building_Incubator()
        {
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		}

		public int totalTicksToGrow = 0;
		public int curTicksToGrow = 0;

		public float totalGrowthCost = 0;
		public IncubatorState incubatorState;
		protected CompPowerTrader powerTrader;
		protected CompBreakdownable breakdownable;
		protected CompRefuelable refuelable;
		protected ThingOwner innerContainer;
		protected bool contentsKnown;

		public int runningOutPowerInTicks;

		public bool isRunningOutPower;

		public bool isRunningOutFuel;
		public bool HasAnyContents => this.innerContainer.FirstOrDefault() != null;
		public virtual bool CanOpen => HasAnyContents && this.incubatorState == IncubatorState.Inactive && !this.InnerThingIsDead;
		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
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
			this.refuelable = base.GetComp<CompRefuelable>();
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
			if (innerContainer.Any() && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize) && this.incubatorState == IncubatorState.Inactive
				&& this.curTicksToGrow == this.totalTicksToGrow && !this.InnerThingIsDead)
			{
				if (mode != DestroyMode.Deconstruct)
				{
					List<Pawn> list = new List<Pawn>();
					foreach (Thing item in innerContainer)
					{
						Pawn pawn = item as Pawn;
						if (pawn != null)
						{
							list.Add(pawn);
						}
					}
					foreach (Pawn item2 in list)
					{
						HealthUtility.DamageUntilDowned(item2);
					}
				}
				EjectContents();
			}
			innerContainer.ClearAndDestroyContents();
			base.Destroy(mode);
		}

		protected virtual bool InnerThingIsDead { get; }
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
		public Thing InnerThing
		{
			get
			{
				return this.innerContainer.FirstOrDefault();
			}
		}

        public virtual int OpenTicks => -1;

        public bool Accepts(Thing thing)
		{
			return this.InnerThing == null;
		}

		public override string GetInspectString()
		{
			var str = base.GetInspectString();
			if (this.InnerThing != null)
			{
				return str + "\n" + "AlteredCarbon.GrowthProgress".Translate() +
					Math.Round(((float)this.curTicksToGrow / this.totalTicksToGrow) * 100f, 2).ToString() + "%";
			}
			else
			{
				return str;
			}
		}
		public virtual void EjectContents()
		{
			if (!base.Destroyed)
			{
				SoundStarter.PlayOneShot(SoundDefOf.CryptosleepCasket_Eject, SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), 0));
			}
			this.contentsKnown = true;
		}
		public virtual void KillInnerThing()
		{

		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref this.totalTicksToGrow, "totalTicksToGrow", 0, true);
			Scribe_Values.Look(ref this.curTicksToGrow, "curTicksToGrow", 0, true);
			Scribe_Values.Look(ref this.totalGrowthCost, "totalGrowthCost", 0f, true);
			Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false, true);
			Scribe_Values.Look(ref this.incubatorState, "incubatorState", IncubatorState.Inactive);
			Scribe_Values.Look(ref this.isRunningOutPower, "isRunningOutPower", false, true);
			Scribe_Values.Look(ref this.isRunningOutFuel, "isRunningOutFuel", false, true);
			Scribe_Values.Look(ref this.runningOutPowerInTicks, "runningOutPowerInTicks", 0, true);

		}
	}
}