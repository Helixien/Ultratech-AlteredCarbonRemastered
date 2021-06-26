using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class Recipe_RemoveCorticalStack : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < allHediffs.Count; i++)
			{
				if (allHediffs[i].Part != null && allHediffs[i].def == recipe.removesHediff && allHediffs[i].Visible)
				{
					yield return allHediffs[i].Part;
				}
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			MedicalRecipesUtility.IsClean(pawn, part);
			bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
				{
					return;
				}

				var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_CorticalStack) as Hediff_CorticalStack;
				if (hediff != null)
				{
					if (hediff.def.spawnThingOnRemoved != null)
					{
						var corticalStack = ThingMaker.MakeThing(hediff.def.spawnThingOnRemoved) as CorticalStack;
						hediff.PersonaData.CopyPawn(pawn);
						corticalStack.PersonaData.CopyDataFrom(hediff.PersonaData);
						corticalStack.PersonaData.gender = hediff.PersonaData.gender;
						corticalStack.PersonaData.race = hediff.PersonaData.race;
						//if (hediff.stackGroupID == 0)
                        //{
						//	corticalStack.stackGroupID = AlteredCarbonManager.Instance.GetStackGroupID(corticalStack);
						//}
						GenPlace.TryPlaceThing(corticalStack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
						if (AlteredCarbonManager.Instance.stacksIndex == null) AlteredCarbonManager.Instance.stacksIndex = new Dictionary<int, CorticalStack>();
						AlteredCarbonManager.Instance.stacksIndex[pawn.thingIDNumber] = corticalStack;
						AlteredCarbonManager.Instance.ReplacePawnWithStack(pawn, corticalStack);
						AlteredCarbonManager.Instance.RegisterSleeve(pawn, hediff.PersonaData.stackGroupID);
						//Log.Message("corticalStack.stackGroupID: " + corticalStack.stackGroupID, true);
						//Log.Message("hediff.stackGroupID: " + hediff.stackGroupID, true);
					}
					AlteredCarbonManager.Instance.deadPawns.Add(pawn);
					var head = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
					if (head != null)
					{
						pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, head));
					}
					pawn.health.RemoveHediff(hediff);
				}

			}
			if (flag)
			{
				ReportViolation(pawn, billDoer, pawn.Faction, -70);
			}
		}
	}
}

