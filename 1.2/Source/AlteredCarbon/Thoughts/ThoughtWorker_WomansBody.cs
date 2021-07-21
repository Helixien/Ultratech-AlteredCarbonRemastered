using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class ThoughtWorker_WomansBody : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.story.traits.HasTrait(TraitDefOf.DislikesWomen) && ACUtils.ACTracker.pawnsWithStacks.Contains(p) && p.gender == Gender.Female)
			{
				return ThoughtState.ActiveDefault;
			}
			return ThoughtState.Inactive;
		}
	}
}

