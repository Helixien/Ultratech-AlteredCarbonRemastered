using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class ThoughtWorker_MansBody : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.story.traits.HasTrait(TraitDefOf.DislikesMen) && ACUtils.ACTracker.pawnsWithStacks.Contains(p) && p.gender == Gender.Male)
			{
				return ThoughtState.ActiveDefault;
			}
			return ThoughtState.Inactive;
		}
	}
}

