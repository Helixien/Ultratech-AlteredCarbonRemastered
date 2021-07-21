using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_WipeStack : JobDriver
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
                    var stack = (CorticalStack)TargetThingA;
                    if (stack.faction != null && pawn != null && pawn.Faction != null && pawn.Faction != stack.Faction)
                    {
                        stack.EmptyStack(pawn, true);
                    }
                    else
                    {
                        stack.EmptyStack(pawn);
                    }
                }
            };
        }
    }
}

