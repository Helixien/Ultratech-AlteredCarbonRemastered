using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class ThoughtWorker_LostMySoul : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.story.traits.HasTrait(AlteredCarbonDefOf.AC_AntiStack) && ACUtils.ACTracker.pawnsWithStacks.Contains(p))
			{
				return ThoughtState.ActiveDefault;
			}
			return ThoughtState.Inactive;
		}
	}
}

