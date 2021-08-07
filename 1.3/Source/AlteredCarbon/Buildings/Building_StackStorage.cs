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
    public class Building_StackStorage : Building, IThingHolder
    {
        public const int MaxFilledStackCapacity = 25;
        public static HashSet<Building_StackStorage> building_StackStorages = new HashSet<Building_StackStorage>();
        public Building_StackStorage()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            this.backedUpStacks = new Dictionary<int, PersonaData>();
        }

        public bool allowColonistCorticalStacks = true;
        public bool allowStrangerCorticalStacks;
        public bool allowHostileCorticalStacks;
        public CompPowerTrader compPower;
        private bool backupIsEnabled;
        public Dictionary<int, PersonaData> backedUpStacks;
        public CorticalStack stackToDuplicate;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            building_StackStorages.Add(this);
            base.SpawnSetup(map, respawningAfterLoad);
            if (base.Faction != null && base.Faction.IsPlayer)
            {
                this.contentsKnown = true;
            }
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
                if (!Powered)
                {
                    return false;
                }
                return true;
            }
        }

        public PersonaData FirstPersonaStackToRestore
        {
            get
            {
                foreach (var pawn in AlteredCarbonManager.Instance.PawnsWithStacks)
                {
                    foreach (var personaData in StoredBackedUpStacks)
                    {
                        if (pawn.thingIDNumber == personaData.pawnID)
                        {
                            if (pawn.Destroyed || pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) is null)
                            {
                                if (!CorticalStack.corticalStacks.Any(x => x.PersonaData.pawnID == personaData.pawnID && x.Spawned && !x.Destroyed))
                                {
                                    return personaData;
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }
        public bool Powered => this.compPower.PowerOn;
        public bool HasAnyContents
        {
            get
            {
                return this.innerContainer.Any();
            }
        }
        public IEnumerable<PersonaData> StoredBackedUpStacks => this.backedUpStacks.Values;
        public IEnumerable<CorticalStack> StoredStacks => this.innerContainer.OfType<CorticalStack>();
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (base.Faction == Faction.OfPlayer)
            {
                var stacks = StoredStacks.ToList();
                if (stacks.Any())
                {
                    var command = new Command_Action
                    {
                        defaultLabel = "AlteredCarbon.DuplicateStack".Translate(),
                        defaultDesc = "AC.DuplicateStackDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/DuplicateStack"),
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
                    else if (stacks.Count() >= MaxFilledStackCapacity)
                    {
                        command.Disable("AC.NoEnoughSpaceForNewStack".Translate());
                    }
                    yield return command;
                }

                var enableBackup = new Command_Toggle()
                {
                    defaultLabel = "AlteredCarbon.EnableBackup".Translate(),
                    defaultDesc = "AlteredCarbon.EnableBackupDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/EnableBackup"),
                    toggleAction = delegate ()
                    {
                        backupIsEnabled = !backupIsEnabled;
                    },
                    isActive = () => backupIsEnabled
                };
                yield return enableBackup;
                if (backupIsEnabled)
                {
                    var backupAll = new Command_Action()
                    {
                        defaultLabel = "AlteredCarbon.BackupAllStacks".Translate(),
                        defaultDesc = "AlteredCarbon.BackupAllStacksDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/BackupAllStacks"),
                        action = delegate
                        {
                            BackupAllColonistsWithStacks();
                        },
                    };
                    if (!this.compPower.PowerOn)
                    {
                        backupAll.Disable("NoPower".Translate());
                    }
                    yield return backupAll;
                }

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
        }
        public void PerformStackRestoration(Pawn doer)
        {
            var stackRestoreTo = (CorticalStack)ThingMaker.MakeThing(AC_DefOf.UT_FilledCorticalStack);
            stackRestoreTo.PersonaData.hasPawn = true;
            var personaDataToRestore = FirstPersonaStackToRestore;
            stackRestoreTo.PersonaData.CopyDataFrom(personaDataToRestore, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            backedUpStacks.Remove(personaDataToRestore.pawnID);
            Messages.Message("AlteredCarbon.SuccessfullyRestoredStackFromBackup".Translate(doer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, doer.Position, doer.Map, ThingPlaceMode.Near);
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption opt in base.GetFloatMenuOptions(myPawn))
            {
                yield return opt;
            }
        }
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
            if (this.backupIsEnabled && compPower.PowerOn)
            {
                if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                {
                    BackupAllColonistsWithStacks();
                }
            }
        }

        public void BackupAllColonistsWithStacks()
        {
            foreach (var pawn in AlteredCarbonManager.Instance.PawnsWithStacks)
            {
                if (CanBackup(pawn))
                {
                    Backup(pawn);
                }
            }
        }
        public bool CanBackup(Pawn pawn)
        {
            return pawn.Faction == this.Faction && pawn.MapHeld == this.Map && pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) is Hediff_CorticalStack;
        }
        public void Backup(Pawn pawn)
        {
            var copy = new PersonaData();
            copy.CopyPawn(pawn);
            copy.isCopied = true;
            copy.lastTimeUpdated = Find.TickManager.TicksGame;
            this.backedUpStacks[copy.pawnID] = copy;
        }
        public bool HasFreeSpace => this.innerContainer.Count < MaxFilledStackCapacity;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false);
            Scribe_Values.Look(ref this.allowColonistCorticalStacks, "allowColonistCorticalStacks", true);
            Scribe_Values.Look(ref this.allowHostileCorticalStacks, "allowHostileCorticalStacks", false);
            Scribe_Values.Look(ref this.allowStrangerCorticalStacks, "allowStrangerCorticalStacks", false);
            Scribe_Values.Look(ref this.backupIsEnabled, "backupIsEnabled");
            Scribe_References.Look(ref this.stackToDuplicate, "stackToDuplicate");
            Scribe_Collections.Look(ref this.backedUpStacks, "backedUpStacks", LookMode.Value, LookMode.Deep, ref intKeys, ref personaDataValues);
        }

        private List<int> intKeys;
        private List<PersonaData> personaDataValues;
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