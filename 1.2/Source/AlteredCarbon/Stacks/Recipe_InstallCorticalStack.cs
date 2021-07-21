using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
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
                pawn.health.AddHediff(hediff, part);
                if (corticalStack.hasPawn)
                {
                    hediff.gender = corticalStack.gender;
                    hediff.race = corticalStack.race;
                    if (pawn.IsColonist)
                    {
                        Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
                    }
                    PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, PawnDiedOrDownedThoughtsKind.Died);
                    pawn.health.NotifyPlayerOfKilled(null, null, null);
                    ACUtils.ACTracker.stacksIndex.Remove(corticalStack.pawnID + corticalStack.name);
                    corticalStack.OverwritePawn(pawn);
                    hediff.stackGroupID = corticalStack.stackGroupID;
                    ACUtils.ACTracker.ReplaceStackWithPawn(corticalStack, pawn);

                    var naturalMood = pawn.story.traits.GetTrait(TraitDefOf.NaturalMood);
                    var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves);

                    if ((naturalMood != null && naturalMood.Degree == -2)
                            || pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)
                            || (nerves != null && nerves.Degree == -2)
                            || pawn.story.traits.HasTrait(AlteredCarbonDefOf.AC_Sleever))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeveDouble);
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_NewSleeve);
                    }
                }
                else
                {
                    hediff.gender = pawn.gender;
                    hediff.race = pawn.kindDef.race;
                    if (ACUtils.ACTracker.stacksRelationships != null)
                    {
                        hediff.stackGroupID = ACUtils.ACTracker.stacksRelationships.Count + 1;
                    }
                    else
                    {
                        hediff.stackGroupID = 0;
                    }
                    ACUtils.ACTracker.RegisterPawn(pawn);
                    ACUtils.ACTracker.TryAddRelationships(pawn);
                }

                var additionalSleeveBodyData = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_SleeveBodyData) as Hediff_SleeveBodyStats;
                if (additionalSleeveBodyData != null)
                {
                    additionalSleeveBodyData.ApplyEffects();
                }
                if (ACUtils.ACTracker.emptySleeves != null && ACUtils.ACTracker.emptySleeves.Contains(pawn))
                {
                    ACUtils.ACTracker.emptySleeves.Remove(pawn);
                }
            }
        }
    }
}
