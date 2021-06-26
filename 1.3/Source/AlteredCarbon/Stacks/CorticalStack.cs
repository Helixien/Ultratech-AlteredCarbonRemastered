using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class CorticalStack : ThingWithComps
    {
        public static HashSet<CorticalStack> corticalStacks = new HashSet<CorticalStack>();

        private PersonaData personaData;
        public PersonaData PersonaData
        {
            get
            {
                if (personaData is null)
                {
                    personaData = new PersonaData();
                }
                return personaData;
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            corticalStacks.Add(this);
            try
            {
                if (!respawningAfterLoad && !PersonaData.hasPawn && this.def == AC_DefOf.AC_FilledCorticalStack)
                {
                    var pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike).RandomElement();
                    var faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
                    Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
                    PersonaData.CopyPawn(pawn);
                    PersonaData.gender = pawn.gender;
                    PersonaData.race = pawn.kindDef.race;
                    PersonaData.stackGroupID = AlteredCarbonManager.Instance.GetStackGroupID(this);
                    AlteredCarbonManager.Instance.RegisterStack(this);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception spawning " + this + ": " + ex);
            }
            if (this.def == AC_DefOf.AC_FilledCorticalStack && this.stackCount != 1)
            {
                this.stackCount = 1;
            }
            base.SpawnSetup(map, respawningAfterLoad);
        }

        //public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        //{
        //    if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false))
        //    {
        //        FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
        //            MenuOptionPriority.Default, null, null, 0f, null, null);
        //        yield return floatMenuOption;
        //    }
        //    else if (this.def == AlteredCarbonDefOf.AC_FilledCorticalStack && myPawn.skills.GetSkill(SkillDefOf.Intellectual).Level >= 5)
        //    {
        //        string label = "AlteredCarbon.WipeStack".Translate();
        //        Action action = delegate ()
        //        {
        //            Job job = JobMaker.MakeJob(AlteredCarbonDefOf.AC_WipeStack, this);
        //            job.count = 1;
        //            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        //        };
        //        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
        //                (label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
        //                this, "ReservedBy");
        //    }
        //    else if (this.def == AlteredCarbonDefOf.AC_FilledCorticalStack && myPawn.skills.GetSkill(SkillDefOf.Intellectual).Level < 5)
        //    {
        //        FloatMenuOption floatMenuOption = new FloatMenuOption("AlteredCarbon.CantWipeStackTooDumb".Translate(), null,
        //            MenuOptionPriority.Default, null, null, 0f, null, null);
        //        yield return floatMenuOption;
        //    }
        //    string label2 = "AlteredCarbon.DestroyStack".Translate();
        //    Action action2 = delegate ()
        //    {
        //        Job job = JobMaker.MakeJob(AlteredCarbonDefOf.AC_DestroyStack, this);
        //        job.count = 1;
        //        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        //    };
        //    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
        //            (label2, action2, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
        //            this, "ReservedBy");
        //    yield break;
        //}

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (PersonaData.hasPawn)
            {
                stringBuilder.AppendLineTagged("AlteredCarbon.Name".Translate() + ": " + personaData.PawnNameColored);
                stringBuilder.AppendLineTagged("AlteredCarbon.faction".Translate() + ": " + PersonaData.faction.NameColored);
                Backstory newChildhood = null;
                BackstoryDatabase.TryGetWithIdentifier(PersonaData.childhood, out newChildhood, true);
                stringBuilder.Append("AlteredCarbon.childhood".Translate() + ": " + newChildhood.title.CapitalizeFirst() + "\n");

                if (PersonaData.adulthood?.Length > 0)
                {
                    Backstory newAdulthood = null;
                    BackstoryDatabase.TryGetWithIdentifier(PersonaData.adulthood, out newAdulthood, true);
                    stringBuilder.Append("AlteredCarbon.adulthood".Translate() + ": " + newAdulthood.title.CapitalizeFirst() + "\n");
                }
                stringBuilder.Append("AlteredCarbon.ageChronologicalTicks".Translate() + ": " + (int)(PersonaData.ageChronologicalTicks / 3600000) + "\n");
                stringBuilder.Append("Gender".Translate() + ": " + PersonaData.gender.GetLabel().CapitalizeFirst() + "\n");

            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (this.Destroyed)
            {
                this.KillInnerPawn();
            }
        }

        public void DestroyWithConfirmation()
        {
            Find.WindowStack.Add(new Dialog_MessageBox("AlteredCarbon.DestroyStackConfirmation".Translate(),
                    "No".Translate(), null,
                    "Yes".Translate(), delegate ()
            {
                this.Destroy();
            }, null, false, null, null));
        }

        public bool dontKillThePawn = false;

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            corticalStacks.Remove(this);
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            corticalStacks.Remove(this);
            base.Destroy(mode);
            if (PersonaData.hasPawn && !dontKillThePawn)
            {
                this.KillInnerPawn();
            }
        }

        public void KillInnerPawn(bool affectFactionRelationship = false, Pawn affecter = null)
        {
            if (PersonaData.hasPawn)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                PersonaData.OverwritePawn(pawn, this.def.GetModExtension<StackSavingOptionsModExtension>());
                if (affectFactionRelationship)
                {
                    PersonaData.faction.TryAffectGoodwillWith(affecter.Faction, -70, canSendMessage: true);
                    QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
                }
                if (PersonaData.isFactionLeader)
                {
                    pawn.Faction.leader = pawn;
                }
                pawn.Kill(null);
            }
            PersonaData.hasPawn = false;
        }
        public void EmptyStack(Pawn affecter, bool affectFactionRelationship = false)
        {
            Find.WindowStack.Add(new Dialog_MessageBox("AlteredCarbon.EmptyStackConfirmation".Translate(),
                "No".Translate(), null,
                "Yes".Translate(), delegate ()
                {
                    float damageChance = Mathf.Abs((affecter.skills.GetSkill(SkillDefOf.Intellectual).levelInt / 2f) - 11f) / 10f;
                    if (Rand.Chance(damageChance))
                    {
                        Find.LetterStack.ReceiveLetter("AlteredCarbon.DestroyedStack".Translate(),
                        "AlteredCarbon.DestroyedWipingStackDesc".Translate(affecter.Named("PAWN")),
                        LetterDefOf.NegativeEvent, affecter);
                        AlteredCarbonManager.Instance.stacksIndex.Remove(PersonaData.pawnID);
                        this.KillInnerPawn(affectFactionRelationship, affecter);
                        this.Destroy();
                    }
                    else
                    {
                        var newStack = ThingMaker.MakeThing(AC_DefOf.AC_EmptyCorticalStack);
                        GenSpawn.Spawn(newStack, this.Position, this.Map);
                        Find.Selector.Select(newStack);
                        AlteredCarbonManager.Instance.stacksIndex.Remove(PersonaData.pawnID);
                        this.KillInnerPawn(affectFactionRelationship, affecter);
                        this.Destroy();
                    }
                }, null, false, null, null));
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref personaData, "personaData");
        }


    }
}