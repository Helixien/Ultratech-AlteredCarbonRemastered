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
    public class Command_HackStacks : Command_Action
    {
        public Building_DecryptionBench decryptionBench;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (var corticalStack in CorticalStack.corticalStacks)
                {
                    if (corticalStack.PersonaData.hasPawn && corticalStack.PersonaData.faction != Faction.OfPlayer 
                        && !decryptionBench.billStack.Bills.Any(x => x is Bill_HackStack hackStack 
                            && hackStack.corticalStack == corticalStack && hackStack.recipe == AC_DefOf.UT_HackFilledCorticalStack)
                        && corticalStack.MapHeld == Find.CurrentMap)
                    {
                        yield return new FloatMenuOption(corticalStack.PersonaData.PawnNameColored, delegate ()
                        {
                            decryptionBench.InstallHackRecipe(corticalStack);
                        });
                    }
                }
            }
        }
    }
    public class Command_ConvertStacksIdeo : Command_Action
    {
        public Building_DecryptionBench decryptionBench;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (var corticalStack in CorticalStack.corticalStacks)
                {
                    if (corticalStack.PersonaData.hasPawn && corticalStack.PersonaData.ideo != Faction.OfPlayer.ideos.PrimaryIdeo 
                        && corticalStack.MapHeld == Find.CurrentMap && !decryptionBench.billStack.Bills
                        .Any(x => x is Bill_HackStack bill && bill.corticalStack == corticalStack 
                        && bill.recipe == ACUtils.UT_ConvertFilledCorticalStackToIdeo))
                    {
                        yield return new FloatMenuOption(corticalStack.PersonaData.PawnNameColored, delegate ()
                        {
                            decryptionBench.InstallConvertIdeoRecipe(corticalStack);
                        });
                    }
                }
            }
        }
    }
    public class Command_WipeStacks : Command_Action
    {
        public Building_DecryptionBench decryptionBench;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (var corticalStack in CorticalStack.corticalStacks)
                {
                    if (corticalStack.PersonaData.hasPawn && !decryptionBench.billStack.Bills.Any(x => x is Bill_HackStack hackStack
                            && hackStack.corticalStack == corticalStack && hackStack.recipe == AC_DefOf.UT_WipeFilledCorticalStack)
                            && corticalStack.MapHeld == Find.CurrentMap)
                    {
                        yield return new FloatMenuOption(corticalStack.PersonaData.PawnNameColored, delegate ()
                        {
                            decryptionBench.InstallWipeStackRecipe(corticalStack);
                        });
                    }
                }
            }
        }
    }

    public class Building_DecryptionBench : Building_WorkTable
    {
        public TargetingParameters ForHackableStack()
        {
            TargetingParameters targetingParameters = new TargetingParameters();
            targetingParameters.canTargetItems = true;
            targetingParameters.mapObjectTargetsMustBeAutoAttackable = false;
            targetingParameters.validator = (TargetInfo x) => x.Thing is CorticalStack stack && stack.PersonaData.hasPawn && stack.PersonaData.faction != Faction.OfPlayer;
            return targetingParameters;
        }
        public TargetingParameters ForWipableStack()
        {
            TargetingParameters targetingParameters = new TargetingParameters();
            targetingParameters.canTargetItems = true;
            targetingParameters.mapObjectTargetsMustBeAutoAttackable = false;
            targetingParameters.validator = (TargetInfo x) => x.Thing is CorticalStack stack && stack.PersonaData.hasPawn;
            return targetingParameters;
        }
        public TargetingParameters ForConvertableStack()
        {
            TargetingParameters targetingParameters = new TargetingParameters();
            targetingParameters.canTargetItems = true;
            targetingParameters.mapObjectTargetsMustBeAutoAttackable = false;
            targetingParameters.validator = (TargetInfo x) => x.Thing is CorticalStack stack && stack.PersonaData.hasPawn;
            return targetingParameters;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_HackStacks
            {
                defaultLabel = "AlteredCarbon.HackStack".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/ConvertStack"),
                action = delegate ()
                {
                    Find.Targeter.BeginTargeting(ForHackableStack(), delegate (LocalTargetInfo x)
                    {
                        InstallHackRecipe(x.Thing as CorticalStack);
                    });
                },
                decryptionBench = this
            };
            yield return new Command_HackStacks
            {
                defaultLabel = "AlteredCarbon.WipeStack".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/WipeStack"),
                action = delegate ()
                {
                    Find.Targeter.BeginTargeting(ForWipableStack(), delegate (LocalTargetInfo x)
                    {
                        InstallWipeStackRecipe(x.Thing as CorticalStack);
                    });
                },
                decryptionBench = this
            };
            if (ModsConfig.IdeologyActive)
            {
                yield return new Command_ConvertStacksIdeo
                {
                    defaultLabel = "AlteredCarbon.ConvertStack".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/ConvertIdeo"),
                    action = delegate ()
                    {
                        Find.Targeter.BeginTargeting(ForConvertableStack(), delegate (LocalTargetInfo x)
                        {
                            InstallConvertIdeoRecipe(x.Thing as CorticalStack);
                        });
                    },
                    decryptionBench = this
                };
            }
        }
        public void InstallHackRecipe(CorticalStack corticalStack)
        {
            if (this.billStack.Bills.Any(y => y is Bill_HackStack hackStack && hackStack.corticalStack == corticalStack && hackStack.recipe == AC_DefOf.UT_HackFilledCorticalStack))
            {
                Messages.Message("AlteredCarbon.AlreadyOrderedToHackStack".Translate(), MessageTypeDefOf.CautionInput);
            }
            else
            {
                this.billStack.AddBill(new Bill_HackStack(corticalStack, AC_DefOf.UT_HackFilledCorticalStack, null));
            }
        }
        public void InstallConvertIdeoRecipe(CorticalStack corticalStack)
        {
            if (this.billStack.Bills.Any(y => y is Bill_HackStack bill && bill.corticalStack == corticalStack && bill.recipe == ACUtils.UT_ConvertFilledCorticalStackToIdeo))
            {
                Messages.Message("AlteredCarbon.AlreadyOrderedToConvertStack".Translate(), MessageTypeDefOf.CautionInput);
            }
            else
            {
                this.billStack.AddBill(new Bill_HackStack(corticalStack, ACUtils.UT_ConvertFilledCorticalStackToIdeo, null));
            }
        }

        public void InstallWipeStackRecipe(CorticalStack corticalStack)
        {
            if (this.billStack.Bills.Any(y => y is Bill_HackStack bill && bill.corticalStack == corticalStack && bill.recipe == AC_DefOf.UT_WipeFilledCorticalStack))
            {
                Messages.Message("AlteredCarbon.AlreadyOrderedToWipeStack".Translate(), MessageTypeDefOf.CautionInput);
            }
            else
            {
                this.billStack.AddBill(new Bill_HackStack(corticalStack, AC_DefOf.UT_WipeFilledCorticalStack, null));
            }
        }
    }
}