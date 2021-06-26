using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    public class WorkGiver_DuplicateStacks : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return Building_StackStorage.building_StackStorages.Where(x => x.stackToDuplicate != null && x.CanDuplicateStack && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt < 10)
            {
                JobFailReason.Is("AlteredCarbon.CannotCopyNoIntellectual".Translate());
                return false;
            }
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(AC_DefOf.UT_DuplicateStack, t);
        }
    }
}