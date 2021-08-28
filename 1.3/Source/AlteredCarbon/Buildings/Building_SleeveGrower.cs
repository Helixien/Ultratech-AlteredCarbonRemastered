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
	public class Building_SleeveGrower : Building_Incubator
	{
		public bool innerPawnIsDead;

		public ThingDef activeBrainTemplateToBeProcessed;

		public bool removeActiveBrainTemplate;
		public override int OpenTicks => 300;
		public Pawn InnerPawn
		{
			get
			{
				return this.innerContainer.OfType<Pawn>().FirstOrDefault();
			}
		}
        protected override bool InnerThingIsDead => innerPawnIsDead;
        public Thing ActiveBrainTemplate
		{
			get
			{
				return this.innerContainer.Where(x => x.TryGetComp<CompBrainTemplate>() != null).FirstOrDefault();
			}
		}
		public override Thing InnerThing => this.innerContainer.Where(x => x.TryGetComp<CompBrainTemplate>() is null).FirstOrDefault();
        public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				if (innerContainer.Count > 0 && this.incubatorState == IncubatorState.Growing)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.action = this.OrderToCancel;
					command_Action.defaultLabel = "AlteredCarbon.CancelSleeveBodyGrowing".Translate();
					command_Action.defaultDesc = "AlteredCarbon.CancelSleeveBodyGrowingDesc".Translate();
					command_Action.hotKey = KeyBindingDefOf.Misc8;
					command_Action.icon = ContentFinder<Texture2D>.Get("UI/Icons/CancelSleeve");
					yield return command_Action;
				}

				if (this.InnerPawn == null || this.innerPawnIsDead)
				{
					Command_Action createSleeveBody = new Command_Action();
					createSleeveBody.action = new Action(this.CreateSleeve);
					createSleeveBody.defaultLabel = "AlteredCarbon.CreateSleeveBody".Translate();
					createSleeveBody.defaultDesc = "AlteredCarbon.CreateSleeveBodyDesc".Translate();
					createSleeveBody.hotKey = KeyBindingDefOf.Misc8;
					createSleeveBody.icon = ContentFinder<Texture2D>.Get("UI/Icons/CreateSleeve", true);
					yield return createSleeveBody;

					Command_Action copySleeveBody = new Command_Action();
					copySleeveBody.action = new Action(this.CopyPawnBody);
					copySleeveBody.defaultLabel = "AlteredCarbon.CloneSleeve".Translate();
					copySleeveBody.defaultDesc = "AlteredCarbon.CloneSleeveDesc".Translate();
					copySleeveBody.hotKey = KeyBindingDefOf.Misc8;
					copySleeveBody.icon = ContentFinder<Texture2D>.Get("UI/Icons/CloneSleeve", true);
					yield return copySleeveBody;


				}
				if (Prefs.DevMode && incubatorState == IncubatorState.Growing)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "Debug: Instant grow";
					command_Action.action = InstantGrowth;
					yield return command_Action;
				}

				if (this.activeBrainTemplateToBeProcessed == null && this.ActiveBrainTemplate == null)
				{
					var command_Action = new Command_SetBrainTemplate(this);
					command_Action.defaultLabel = "AlteredCarbon.InsertBrainTemplate".Translate();
					command_Action.defaultDesc = "AlteredCarbon.InsertBrainTemplateDesc".Translate();
					command_Action.hotKey = KeyBindingDefOf.Misc8;
					command_Action.icon = ContentFinder<Texture2D>.Get("UI/Icons/None", true);
					yield return command_Action;
				}
				if (this.ActiveBrainTemplate != null)
				{
					var command_Action = new Command_SetBrainTemplate(this, true);
					command_Action.defaultLabel = this.ActiveBrainTemplate.LabelCap;
					command_Action.defaultDesc = "AlteredCarbon.ActiveBrainTemplateDesc".Translate() + this.ActiveBrainTemplate.LabelCap;
					command_Action.hotKey = KeyBindingDefOf.Misc8;
					command_Action.icon = this.ActiveBrainTemplate.def.uiIcon;
					yield return command_Action;
				}
				else if (this.activeBrainTemplateToBeProcessed != null)
				{
					var command_Action = new Command_SetBrainTemplate(this, true);
					command_Action.defaultLabel = this.activeBrainTemplateToBeProcessed.LabelCap;
					command_Action.defaultDesc = "AlteredCarbon.AwaitingBrainTemplateDesc".Translate() + this.activeBrainTemplateToBeProcessed.LabelCap;
					command_Action.hotKey = KeyBindingDefOf.Misc8;
					command_Action.icon = this.activeBrainTemplateToBeProcessed.uiIcon;
					yield return command_Action;
				}
			}
			yield break;
		}
		public Graphic fetus;
		public Graphic Fetus
		{
			get
			{
				if (fetus == null)
				{
					fetus = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Humanlike/Vat/Fetus", ShaderDatabase.CutoutSkin, Vector3.one, this.InnerPawn.story.SkinColor);
				}
				return fetus;
			}
		}

		public Graphic child;
		public Graphic Child
		{
			get
			{
				if (child == null)
				{
					child = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Vat/Child", ShaderDatabase.CutoutSkin, Vector3.one, this.InnerPawn.story.SkinColor);
				}
				return child;
			}
		}

		public Graphic fetus_dead;
		public Graphic Fetus_Dead
		{
			get
			{
				if (fetus_dead == null)
				{
					fetus_dead = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Humanlike/Vat/Fetus_Dead", ShaderDatabase.CutoutSkin, Vector3.one, this.InnerPawn.story.SkinColor);
				}
				return fetus_dead;
			}
		}
		public Graphic child_dead;
		public Graphic Child_Dead
		{
			get
			{
				if (child_dead == null)
				{
					child_dead = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Vat/Child_Dead", ShaderDatabase.CutoutSkin, Vector3.one, this.InnerPawn.story.SkinColor);
				}
				return child_dead;
			}
		}

		public Graphic adult_dead;
		public Graphic Adult_Dead
		{
			get
			{
				if (adult_dead == null)
				{
					adult_dead = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Vat/Adult_Dead", ShaderDatabase.CutoutSkin, Vector3.one, this.InnerPawn.story.SkinColor);
				}
				return adult_dead;
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			if (this.incubatorState != IncubatorState.ToBeActivated && this.InnerPawn != null)
			{
				Vector3 newPos = drawLoc;
				newPos.z += 0.2f;
				newPos.y += 1;
				var growthValue = GrowthProgress;
				if (!this.innerPawnIsDead)
				{
					if (growthValue < 0.33f)
					{
						Fetus.Draw(newPos, Rot4.North, this);
					}
					else if (growthValue < 0.66f)
					{
						Child.Draw(newPos, Rot4.North, this);
					}
					else
					{
						this.InnerPawn.Rotation = this.Rotation;
						this.InnerPawn.DrawAt(newPos, flip);
					}
				}
				else if (this.innerPawnIsDead)
				{
					if (growthValue < 0.33f)
					{
						Fetus_Dead.Draw(newPos, Rot4.North, this);
					}
					else if (growthValue < 0.66f)
					{
						Child_Dead.Draw(newPos, Rot4.North, this);
					}
					else
					{
						Adult_Dead.Draw(newPos, Rot4.North, this);
					}
				}
			}
			base.Comps_PostDraw();
		}
		public void ResetGraphics()
		{
			this.fetus = null;
			this.child = null;
			this.fetus_dead = null;
			this.child_dead = null;
			this.adult_dead = null;
		}

		public void OrderToCancel()
        {
			this.incubatorState = IncubatorState.ToBeCanceled;
		}
		public void CancelGrowing()
		{
			this.incubatorState = IncubatorState.Inactive;
			this.totalGrowthCost = 0;
			this.totalTicksToGrow = 0;
			this.curTicksToGrow = 0;
			this.innerPawnIsDead = false;
			this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
			ResetGraphics();
		}
		public void CreateSleeve()
		{
			if (Find.Targeter.IsTargeting)
            {
				Find.Targeter.StopTargeting();
            }
			Find.WindowStack.Add(new CustomizeSleeveWindow(this));
		}
		public static TargetingParameters ForPawn()
		{
			TargetingParameters targetingParameters = new TargetingParameters();
			targetingParameters.canTargetPawns = true;
			targetingParameters.canTargetItems = true;
			targetingParameters.mapObjectTargetsMustBeAutoAttackable = false;
			targetingParameters.validator = (TargetInfo x) => x.Thing is Pawn pawn && pawn.RaceProps.Humanlike || x.Thing is Corpse corpse && corpse.InnerPawn.RaceProps.Humanlike;
			return targetingParameters;
		}
		public void CopyPawnBody()
        {
			Find.Targeter.BeginTargeting(ForPawn(), delegate (LocalTargetInfo x)
			{
				if (x.Thing is Pawn pawn)
                {
					Find.WindowStack.Add(new CustomizeSleeveWindow(this, pawn));
                }
				else if (x.Thing is Corpse corpse)
                {
					Find.WindowStack.Add(new CustomizeSleeveWindow(this, corpse.InnerPawn));
				}
			});
		}

        public void StartGrowth(Pawn newSleeve, int totalTicksToGrow, int totalGrowthCost)
		{
			this.ResetGraphics();
			if (this.ActiveBrainTemplate != null)
			{
				var comp = this.ActiveBrainTemplate.TryGetComp<CompBrainTemplate>();
				comp.SaveBodyData(newSleeve);
			}
			Pawn pawn = this.InnerPawn;
			if (pawn != null)
			{
				this.innerContainer.Remove(this.InnerPawn);
				pawn.Destroy(DestroyMode.Vanish);
			}

			var result = this.TryAcceptThing(newSleeve);
			this.totalTicksToGrow = totalTicksToGrow;
			this.curTicksToGrow = 0;
			this.totalGrowthCost = totalGrowthCost;
			this.incubatorState = IncubatorState.ToBeActivated;
			this.innerPawnIsDead = false;
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
			if (AlteredCarbonManager.Instance.emptySleeves == null) AlteredCarbonManager.Instance.emptySleeves = new HashSet<Pawn>();
			AlteredCarbonManager.Instance.emptySleeves.Add(this.InnerPawn);
			Messages.Message("AlteredCarbon.FinishedGrowingSleeve".Translate(), this, MessageTypeDefOf.CautionInput);
		}

        public override void Open()
        {
			var sleeve = InnerPawn;
            base.Open();
			var pawn = OpeningPawn();
			if (pawn != null)
            {
				Building_Bed bed = RestUtility.FindBedFor(sleeve, pawn, checkSocialProperness: false);
				if (bed == null)
				{
					bed = RestUtility.FindBedFor(sleeve, pawn, checkSocialProperness: false, ignoreOtherReservations: true);
				}
				if (bed != null)
				{
					Job job = JobMaker.MakeJob(JobDefOf.Rescue, sleeve, bed);
					job.count = 1;
					pawn.jobs.jobQueue.EnqueueFirst(job, JobTag.Misc);
				}
			}
		}
		private Pawn OpeningPawn()
		{
			foreach (var reserv in Map.reservationManager.ReservationsReadOnly)
			{
				if (reserv.Target == this)
				{
					if (reserv.Claimant.CurJob != null && reserv.Claimant.CurJob.def == JobDefOf.Open && reserv.Claimant.CurJob.targetA.Thing == this)
					{
						return reserv.Claimant;
					}
				}
			}
			return null;
		}

        public override bool HasAnyContents => this.InnerPawn != null;
        public override void EjectContents()
		{
			base.EjectContents();
			ThingDef filth_Slime = ThingDefOf.Filth_Slime;
			foreach (Thing thing in this.innerContainer)
			{
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					PawnComponentsUtility.AddComponentsForSpawn(pawn);
					pawn.filth.GainFilth(filth_Slime);
					pawn.health.AddHediff(AC_DefOf.UT_EmptySleeve);
				}
			}
			var openingPawn = OpeningPawn();
			if (openingPawn != null)
            {
				this.innerContainer.TryDrop(this.InnerThing, openingPawn.Position, this.Map, ThingPlaceMode.Direct, 1, out Thing resultingThing);
			}
			else
            {
				this.innerContainer.TryDrop(this.InnerThing, ThingPlaceMode.Near, 1, out Thing resultingThing);
            }
			ResetGraphics();
		}
		public void DropActiveBrainTemplate()
		{
			this.innerContainer.TryDrop(this.ActiveBrainTemplate, ThingPlaceMode.Near, out Thing result);
			this.removeActiveBrainTemplate = false;
		}
		public void AcceptBrainTemplate(Thing brainTemplate)
		{
			if (this.ActiveBrainTemplate != null)
			{
				this.DropActiveBrainTemplate();
			}
			this.innerContainer.TryAddOrTransfer(brainTemplate);
			this.activeBrainTemplateToBeProcessed = null;
			this.removeActiveBrainTemplate = false;
		}

        public override void KillInnerThing()
        {
			this.innerPawnIsDead = true;
		}
		public override void Tick()
		{
			base.Tick();
			if (this.InnerPawn == null && this.curTicksToGrow > 0)
			{
				curTicksToGrow = 0;
			}
			if (this.InnerPawn != null)
			{
				if (this.incubatorState == IncubatorState.Growing || this.incubatorState == IncubatorState.ToBeCanceled)
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
						Messages.Message("AlteredCarbon.SleeveInIncubatorIsDead".Translate(), this, MessageTypeDefOf.NegativeEvent);
					}
				}

				if (!powerTrader.PowerOn && !isRunningOutPower)
				{
					Messages.Message("AlteredCarbon.isRunningOutPower".Translate(), this, MessageTypeDefOf.NegativeEvent);
					this.isRunningOutPower = true;
				}
				if (!refuelable.HasFuel && !isRunningOutFuel)
				{
					Messages.Message("AlteredCarbon.isRunningOutFuel".Translate(), this, MessageTypeDefOf.NegativeEvent);
					this.isRunningOutFuel = true;
				}
			}
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref activeBrainTemplateToBeProcessed, "activeBrainTemplateToBeProcessed");
			Scribe_Values.Look(ref removeActiveBrainTemplate, "removeActiveBrainTemplate", false);
			Scribe_Values.Look(ref this.innerPawnIsDead, "innerPawnIsDead", false, true);
		}
	}
}