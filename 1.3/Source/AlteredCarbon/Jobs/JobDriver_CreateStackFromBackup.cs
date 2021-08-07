﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_CreateStackFromBackup : JobDriver
    {
        public const int RestoringDuration = 1000;
        public Building_StackStorage Building_StackStorage => this.TargetA.Thing as Building_StackStorage;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !Building_StackStorage.Powered);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil restoreStack = Toils_General.Wait(RestoringDuration, 0);
            restoreStack.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingA.Position);
            });
            ToilEffects.WithProgressBarToilDelay(restoreStack, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(restoreStack, TargetIndex.A);
            yield return restoreStack;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Building_StackStorage.PerformStackRestoration(pawn);
                    job.targetB.Thing.Destroy();
                }
            };
        }
    }
}

