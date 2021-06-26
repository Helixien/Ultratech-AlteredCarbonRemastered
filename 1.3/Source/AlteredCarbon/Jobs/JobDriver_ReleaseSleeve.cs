using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_ReleaseSleeve : JobDriver
    {
        public JobDriver_ReleaseSleeve()
        {
            this.rotateToFace = TargetIndex.A;
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toils_General.Wait(120).WithProgressBarToilDelay(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    var grower = (Building_SleeveGrower)this.TargetA.Thing;
                    grower.EjectContents();
                }
            };
            yield break;
        }
    }
}

