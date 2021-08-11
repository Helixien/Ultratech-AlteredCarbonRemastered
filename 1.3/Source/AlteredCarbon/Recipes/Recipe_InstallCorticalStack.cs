using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_InstallFilledCorticalStack : Recipe_InstallCorticalStack
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
    }
    public class Recipe_InstallCorticalStack : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
            {
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
                {
                    return false;
                }
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
                {
                    return false;
                }
                return (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && (x.def == recipe.addsHediff || !recipe.CompatibleWithHediff(x.def)))) ? true : false;
            });
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            if (ingredient is CorticalStack c)
            {
                c.dontKillThePawn = true;
            }
            base.ConsumeIngredient(ingredient, recipe, map);
        }
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    foreach (var i in ingredients)
                    {
                        if (i is CorticalStack c)
                        {
                            c.stackCount = 1;
                            Traverse.Create(c).Field("mapIndexOrState").SetValue((sbyte)-1);
                            GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        }
                    }
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }

            var thing = ingredients.Where(x => x is CorticalStack).FirstOrDefault();
            if (thing is CorticalStack corticalStack)
            {
                var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_CorticalStack;
                if (corticalStack.PersonaData.hasPawn)
                {
                    hediff.PersonaData.gender = corticalStack.PersonaData.gender;
                    hediff.PersonaData.race = corticalStack.PersonaData.race;
                    if (pawn.IsColonist)
                    {
                        Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
                    }
                    if (!pawn.IsEmptySleeve())
                    {
                        PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, PawnDiedOrDownedThoughtsKind.Died);
                    }
                    pawn.health.NotifyPlayerOfKilled(null, null, null);
                    corticalStack.PersonaData.OverwritePawn(pawn, corticalStack.def.GetModExtension<StackSavingOptionsModExtension>());
                    hediff.PersonaData.stackGroupID = corticalStack.PersonaData.stackGroupID;
                    pawn.health.AddHediff(hediff, part);
                    AlteredCarbonManager.Instance.stacksIndex.Remove(corticalStack.PersonaData.pawnID);
                    AlteredCarbonManager.Instance.ReplaceStackWithPawn(corticalStack, pawn);

                    var naturalMood = pawn.story.traits.GetTrait(TraitDefOf.NaturalMood);
                    var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves);

                    if ((naturalMood != null && naturalMood.Degree == -2)
                            || pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)
                            || (nerves != null && nerves.Degree == -2))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_NewSleeveDouble);
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_NewSleeve);
                    }

                    if (corticalStack.PersonaData.diedFromCombat.HasValue && corticalStack.PersonaData.diedFromCombat.Value)
                    {
                        pawn.health.AddHediff(HediffMaker.MakeHediff(AC_DefOf.UT_SleeveShock, pawn));
                        corticalStack.PersonaData.diedFromCombat = null;
                    }
                    if (corticalStack.PersonaData.hackedWhileOnStack)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_SomethingIsWrong);
                        corticalStack.PersonaData.hackedWhileOnStack = false;
                    }
                }
                else
                {
                    pawn.health.AddHediff(hediff, part);
                }

                var additionalSleeveBodyData = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_SleeveBodyData) as Hediff_SleeveBodyStats;
                if (additionalSleeveBodyData != null)
                {
                    additionalSleeveBodyData.ApplyEffects();
                }
                if (AlteredCarbonManager.Instance.emptySleeves != null && AlteredCarbonManager.Instance.emptySleeves.Contains(pawn))
                {
                    AlteredCarbonManager.Instance.emptySleeves.Remove(pawn);
                }

                if (ModsConfig.IdeologyActive)
                {
                    var eventDef = DefDatabase<HistoryEventDef>.GetNamed("UT_InstalledCorticalStack");
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(eventDef, pawn.Named(HistoryEventArgsNames.Doer)));
                }
            }
        }
    }
}