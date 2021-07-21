using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_CopyStack : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    var selectedStack = pawn.Map.listerThings.ThingsOfDef(AlteredCarbonDefOf
                        .AC_EmptyCorticalStack)
                        .Where(x => pawn.CanReserveAndReach(x, PathEndMode.ClosestTouch, Danger.Deadly) && !x.IsForbidden(pawn))
                        .MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                    //var selectedStack = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, 
                    //    ThingRequest.ForDef(AlteredCarbonDefOf.AC_EmptyCorticalStack), PathEndMode
                    //    .ClosestTouch, TraverseParms.For(TraverseMode.ByPawn, Danger.Deadly, false));
                    pawn.CurJob.targetB = selectedStack;
                }
            };
            yield return Toils_Haul.StartCarryThing(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing resultingThing);
                    if (resultingThing != TargetThingA)
                    {
                        pawn.CurJob.targetA = resultingThing;
                    }
                }
            };
            Toil copyStack = Toils_General.Wait(120, 0);
            copyStack.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingB.Position);
            });
            ToilEffects.WithProgressBarToilDelay(copyStack, TargetIndex.B, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(copyStack, TargetIndex.A);
            yield return copyStack;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    float damageChance = Mathf.Abs((pawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt / 2f) - 11f) / 10f;
                    if (Rand.Chance(damageChance))
                    {
                        if (TargetThingB.stackCount == 0)
                        {
                            TargetThingB.Destroy(DestroyMode.Vanish);
                        }
                        else
                        {
                            TargetThingB.stackCount--;
                        }
                        Find.LetterStack.ReceiveLetter("AlteredCarbon.DestroyedStack".Translate(),
                            "AlteredCarbon.DestroyedStackDesc".Translate(pawn.Named("PAWN")), 
                            LetterDefOf.NegativeEvent, pawn);
                    }
                    else
                    {
                        var pos = TargetThingB.Position;
                        if (TargetThingB.stackCount == 0)
                        {
                            TargetThingB.Destroy(DestroyMode.Vanish);
                        }
                        else
                        {
                            TargetThingB.stackCount--;
                        }
                        var stackCopyTo = (CorticalStack)ThingMaker.MakeThing(AlteredCarbonDefOf.AC_FilledCorticalStack);
                        stackCopyTo.hasPawn = true;
                        GenSpawn.Spawn(stackCopyTo, pos, pawn.Map);
                        var stackCopyFrom = (CorticalStack)TargetThingA;
                        stackCopyTo.CopyFromOtherStack(stackCopyFrom);
                        ACUtils.ACTracker.RegisterStack(stackCopyTo);
                    }
                }
            };
        }
    }
}

