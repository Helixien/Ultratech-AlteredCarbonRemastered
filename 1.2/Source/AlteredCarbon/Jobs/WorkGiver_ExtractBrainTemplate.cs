using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace AlteredCarbon
{
    public class WorkGiver_ExtractBrainTemplate : WorkGiver_Scanner
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
            if (sleeveIncubator == null) return false;
                
            if (sleeveIncubator.activeBrainTemplateToBeProcessed == null && sleeveIncubator.ActiveBrainTemplate == null)
            {
                return false;
            }
            if (!t.IsForbidden(pawn) && !t.IsBurning())
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, 1, null, forced))
                {
                    if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                    {
                        return false;
                    }
                    if (sleeveIncubator.removeActiveBrainTemplate && sleeveIncubator.ActiveBrainTemplate != null && !sleeveIncubator.active)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var sleeveIncubator = t as Building_SleeveGrower;
            return new Job(AlteredCarbonDefOf.AC_ExtractActiveBrainTemplate, sleeveIncubator);
        }
    }
}
