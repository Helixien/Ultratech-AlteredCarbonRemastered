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

        private GraphicData hostileGraphicData;
        private GraphicData friendlyGraphicData;
        private GraphicData strangerGraphicData;

        private Graphic hostileGraphic;
        private Graphic friendlyGraphic;
        private Graphic strangerGraphic;

        public override Graphic Graphic
        {
            get
            {
                var personaData = this.PersonaData;
                if (personaData.hasPawn)
                {
                    if (personaData.faction == Faction.OfPlayer)
                    {
                        if (friendlyGraphic is null)
                        {
                            if (friendlyGraphicData is null)
                            {
                                friendlyGraphicData = GetGraphicDataWithOtherPath("Things/Item/Stacks/FriendlyStack");
                            }
                            friendlyGraphic = friendlyGraphicData.GraphicColoredFor(this);
                        }
                        return friendlyGraphic;
                    }
                    else if (personaData.faction is null || !personaData.faction.HostileTo(Faction.OfPlayer))
                    {
                        if (strangerGraphic is null)
                        {
                            if (strangerGraphicData is null)
                            {
                                strangerGraphicData = GetGraphicDataWithOtherPath("Things/Item/Stacks/NeutralStack");
                            }
                            strangerGraphic = strangerGraphicData.GraphicColoredFor(this);
                        }
                        return strangerGraphic;
                    }
                    else
                    {
                        if (hostileGraphic is null)
                        {
                            if (hostileGraphicData is null)
                            {
                                hostileGraphicData = GetGraphicDataWithOtherPath("Things/Item/Stacks/HostileStack");
                            }
                            hostileGraphic = hostileGraphicData.GraphicColoredFor(this);
                        }
                        return hostileGraphic;
                    }
                }
                else
                {
                    return base.Graphic;
                } 
            }
        }

        private GraphicData GetGraphicDataWithOtherPath(string texPath)
        {
            return new GraphicData
            {
                texPath = texPath,
                graphicClass = def.graphicData.graphicClass,
                shadowData = def.graphicData.shadowData,
                shaderType = def.graphicData.shaderType,
                shaderParameters = def.graphicData.shaderParameters,
                onGroundRandomRotateAngle = def.graphicData.onGroundRandomRotateAngle,
                linkType = def.graphicData.linkType,
                linkFlags = def.graphicData.linkFlags,
                flipExtraRotation = def.graphicData.flipExtraRotation,
                drawSize = def.graphicData.drawSize,
                drawRotated = !def.graphicData.drawRotated,
                drawOffsetWest = def.graphicData.drawOffsetWest,
                drawOffsetSouth = def.graphicData.drawOffsetSouth,
                drawOffsetNorth = def.graphicData.drawOffsetNorth,
                drawOffsetEast = def.graphicData.drawOffsetEast,
                drawOffset = def.graphicData.drawOffset,
                damageData = def.graphicData.damageData,
                colorTwo = def.graphicData.colorTwo,
                color = def.graphicData.color,
                allowFlip = def.graphicData.allowFlip
            };
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            corticalStacks.Add(this);
            try
            {
                if (!respawningAfterLoad && !PersonaData.hasPawn && this.def == AC_DefOf.UT_FilledCorticalStack)
                {
                    var pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike).RandomElement();
                    var faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
                    Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
                    PersonaData.CopyPawn(pawn);
                    PersonaData.gender = pawn.gender;
                    PersonaData.race = pawn.kindDef.race;
                    PersonaData.stackGroupID = AlteredCarbonManager.Instance.GetStackGroupID(this);
                    AlteredCarbonManager.Instance.RegisterStack(this);
                    AlteredCarbonManager.Instance.stacksIndex[pawn.thingIDNumber] = this;

                    if (LookTargets_Patch.targets.TryGetValue(pawn, out var targets))
                    {
                        foreach (var target in targets)
                        {
                            target.targets.Remove(pawn);
                            target.targets.Add(this);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception spawning " + this + ": " + ex);
            }
            if (this.def == AC_DefOf.UT_FilledCorticalStack && this.stackCount != 1)
            {
                this.stackCount = 1;
            }
            base.SpawnSetup(map, respawningAfterLoad);
        }
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
            var newStack = ThingMaker.MakeThing(AC_DefOf.UT_EmptyCorticalStack);
            GenPlace.TryPlaceThing(newStack, affecter.Position, affecter.Map, ThingPlaceMode.Near);
            AlteredCarbonManager.Instance.stacksIndex.Remove(PersonaData.pawnID);
            this.KillInnerPawn(affectFactionRelationship, affecter);
            this.Destroy();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref personaData, "personaData");
        }


    }
}