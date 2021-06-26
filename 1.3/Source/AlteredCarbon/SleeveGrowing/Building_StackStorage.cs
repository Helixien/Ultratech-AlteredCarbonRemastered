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
    public class Building_StackStorage : Building, IThingHolder, IOpenable
    {
        public const int MaxFilledStackCapacity = 10;
        public const int MaxEmergencyBackupDistanceInTiles = 25;
        public static HashSet<Building_StackStorage> building_StackStorages = new HashSet<Building_StackStorage>();
        public Building_StackStorage()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public bool allowColonistCorticalStacks;
        public bool allowStrangerCorticalStacks;
        public bool allowHostileCorticalStacks;
        public CompRefuelable compRefuelable;
        public CompPowerTrader compPower;

        public CorticalStack stackToDuplicate;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            building_StackStorages.Add(this);
            base.SpawnSetup(map, respawningAfterLoad);
            if (base.Faction != null && base.Faction.IsPlayer)
            {
                this.contentsKnown = true;
            }
            compRefuelable = this.TryGetComp<CompRefuelable>();
            compPower = this.TryGetComp<CompPowerTrader>();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            building_StackStorages.Remove(this);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            building_StackStorages.Remove(this);
            if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                this.EjectContents();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }
        public bool CanDuplicateStack
        {
            get
            {
                if (this.stackToDuplicate is null || !this.innerContainer.Contains(this.stackToDuplicate))
                {
                    return false;
                }
                if (!PoweredAndFueled)
                {
                    return false;
                }
                return true;
            }
        }

        public bool PoweredAndFueled => this.compPower.PowerOn && this.compRefuelable.HasFuel;
        public bool HasAnyContents
        {
            get
            {
                return this.innerContainer.Any();
            }
        }

        public IEnumerable<CorticalStack> StoredStacks => this.innerContainer.OfType<CorticalStack>();
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            var stacks = StoredStacks.ToList();
            if (base.Faction == Faction.OfPlayer && stacks.Any())
            {
                var command = new Command_Action
                {
                    defaultLabel = "AlteredCarbon.DuplicateStack".Translate(),
                    defaultDesc = "AC.DuplicateStackDesc".Translate(),
                    action = delegate ()
                    {
                        var floatList = new List<FloatMenuOption>();
                        foreach (var stack in StoredStacks)
                        {
                            floatList.Add(new FloatMenuOption(stack.PersonaData.PawnNameColored, delegate ()
                            {
                                this.stackToDuplicate = stack;
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatList));
                    }
                };

                if (this.stackToDuplicate != null)
                {
                    command.Disable("AC.AlreadySetToDuplicate".Translate());
                }
                else if (!this.compRefuelable.HasFuel)
                {
                    command.Disable("AC.NoEnoughEmptyStacks".Translate());
                }
                else if (stacks.Count() >= MaxFilledStackCapacity)
                {
                    command.Disable("AC.NoEnoughSpaceForNewStack".Translate());
                }
                yield return command;
            }
        }

        public void PerformStackDuplication(Pawn doer)
        {
            float successChance = 1f - Mathf.Abs((doer.skills.GetSkill(SkillDefOf.Intellectual).levelInt / 2f) - 11f) / 10f;
            if (Rand.Chance(successChance))
            {
                var stackCopyTo = (CorticalStack)ThingMaker.MakeThing(AC_DefOf.UT_FilledCorticalStack);
                stackCopyTo.PersonaData.hasPawn = true;
                this.innerContainer.TryAdd(stackCopyTo);
                stackCopyTo.PersonaData.CopyDataFrom(stackToDuplicate.PersonaData, true);
                AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
                compRefuelable.ConsumeFuel(1);
                stackToDuplicate = null;
                Messages.Message("AC.SuccessfullyDuplicatedStack".Translate(doer.Named("PAWN")), this, MessageTypeDefOf.TaskCompletion);
            }
        }

        public void PerformStackBackup(Hediff_CorticalStack hediff_CorticalStack)
        {
            var stackCopyTo = (CorticalStack)ThingMaker.MakeThing(AC_DefOf.UT_FilledCorticalStack);
            stackCopyTo.PersonaData.hasPawn = true;
            this.innerContainer.TryAdd(stackCopyTo);
            stackCopyTo.PersonaData.CopyDataFrom(hediff_CorticalStack.PersonaData);
            AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
            compRefuelable.ConsumeFuel(1);
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption opt in base.GetFloatMenuOptions(myPawn))
            {
                yield return opt;
            }

        }
        public override string GetInspectString()
        {
            return base.GetInspectString();
        }

        public bool CanOpen
        {
            get
            {
                return this.HasAnyContents;
            }
        }

        public int OpenTicks => 60;

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
        public override void Tick()
        {
            base.Tick();
            this.innerContainer.ThingOwnerTick(true);
        }

        public void Open()
        {

        }

        public bool HasFreeSpace => this.innerContainer.Count < MaxFilledStackCapacity;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false, false);
            Scribe_Values.Look(ref this.allowColonistCorticalStacks, "allowColonistCorticalStacks", true);
            Scribe_Values.Look(ref this.allowHostileCorticalStacks, "allowHostileCorticalStacks", false, false);
            Scribe_Values.Look(ref this.allowStrangerCorticalStacks, "allowStrangerCorticalStacks", false, false);
            Scribe_References.Look(ref this.stackToDuplicate, "stackToDuplicate");
        }

        public bool Accepts(Thing thing)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                var stack = thing as CorticalStack;
                if (stack is null)
                {
                    return false;
                }
                if (!stack.PersonaData.hasPawn)
                {
                    return false;
                }

                if (this.allowColonistCorticalStacks && stack.PersonaData.faction != null && stack.PersonaData.faction == Faction.OfPlayer)
                {
                    return true;
                }

                if (this.allowHostileCorticalStacks && stack.PersonaData.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }

                if (this.allowStrangerCorticalStacks && (stack.PersonaData.faction is null || stack.PersonaData.faction != Faction.OfPlayer && !stack.PersonaData.faction.HostileTo(Faction.OfPlayer)))
                {
                    return true;
                }

                return false;
            };
            return validator(thing) && this.innerContainer.CanAcceptAnyOf(thing, true) && HasFreeSpace;
        }

        public bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
            }
            else if (this.innerContainer.TryAdd(thing, true))
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    this.contentsKnown = true;
                }
                return true;
            }
            return false;
        }
        public void EjectContents()
        {
            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Direct, null, null);
            this.contentsKnown = true;
        }

        public ThingOwner innerContainer;

        public bool contentsKnown;
    }
}