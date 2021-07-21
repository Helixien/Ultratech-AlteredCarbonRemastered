using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	[HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange", null)]
	public static class CheckForStateChange_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff)
		{
			if (dinfo != null && ___pawn.health.hediffSet.GetNotMissingParts()
				.Where(x => x.def == BodyPartDefOf.Neck).Count() == 0)
			{
				Hediff stackHediff = ___pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == AlteredCarbonDefOf.AC_CorticalStack);
				Random random = new Random();
				if (stackHediff != null)
				{
					if (random.Next(0, 100) <= 25)
					{
						if (stackHediff.def.spawnThingOnRemoved != null)
						{
							Notify_ColonistKilled_Patch.DisableKilledEffect = true;
							var corticalStack = ThingMaker.MakeThing(stackHediff.def.spawnThingOnRemoved) as CorticalStack;
							corticalStack.SavePawnToCorticalStack(___pawn);
							GenPlace.TryPlaceThing(corticalStack, ___pawn.Position, ___pawn.Map, ThingPlaceMode.Near);
						}
						___pawn.health.RemoveHediff(stackHediff);
					}
					if (!___pawn.Dead)
					{
						___pawn.Kill(null);
						Notify_ColonistKilled_Patch.DisableKilledEffect = false;
					}
				}
			}
		}
	}
}

