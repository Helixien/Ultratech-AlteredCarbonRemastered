using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace AlteredCarbon
{
    public class WorkGiver_InsertBrainTemplate : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(AlteredCarbonDefOf.AC_SleeveIncubator);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var sleeveIncubator = t as Building_SleeveGrower;
            if (sleeveIncubator == null || sleeveIncubator.active) return false;
                
            if (sleeveIncubator.activeBrainTemplateToBeProcessed != null && !t.IsForbidden(pawn) && !t.IsBurning())
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, 1, null, forced))
                {
                    if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                    {
                        return false;
                    }
                    if (this.FindBrainTemplate(pawn, sleeveIncubator.activeBrainTemplateToBeProcessed) == null)
                    {
                        JobFailReason.Is("AlteredCarbon.NoBrainTemplatesFound".Translate(), null);
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var sleeveIncubator = t as Building_SleeveGrower;
            Thing brainTemplate = this.FindBrainTemplate(pawn, sleeveIncubator.activeBrainTemplateToBeProcessed);
            return new Job(AlteredCarbonDefOf.AC_InsertBrainTemplate, sleeveIncubator, brainTemplate);
        }

        private Thing FindBrainTemplate(Pawn pawn, ThingDef brainTemplate)
        {
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, 1, null, false);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest thingReq = ThingRequest.ForDef(brainTemplate);
            PathEndMode peMode = PathEndMode.ClosestTouch;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
