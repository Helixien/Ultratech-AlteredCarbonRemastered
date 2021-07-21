using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class ThoughtWorker_ColonyUsesStacks : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.story.traits.HasTrait(AlteredCarbonDefOf.AC_AntiStack))
			{
				foreach (var pawn in p.Map.mapPawns.AllPawns.Where(x => x.IsColonist))
				{
					if (pawn != p && ACUtils.ACTracker.pawnsWithStacks.Contains(pawn))
					{
						return ThoughtState.ActiveDefault;
					}
				}
			}
			return ThoughtState.Inactive;
		}
	}
}

