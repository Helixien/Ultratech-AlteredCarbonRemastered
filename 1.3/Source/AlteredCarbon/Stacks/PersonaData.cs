using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class PersonaData : IExposable
    {
        public Name name;
        private Pawn origPawn;
        private int hostilityMode;
        private Area areaRestriction;
        private MedicalCareCategory medicalCareCategory;
        private bool selfTend;
        public long ageChronologicalTicks;
        private List<TimeAssignmentDef> times;
        private FoodRestriction foodRestriction;
        private Outfit outfit;
        private DrugPolicy drugPolicy;
        public Faction faction;
        public bool isFactionLeader;
        private List<Thought_Memory> thoughts;
        private List<Trait> traits;
        private List<DirectPawnRelation> relations;
        private HashSet<Pawn> relatedPawns;
        private List<SkillRecord> skills;
        public string childhood;
        public string adulthood;
        public string title;

        private Dictionary<WorkTypeDef, int> priorities;

        private GuestStatus guestStatusInt;
        private PrisonerInteractionModeDef interactionMode;
        private SlaveInteractionModeDef slaveInteractionMode;
        private Faction hostFactionInt;
        private JoinStatus joinStatus;
        private Faction slaveFactionInt;
        private string lastRecruiterName;
        private int lastRecruiterOpinion;
        private bool hasOpinionOfLastRecruiter;
        private float lastRecruiterResistanceReduce;
        private bool releasedInt;
        private int ticksWhenAllowedToEscapeAgain;
        public IntVec3 spotToWaitInsteadOfEscaping;
        public int lastPrisonBreakTicks = -1;
        public bool everParticipatedInPrisonBreak;
        public float resistance = -1f;
        public float will = -1f;
        public Ideo ideoForConversion;
        private bool everEnslaved = false;
        public bool getRescuedThoughtOnUndownedBecauseOfPlayer;


        public bool hasPawn = false;
        public Gender gender;
        public ThingDef race;
        public int pawnID;

        // Royalty
        private List<RoyalTitle> royalTitles;
        private Dictionary<Faction, int> favor = new Dictionary<Faction, int>();
        private Dictionary<Faction, Pawn> heirs = new Dictionary<Faction, Pawn>();
        private List<Thing> bondedThings = new List<Thing>();
        private List<FactionPermit> factionPermits = new List<FactionPermit>();
        private Dictionary<Faction, int> permitPoints = new Dictionary<Faction, int>();

        // Ideology
        private Ideo ideo;
        private Color? favoriteColor;
        private int joinTick;
        private List<Ideo> previousIdeos;
        private float certainty;
        private Precept_RoleMulti precept_RoleMulti;
        private Precept_RoleSingle precept_RoleSingle;

        // [SYR] Individuality
        private int sexuality;
        private float romanceFactor;

        // Psychology
        private PsychologyData psychologyData;

        // misc
        public bool isCopied = false;
        public int stackGroupID;

        public List<SkillOffsets> negativeSkillsOffsets;
        public List<SkillOffsets> negativeSkillPassionsOffsets;
        public TaggedString PawnNameColored
        {
            get
            {
                Log.Message(this + " - " + this.name);
                if (TitleShort?.CapitalizeFirst().NullOrEmpty() ?? false)
                {
                    return this.name.ToStringShort.Colorize(GetFactionRelationColor(this.faction));
                }
                return this.name.ToStringShort.Colorize(GetFactionRelationColor(this.faction)) + ", " + TitleShort.CapitalizeFirst();
            }
        }
        public string TitleShort
        {
            get
            {
                if (title != null)
                {
                    return title;
                }
                if (BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out var newAdulthood, true))
                {
                    return newAdulthood.TitleShortFor(gender);
                }
                if (BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out var newChildhood, true))
                {
                    return newChildhood.TitleShortFor(gender);
                }
                return "";
            }
        }
        private Color GetFactionRelationColor(Faction faction)
        {
            if (faction == null)
            {
                return Color.white;
            }
            if (faction.IsPlayer)
            {
                return faction.Color;
            }
            switch (faction.RelationKindWith(Faction.OfPlayer))
            {
                case FactionRelationKind.Ally:
                    return ColoredText.FactionColor_Ally;
                case FactionRelationKind.Hostile:
                    return ColoredText.FactionColor_Hostile;
                case FactionRelationKind.Neutral:
                    return ColoredText.FactionColor_Neutral;
                default:
                    return faction.Color;
            }
        }
        public void CopyPawn(Pawn pawn)
        {
            this.name = pawn.Name;
            this.origPawn = pawn;
            if (pawn.playerSettings != null)
            {
                this.hostilityMode = (int)pawn.playerSettings.hostilityResponse;
                this.areaRestriction = pawn.playerSettings.AreaRestriction;
                this.medicalCareCategory = pawn.playerSettings.medCare;
                this.selfTend = pawn.playerSettings.selfTend;
            }
            if (pawn.ageTracker != null)
            {
                this.ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
            }
            this.foodRestriction = pawn.foodRestriction?.CurrentFoodRestriction;
            this.outfit = pawn.outfits?.CurrentOutfit;
            this.drugPolicy = pawn.drugs?.CurrentPolicy;
            this.times = pawn.timetable?.times;
            this.thoughts = pawn.needs?.mood?.thoughts?.memories?.Memories;
            this.faction = pawn.Faction;
            if (pawn.Faction.leader == pawn)
            {
                this.isFactionLeader = true;
            }
            this.traits = pawn.story?.traits?.allTraits;
            this.relations = pawn.relations?.DirectRelations;
            this.relatedPawns = pawn.relations?.RelatedPawns?.ToHashSet();
            foreach (var otherPawn in pawn.relations.RelatedPawns)
            {
                foreach (var rel2 in pawn.GetRelations(otherPawn))
                {
                    if (this.relations.Where(r => r.def == rel2 && r.otherPawn == otherPawn).Count() == 0)
                    {
                        //Log.Message("00000 Rel: " + otherPawn?.Name + " - " + rel2 + " - " + pawn.Name, true);
                        if (!rel2.implied)
                        {
                            this.relations.Add(new DirectPawnRelation(rel2, otherPawn, 0));
                        }
                    }
                }
                relatedPawns.Add(otherPawn);
            }
            this.skills = pawn.skills?.skills;
            this.childhood = pawn.story?.childhood?.identifier;
            if (pawn.story?.adulthood != null)
            {
                this.adulthood = pawn.story.adulthood.identifier;
            }
            this.title = pawn.story?.title;

            this.priorities = new Dictionary<WorkTypeDef, int>();
            if (pawn.workSettings != null && Traverse.Create(pawn.workSettings).Field("priorities").GetValue<DefMap<WorkTypeDef, int>>() != null)
            {
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    this.priorities[w] = pawn.workSettings.GetPriority(w);
                }
            }

            this.guestStatusInt = pawn.guest.GuestStatus;
            this.interactionMode = pawn.guest.interactionMode;
            this.slaveInteractionMode = pawn.guest.slaveInteractionMode;
            this.hostFactionInt = pawn.guest.HostFaction;
            this.joinStatus = pawn.guest.joinStatus;
            this.slaveFactionInt = pawn.guest.SlaveFaction;
            this.lastRecruiterName = pawn.guest.lastRecruiterName;
            this.lastRecruiterOpinion = pawn.guest.lastRecruiterOpinion;
            this.hasOpinionOfLastRecruiter = pawn.guest.hasOpinionOfLastRecruiter;
            this.lastRecruiterResistanceReduce = pawn.guest.lastRecruiterResistanceReduce;
            this.releasedInt = pawn.guest.Released;
            this.ticksWhenAllowedToEscapeAgain = Traverse.Create(pawn.guest).Field("ticksWhenAllowedToEscapeAgain").GetValue<int>();
            this.spotToWaitInsteadOfEscaping = pawn.guest.spotToWaitInsteadOfEscaping;
            this.lastPrisonBreakTicks = pawn.guest.lastPrisonBreakTicks;
            this.everParticipatedInPrisonBreak = pawn.guest.everParticipatedInPrisonBreak;
            this.resistance = pawn.guest.resistance;
            this.will = pawn.guest.will;
            this.ideoForConversion = pawn.guest.ideoForConversion;
            this.everEnslaved = pawn.guest.EverEnslaved;
            this.getRescuedThoughtOnUndownedBecauseOfPlayer = pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer;

            this.hasPawn = true;
            this.pawnID = pawn.thingIDNumber;
            if (ModsConfig.RoyaltyActive && pawn.royalty != null)
            {
                this.royalTitles = pawn.royalty?.AllTitlesForReading;
                this.favor = Traverse.Create(pawn.royalty).Field("favor").GetValue<Dictionary<Faction, int>>();
                this.heirs = Traverse.Create(pawn.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>();
                foreach (var map in Find.Maps)
                {
                    foreach (var thing in map.listerThings.AllThings)
                    {
                        var comp = thing.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            this.bondedThings.Add(thing);
                        }
                    }
                    foreach (var gear in pawn.apparel?.WornApparel)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.equipment?.AllEquipmentListForReading)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.inventory?.innerContainer)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                }
                this.factionPermits = Traverse.Create(pawn.royalty).Field("factionPermits").GetValue<List<FactionPermit>>();
                this.permitPoints = Traverse.Create(pawn.royalty).Field("permitPoints").GetValue<Dictionary<Faction, int>>();
            }

            if (ModsConfig.IdeologyActive)
            {
                if (pawn.Ideo != null)
                {
                    this.ideo = pawn.Ideo;
                    this.certainty = pawn.ideo.Certainty;
                    this.previousIdeos = pawn.ideo.PreviousIdeos;
                    this.joinTick = pawn.ideo.joinTick;

                    var role = pawn.Ideo.GetRole(pawn);
                    if (role is Precept_RoleMulti multi)
                    {
                        this.precept_RoleMulti = multi;
                        this.precept_RoleSingle = null;
                    }
                    else if (role is Precept_RoleSingle single)
                    {
                        this.precept_RoleMulti = null;
                        this.precept_RoleSingle = single;
                    }
                }

                if (pawn.story.favoriteColor.HasValue)
                {
                    this.favoriteColor = pawn.story.favoriteColor.Value;
                }
            }

            if (ModCompatibility.IndividualityIsActive)
            {
                this.sexuality = ModCompatibility.GetSyrTraitsSexuality(pawn);
                this.romanceFactor = ModCompatibility.GetSyrTraitsRomanceFactor(pawn);
            }
            if (ModCompatibility.PsychologyIsActive)
            {
                this.psychologyData = ModCompatibility.GetPsychologyData(pawn);
            }
        }
        public void CopyDataFrom(PersonaData other, bool isDuplicateOperation = false)
        {
            this.name = other.name;
            this.origPawn = other.origPawn;
            this.hostilityMode = other.hostilityMode;
            this.areaRestriction = other.areaRestriction;
            this.ageChronologicalTicks = other.ageChronologicalTicks;
            this.medicalCareCategory = other.medicalCareCategory;
            this.selfTend = other.selfTend;
            this.foodRestriction = other.foodRestriction;
            this.outfit = other.outfit;
            this.drugPolicy = other.drugPolicy;
            this.times = other.times;
            this.thoughts = other.thoughts;
            this.faction = other.faction;
            this.isFactionLeader = other.isFactionLeader;
            this.traits = other.traits;
            this.relations = other.relations;
            this.relatedPawns = other.relatedPawns;
            this.skills = other.skills;

            if (other.negativeSkillsOffsets != null)
            {
                foreach (var negativeOffset in other.negativeSkillsOffsets)
                {
                    var skill = this.skills.Where(x => x.def == negativeOffset.skill).FirstOrDefault();
                    if (skill != null)
                    {
                        skill.Level += negativeOffset.offset;
                    }
                }
            }
            if (other.negativeSkillPassionsOffsets != null)
            {
                foreach (var negativeOffset in other.negativeSkillPassionsOffsets)
                {
                    var skill = this.skills.Where(x => x.def == negativeOffset.skill).FirstOrDefault();
                    if (skill != null)
                    {
                        var finalValue = (int)skill.passion + negativeOffset.offset + 1;
                        //Log.Message("finalValue: " + finalValue, true);
                        if (finalValue <= 2)
                        {
                            switch (finalValue)
                            {
                                case 0:
                                    skill.passion = Passion.None;
                                    //Log.Message(skill.def + " - finalValue: " + finalValue + " - skill.passion = Passion.None");
                                    break;
                                case 1:
                                    skill.passion = Passion.Minor;
                                    //Log.Message(skill.def + " - finalValue: " + finalValue + " - skill.passion = Passion.Minor");
                                    break;
                                case 2:
                                    skill.passion = Passion.Major;
                                    //Log.Message(skill.def + " - finalValue: " + finalValue + " - skill.passion = Passion.Major");
                                    break;
                                default:
                                    skill.passion = Passion.None;
                                    //Log.Message("default: " + skill.def + " - finalValue: " + finalValue + " - skill.passion = Passion.None");
                                    break;
                            }
                        }
                        else
                        {
                            skill.passion = Passion.None;
                            //Log.Message("2 default: " + skill.def + " - finalValue: " + finalValue + " - skill.passion = Passion.None");
                        }
                    }
                }
            }

            this.childhood = other.childhood;
            this.adulthood = other.adulthood;
            this.title = other.title;
            this.priorities = other.priorities;

            this.guestStatusInt = other.guestStatusInt;
            this.interactionMode = other.interactionMode;
            this.slaveInteractionMode = other.slaveInteractionMode;
            this.hostFactionInt = other.hostFactionInt;
            this.joinStatus = other.joinStatus;
            this.slaveFactionInt = other.slaveFactionInt;
            this.lastRecruiterName = other.lastRecruiterName;
            this.lastRecruiterOpinion = other.lastRecruiterOpinion;
            this.hasOpinionOfLastRecruiter = other.hasOpinionOfLastRecruiter;
            this.lastRecruiterResistanceReduce = other.lastRecruiterResistanceReduce;
            this.releasedInt = other.releasedInt;
            this.ticksWhenAllowedToEscapeAgain = other.ticksWhenAllowedToEscapeAgain;
            this.spotToWaitInsteadOfEscaping = other.spotToWaitInsteadOfEscaping;
            this.lastPrisonBreakTicks = other.lastPrisonBreakTicks;
            this.everParticipatedInPrisonBreak = other.everParticipatedInPrisonBreak;
            this.resistance = other.resistance;
            this.will = other.will;
            this.ideoForConversion = other.ideoForConversion;
            this.everEnslaved = other.everEnslaved;
            this.getRescuedThoughtOnUndownedBecauseOfPlayer = other.getRescuedThoughtOnUndownedBecauseOfPlayer;

            this.hasPawn = true;

            if (this.gender == Gender.None)
            {
                this.gender = other.gender;
            }
            if (this.race == null)
            {
                this.race = other.race;
            }


            this.pawnID = other.pawnID;

            if (ModsConfig.RoyaltyActive)
            {
                this.royalTitles = other.royalTitles;
                this.favor = other.favor;
                this.heirs = other.heirs;
                this.bondedThings = other.bondedThings;
                this.permitPoints = other.permitPoints;
                this.factionPermits = other.factionPermits;
            }
            if (ModsConfig.IdeologyActive)
            {
                this.ideo = other.ideo;
                this.previousIdeos = other.previousIdeos;
                this.joinTick = other.joinTick;
                this.certainty = other.certainty;

                this.precept_RoleSingle = other.precept_RoleSingle;
                this.precept_RoleMulti = other.precept_RoleMulti;

                if (other.favoriteColor.HasValue)
                {
                    this.favoriteColor = other.favoriteColor.Value;
                }
            }

            this.isCopied = isDuplicateOperation ? true : other.isCopied;
            this.stackGroupID = other.stackGroupID;

            this.sexuality = other.sexuality;
            this.romanceFactor = other.romanceFactor;
            this.psychologyData = other.psychologyData;
        }

        public void OverwritePawn(Pawn pawn, StackSavingOptionsModExtension extension)
        {
            if (pawn.Faction != this.faction)
            {
                pawn.SetFaction(this.faction);
            }
            if (this.isFactionLeader)
            {
                pawn.Faction.leader = pawn;
            }

            pawn.Name = this.name;
            if (pawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = pawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]);
                }
            }

            if (this.thoughts != null)
            {
                if (this.gender == pawn.gender)
                {
                    this.thoughts.RemoveAll(x => x.def == AC_DefOf.AC_WrongGender);
                    this.thoughts.RemoveAll(x => x.def == AC_DefOf.AC_WrongGenderDouble);
                }
                if (this.race == pawn.kindDef.race)
                {
                    this.thoughts.RemoveAll(x => x.def == AC_DefOf.AC_WrongRace);
                }

                foreach (var thought in this.thoughts)
                {
                    if (thought is Thought_MemorySocial && thought.otherPawn == null)
                    {
                        continue;
                    }
                    try
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                    }
                    catch { }
                }
            }
            if (extension != null)
            {
                pawn.story.traits.allTraits.RemoveAll(x => !extension.ignoresTraits.Contains(x.def.defName));
            }
            if (this.traits != null)
            {
                foreach (var trait in this.traits)
                {
                    if (extension != null && extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                    {
                        continue;
                    }
                    pawn.story.traits.GainTrait(trait);
                }
            }

            pawn.relations.ClearAllRelations();
            var origPawn = GetOriginalPawn(pawn);

            foreach (var pawn2 in this.relatedPawns)
            {
                var otherPawn = GetTruePawn(pawn2);
                if (otherPawn != null)
                {
                    foreach (var rel in otherPawn.relations.DirectRelations)
                    {
                        if (this.name.ToStringFull == rel.otherPawn?.Name.ToStringFull)
                        {
                            rel.otherPawn = pawn;
                        }
                    }
                }
            }

            foreach (var pawn2 in this.relatedPawns)
            {
                var otherPawn = GetTruePawn(pawn2);
                if (otherPawn != null)
                {
                    foreach (var rel in this.relations)
                    {
                        foreach (var rel2 in otherPawn.relations.DirectRelations)
                        {
                            if (rel.def == rel2.def && rel2.otherPawn?.Name.ToStringFull == pawn.Name.ToStringFull)
                            {
                                rel2.otherPawn = pawn;
                            }
                        }
                    }
                }
            }

            foreach (var rel in this.relations)
            {
                var otherPawn = GetTruePawn(rel.otherPawn);
                if (otherPawn != null)
                {
                    var oldRelation = otherPawn.relations.DirectRelations.Where(r => r.def == rel.def && r.otherPawn.Name.ToStringFull == pawn.Name.ToStringFull).FirstOrDefault();
                    if (oldRelation != null)
                    {
                        oldRelation.otherPawn = pawn;
                    }
                }
                pawn.relations.AddDirectRelation(rel.def, otherPawn);
            }

            if (origPawn != null)
            {
                origPawn.relations = new Pawn_RelationsTracker(origPawn);
            }


            pawn.skills.skills.Clear();
            if (this.skills != null)
            {
                foreach (var skill in this.skills)
                {
                    var newSkill = new SkillRecord(pawn, skill.def);
                    newSkill.passion = skill.passion;
                    newSkill.levelInt = skill.levelInt;
                    newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                    newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                    pawn.skills.skills.Add(newSkill);
                }
            }
            if (pawn.playerSettings == null) pawn.playerSettings = new Pawn_PlayerSettings(pawn);
            pawn.playerSettings.hostilityResponse = (HostilityResponseMode)this.hostilityMode;

            BackstoryDatabase.TryGetWithIdentifier(this.childhood, out var newChildhood, true);
            pawn.story.childhood = newChildhood;

            BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out var newAdulthood, true);
            pawn.story.adulthood = newAdulthood;
            pawn.story.title = this.title;

            if (pawn.workSettings == null) pawn.workSettings = new Pawn_WorkSettings();
            pawn.Notify_DisabledWorkTypesChanged();
            if (priorities != null)
            {
                foreach (var priority in priorities)
                {
                    pawn.workSettings.SetPriority(priority.Key, priority.Value);
                }
            }

            pawn.guest.guestStatusInt = this.guestStatusInt;
            pawn.guest.interactionMode = this.interactionMode;
            pawn.guest.slaveInteractionMode = this.slaveInteractionMode;
            Traverse.Create(pawn.guest).Field("hostFactionInt").SetValue(this.hostFactionInt);
            pawn.guest.joinStatus = this.joinStatus;
            Traverse.Create(pawn.guest).Field("slaveFactionInt").SetValue(this.slaveFactionInt);
            pawn.guest.lastRecruiterName = this.lastRecruiterName;
            pawn.guest.lastRecruiterOpinion = this.lastRecruiterOpinion;
            pawn.guest.hasOpinionOfLastRecruiter = this.hasOpinionOfLastRecruiter;
            pawn.guest.lastRecruiterResistanceReduce = this.lastRecruiterResistanceReduce;
            pawn.guest.Released = this.releasedInt;
            Traverse.Create(pawn.guest).Field("ticksWhenAllowedToEscapeAgain").SetValue(this.ticksWhenAllowedToEscapeAgain);
            pawn.guest.spotToWaitInsteadOfEscaping = this.spotToWaitInsteadOfEscaping;
            pawn.guest.lastPrisonBreakTicks = this.lastPrisonBreakTicks;
            pawn.guest.everParticipatedInPrisonBreak = this.everParticipatedInPrisonBreak;
            pawn.guest.resistance = this.resistance;
            pawn.guest.will = this.will;
            pawn.guest.ideoForConversion = this.ideoForConversion;
            Traverse.Create(pawn.guest).Field("everEnslaved").SetValue(this.everEnslaved);
            pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = this.getRescuedThoughtOnUndownedBecauseOfPlayer;

            pawn.playerSettings.AreaRestriction = this.areaRestriction;
            pawn.playerSettings.medCare = this.medicalCareCategory;
            pawn.playerSettings.selfTend = this.selfTend;
            if (pawn.foodRestriction == null) pawn.foodRestriction = new Pawn_FoodRestrictionTracker();
            pawn.foodRestriction.CurrentFoodRestriction = this.foodRestriction;
            if (pawn.outfits == null) pawn.outfits = new Pawn_OutfitTracker();
            pawn.outfits.CurrentOutfit = this.outfit;
            if (pawn.drugs == null) pawn.drugs = new Pawn_DrugPolicyTracker();
            pawn.drugs.CurrentPolicy = this.drugPolicy;
            pawn.ageTracker.AgeChronologicalTicks = this.ageChronologicalTicks;
            if (pawn.timetable == null) pawn.timetable = new Pawn_TimetableTracker(pawn);
            if (this.times != null) pawn.timetable.times = this.times;
            if (pawn.gender != this.gender)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist))
                {
                    try
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_WrongGenderDouble);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
                else
                {

                    try
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_WrongGender);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }

            if (pawn.kindDef.race != this.race)
            {
                try
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_WrongRace);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            if (ModsConfig.RoyaltyActive)
            {
                if (pawn.royalty == null) pawn.royalty = new Pawn_RoyaltyTracker(pawn);
                if (this.royalTitles != null)
                {
                    foreach (var title in this.royalTitles)
                    {
                        pawn.royalty.SetTitle(title.faction, title.def, false, false, false);
                    }
                }
                if (this.heirs != null)
                {
                    foreach (var heir in this.heirs)
                    {
                        pawn.royalty.SetHeir(heir.Value, heir.Key);
                    }
                }

                if (this.favor != null)
                {
                    foreach (var fav in this.favor)
                    {
                        pawn.royalty.SetFavor(fav.Key, fav.Value);
                    }
                }

                if (this.bondedThings != null)
                {
                    foreach (var bonded in this.bondedThings)
                    {
                        var comp = bonded.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            comp.CodeFor(pawn);
                        }
                    }
                }
                if (this.factionPermits != null)
                {
                    Traverse.Create(pawn.royalty).Field("factionPermits").SetValue(this.factionPermits);
                }
                if (this.permitPoints != null)
                {
                    Traverse.Create(pawn.royalty).Field("permitPoints").SetValue(this.permitPoints);
                }
            }

            if (ModsConfig.IdeologyActive)
            {
                if (this.ideo != null)
                {
                    pawn.ideo.SetIdeo(this.ideo);
                    Traverse.Create(pawn.ideo).Field("certainty").SetValue(this.certainty);
                    Traverse.Create(pawn.ideo).Field("previousIdeos").SetValue(this.previousIdeos);
                    pawn.ideo.joinTick = this.joinTick;
                }
                if (this.favoriteColor.HasValue)
                {
                    pawn.story.favoriteColor = this.favoriteColor.Value;
                }

                if (this.precept_RoleMulti != null)
                {
                    if (this.precept_RoleMulti.chosenPawns is null)
                    {
                        this.precept_RoleMulti.chosenPawns = new List<IdeoRoleInstance>();
                    }

                    this.precept_RoleMulti.chosenPawns.Add(new IdeoRoleInstance(this.precept_RoleMulti)
                    {
                        pawn = pawn
                    });
                }
                if (this.precept_RoleSingle != null)
                {
                    this.precept_RoleSingle.chosenPawn = new IdeoRoleInstance(this.precept_RoleMulti)
                    {
                        pawn = pawn
                    };
                }
            }

            if (ModCompatibility.IndividualityIsActive)
            {
                ModCompatibility.SetSyrTraitsSexuality(pawn, this.sexuality);
                ModCompatibility.SetSyrTraitsRomanceFactor(pawn, this.romanceFactor);
            }

            if (ModCompatibility.PsychologyIsActive)
            {
                ModCompatibility.SetPsychologyData(pawn, this.psychologyData);
            }
        }

        private Pawn GetOriginalPawn(Pawn pawn)
        {
            if (this.origPawn != null)
            {
                return this.origPawn;
            }
            if (this.relatedPawns != null)
            {
                foreach (var otherPawn in this.relatedPawns)
                {
                    if (otherPawn != null && otherPawn.relations?.DirectRelations != null)
                    {
                        foreach (var rel in otherPawn.relations.DirectRelations)
                        {
                            if (rel?.otherPawn?.Name != null && pawn.Name != null)
                            {
                                if (rel.otherPawn.Name.ToStringFull == pawn.Name.ToStringFull && rel.otherPawn != pawn)
                                {
                                    return rel.otherPawn;
                                }
                            }

                        }
                    }
                }
            }
            foreach (var otherPawn in PawnsFinder.AllMaps)
            {
                if (otherPawn?.Name != null && otherPawn.Name.ToStringFull == pawn.Name.ToStringFull && otherPawn != pawn)
                {
                    return otherPawn;
                }
            }

            return null;
        }



        private Pawn GetTruePawn(Pawn pawn)
        {
            if (pawn.Dead && AlteredCarbonManager.Instance.pawnsWithStacks != null)
            {
                foreach (var otherPawn in AlteredCarbonManager.Instance.pawnsWithStacks)
                {
                    if (otherPawn != null && otherPawn.Name.ToStringFull == pawn.Name.ToStringFull)
                    {
                        return otherPawn;
                    }
                }
            }
            return pawn;
        }
        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.stackGroupID, "stackGroupID", 0);
            Scribe_Values.Look<bool>(ref this.isCopied, "isCopied", false, false);
            Scribe_Deep.Look<Name>(ref this.name, "name", new object[0]);
            Scribe_References.Look<Pawn>(ref this.origPawn, "origPawn", true);
            Scribe_Values.Look<int>(ref this.hostilityMode, "hostilityMode");
            Scribe_References.Look<Area>(ref this.areaRestriction, "areaRestriction", false);
            Scribe_Values.Look<MedicalCareCategory>(ref this.medicalCareCategory, "medicalCareCategory", 0, false);
            Scribe_Values.Look<bool>(ref this.selfTend, "selfTend", false, false);
            Scribe_Values.Look<long>(ref this.ageChronologicalTicks, "ageChronologicalTicks", 0, false);
            Scribe_Defs.Look<ThingDef>(ref this.race, "race");
            Scribe_References.Look<Outfit>(ref this.outfit, "outfit", false);
            Scribe_References.Look<FoodRestriction>(ref this.foodRestriction, "foodPolicy", false);
            Scribe_References.Look<DrugPolicy>(ref this.drugPolicy, "drugPolicy", false);

            Scribe_Collections.Look<TimeAssignmentDef>(ref this.times, "times");
            Scribe_Collections.Look<Thought_Memory>(ref this.thoughts, "thoughts");
            Scribe_References.Look<Faction>(ref this.faction, "faction", true);
            Scribe_Values.Look<bool>(ref this.isFactionLeader, "isFactionLeader", false, false);

            Scribe_Values.Look<string>(ref this.childhood, "childhood", null, false);
            Scribe_Values.Look<string>(ref this.adulthood, "adulthood", null, false);
            Scribe_Values.Look<string>(ref this.title, "title", null, false);

            Scribe_Values.Look<int>(ref this.pawnID, "pawnID", 0, false);
            Scribe_Collections.Look<Trait>(ref this.traits, "traits");
            Scribe_Collections.Look<SkillRecord>(ref this.skills, "skills");
            Scribe_Collections.Look<DirectPawnRelation>(ref this.relations, "relations");
            Scribe_Collections.Look<Pawn>(ref this.relatedPawns, saveDestroyedThings: true, "relatedPawns", LookMode.Reference);

            Scribe_Collections.Look<WorkTypeDef, int>(ref this.priorities, "priorities");


            Scribe_Values.Look(ref guestStatusInt, "guestStatusInt");
            Scribe_Defs.Look(ref interactionMode, "interactionMode");
            Scribe_Defs.Look(ref slaveInteractionMode, "slaveInteractionMode");
            Scribe_References.Look(ref hostFactionInt, "hostFactionInt");
            Scribe_References.Look(ref slaveFactionInt, "slaveFactionInt");
            Scribe_Values.Look(ref joinStatus, "joinStatus");
            Scribe_Values.Look(ref lastRecruiterName, "lastRecruiterName");
            Scribe_Values.Look(ref lastRecruiterOpinion, "lastRecruiterOpinion");
            Scribe_Values.Look(ref hasOpinionOfLastRecruiter, "hasOpinionOfLastRecruiter");
            Scribe_Values.Look(ref lastRecruiterResistanceReduce, "lastRecruiterResistanceReduce");
            Scribe_Values.Look(ref releasedInt, "releasedInt");
            Scribe_Values.Look(ref ticksWhenAllowedToEscapeAgain, "ticksWhenAllowedToEscapeAgain");
            Scribe_Values.Look(ref spotToWaitInsteadOfEscaping, "spotToWaitInsteadOfEscaping");
            Scribe_Values.Look(ref lastPrisonBreakTicks, "lastPrisonBreakTicks");
            Scribe_Values.Look(ref everParticipatedInPrisonBreak, "everParticipatedInPrisonBreak");
            Scribe_Values.Look(ref resistance, "resistance");
            Scribe_Values.Look(ref will, "will");
            Scribe_References.Look(ref ideoForConversion, "ideoForConversion");
            Scribe_Values.Look(ref everEnslaved, "everEnslaved");
            Scribe_Values.Look(ref getRescuedThoughtOnUndownedBecauseOfPlayer, "getRescuedThoughtOnUndownedBecauseOfPlayer");

            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);
            Scribe_Values.Look<Gender>(ref this.gender, "gender", 0, false);
            if (ModsConfig.RoyaltyActive)
            {
                Scribe_Collections.Look<Faction, int>(ref this.favor, "favor", LookMode.Reference, LookMode.Value, ref this.favorKeys, ref this.favorValues);
                Scribe_Collections.Look<Faction, Pawn>(ref this.heirs, "heirs", LookMode.Reference, LookMode.Reference, ref this.heirsKeys, ref this.heirsValues);
                Scribe_Collections.Look<Thing>(ref this.bondedThings, "bondedThings", LookMode.Reference);
                Scribe_Collections.Look<RoyalTitle>(ref this.royalTitles, "royalTitles", LookMode.Deep);
                Scribe_Collections.Look(ref permitPoints, "permitPoints", LookMode.Reference, LookMode.Value, ref tmpPermitFactions, ref tmpPermitPointsAmounts);
                Scribe_Collections.Look(ref factionPermits, "permits", LookMode.Deep);
            }
            if (ModsConfig.IdeologyActive)
            {
                Scribe_References.Look(ref ideo, "ideo", saveDestroyedThings: true);
                Scribe_Collections.Look(ref previousIdeos, saveDestroyedThings: true, "previousIdeos", LookMode.Reference);
                Scribe_Values.Look(ref favoriteColor, "favoriteColor");
                Scribe_Values.Look(ref joinTick, "joinTick");
                Scribe_Values.Look(ref certainty, "certainty");
                Scribe_References.Look(ref precept_RoleSingle, "precept_RoleSingle");
                Scribe_References.Look(ref precept_RoleMulti, "precept_RoleMulti");
            }
            Scribe_Values.Look<int>(ref this.sexuality, "sexuality", -1);
            Scribe_Values.Look<float>(ref this.romanceFactor, "romanceFactor", -1f);
            Scribe_Deep.Look<PsychologyData>(ref this.psychologyData, "psychologyData");
        }

        private List<Faction> favorKeys = new List<Faction>();
        private List<int> favorValues = new List<int>();

        private List<Faction> heirsKeys = new List<Faction>();
        private List<Pawn> heirsValues = new List<Pawn>();

        private List<Faction> tmpPermitFactions;
        private List<int> tmpPermitPointsAmounts;
    }
}

