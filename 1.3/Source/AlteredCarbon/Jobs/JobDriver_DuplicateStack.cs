using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_DuplicateStack : JobDriver
    {
        public const int DuplicateDuration = 1000;
        public Building_StackStorage Building_StackStorage => this.TargetA.Thing as Building_StackStorage;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !Building_StackStorage.CanDuplicateStack);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil copyStack = Toils_General.Wait(DuplicateDuration, 0);
            copyStack.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingA.Position);
            });
            ToilEffects.WithProgressBarToilDelay(copyStack, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(copyStack, TargetIndex.A);
            yield return copyStack;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Building_StackStorage.PerformStackDuplication(pawn);
                }
            };
        }
    }
}

