﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_ExtractStack : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil extractStack = Toils_General.Wait(120, 0);
            ToilEffects.WithProgressBarToilDelay(extractStack, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(extractStack, TargetIndex.A);
            ToilFailConditions.FailOnCannotTouch<Toil>(extractStack, TargetIndex.A, PathEndMode.OnCell);
            yield return extractStack;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Corpse corpse = (Corpse)TargetThingA;
                    Hediff_CorticalStack hediff = corpse.InnerPawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => 
                    x.def.defName == "AC_CorticalStack") as Hediff_CorticalStack;
                    if (hediff != null)
                    {
                        if (hediff.def.spawnThingOnRemoved != null)
                        {
                            var corticalStack = ThingMaker.MakeThing(hediff.def.spawnThingOnRemoved) as CorticalStack;
                            if (hediff.PersonaData.hasPawn)
                            {
                                corticalStack.PersonaData.CopyDataFrom(hediff.PersonaData);
                            }
                            else
                            {
                                corticalStack.PersonaData.CopyPawn(corpse.InnerPawn);
                            }
                            GenPlace.TryPlaceThing(corticalStack, TargetThingA.Position, GetActor().Map, ThingPlaceMode.Near);
                            AlteredCarbonManager.Instance.RegisterStack(corticalStack);
                            AlteredCarbonManager.Instance.RegisterSleeve(corpse.InnerPawn, corticalStack.PersonaData.stackGroupID);
                        }
                        var head = corpse.InnerPawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
                        if (head != null)
                        {
                            Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, corpse.InnerPawn, head);
                            hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                            hediff_MissingPart.IsFresh = true;
                            corpse.InnerPawn.health.AddHediff(hediff_MissingPart);
                        }
                        corpse.InnerPawn.health.RemoveHediff(hediff);
                        if (pawn.Map.designationManager.DesignationOn(corpse)?.def == AC_DefOf.UT_ExtractStackDesignation)
                        {
                            pawn.Map.designationManager.TryRemoveDesignationOn(corpse, AC_DefOf.UT_ExtractStackDesignation);
                        }
                    }
                }
            };
        }
    }
}

