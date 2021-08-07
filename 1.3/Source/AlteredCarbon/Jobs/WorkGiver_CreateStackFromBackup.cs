using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    public class WorkGiver_CreateStackFromBackup : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return Building_StackStorage.building_StackStorages.Where(x => x.Powered && x.FirstPersonaStackToRestore != null 
                && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt < 10)
            {
                JobFailReason.Is("AlteredCarbon.CannotCopyNoIntellectual".Translate());
                return false;
            }
            var emptyCorticalStack = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                    ThingRequest.ForDef(AC_DefOf.UT_EmptyCorticalStack), PathEndMode.Touch, TraverseParms.For(pawn));
            if (emptyCorticalStack is null)
            {
                JobFailReason.Is("AlteredCarbon.CannotRestoreBackupNoOtherEmptyStacks".Translate());
                return false;
            }
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var emptyCorticalStack = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForDef(AC_DefOf.UT_EmptyCorticalStack), PathEndMode.Touch, TraverseParms.For(pawn));
            Job job = JobMaker.MakeJob(AC_DefOf.UT_CreateStackFromBackup, t, emptyCorticalStack);
            job.count = 1;
            return job;
        }
    }
}