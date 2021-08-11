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

        private DefMap<RecordDef, float> records = new DefMap<RecordDef, float>();
        private Battle battleActive;
        private int battleExitTick;

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
        public Ideo ideo;
        public Color? favoriteColor;
        public int joinTick;
        public List<Ideo> previousIdeos;
        public float certainty;
        public Precept_RoleMulti precept_RoleMulti;
        public Precept_RoleSingle precept_RoleSingle;

        // [SYR] Individuality
        private int sexuality;
        private float romanceFactor;

        // Psychology
        private PsychologyData psychologyData;
        // RJW
        private RJWData rjwData;
        // misc
        public bool? diedFromCombat;
        public bool hackedWhileOnStack;
        public bool isCopied = false;
        public int stackGroupID = -1;

        public int lastTimeUpdated;

        public List<SkillOffsets> negativeSkillsOffsets;
        public List<SkillOffsets> negativeSkillPassionsOffsets;

        private Pawn dummyPawn;

        public Pawn GetDummyPawn
        {
            get
            {
                if (dummyPawn is null)
                {
                    if (this.origPawn?.story != null)
                    {
                        dummyPawn = this.origPawn;
                    }
                    else
                    {
                        dummyPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                        OverwritePawn(dummyPawn, null);
                    }
                }
                return dummyPawn;
            }
        }
        public TaggedString PawnNameColored
        {
            get
            {
                if (TitleShort?.CapitalizeFirst().NullOrEmpty() ?? false)
                {
                    return this.name?.ToStringShort.Colorize(GetFactionRelationColor(this.faction));
                }
                return this.name?.ToStringShort.Colorize(GetFactionRelationColor(this.faction)) + ", " + TitleShort?.CapitalizeFirst();
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
                if (!this.adulthood.NullOrEmpty() && BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out var newAdulthood, true))
                {
                    return newAdulthood.TitleShortFor(gender);
                }
                if (!this.childhood.NullOrEmpty() && BackstoryDatabase.TryGetWithIdentifier(this.childhood, out var newChildhood, true))
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
            if (pawn.Faction?.leader == pawn)
            {
                this.isFactionLeader = true;
            }
            this.traits = pawn.story?.traits?.allTraits;

            if (pawn.relations != null)
            {
                this.relations = pawn.relations.DirectRelations ?? new List<DirectPawnRelation>();
                this.relatedPawns = pawn.relations.RelatedPawns?.ToHashSet() ?? new HashSet<Pawn>();
                foreach (var otherPawn in pawn.relations.RelatedPawns)
                {
                    foreach (var rel2 in pawn.GetRelations(otherPawn))
                    {
                        if (!this.relations.Any(r => r.def == rel2 && r.otherPawn == otherPawn))
                        {
                            if (!rel2.implied)
                            {
                                this.relations.Add(new DirectPawnRelation(rel2, otherPawn, 0));
                            }
                        }
                    }
                    relatedPawns.Add(otherPawn);
                }
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
            if (pawn.guest != null)
            {
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
            }
            if (pawn.records != null)
            {
                this.records = Traverse.Create(pawn.records).Field("records").GetValue<DefMap<RecordDef, float>>();
                this.battleActive = pawn.records.BattleActive;
                this.battleExitTick = pawn.records.LastBattleTick;
            }
            this.hasPawn = true;
            this.pawnID = pawn.thingIDNumber;
            if (ModsConfig.RoyaltyActive && pawn.royalty != null)
            {
                this.royalTitles = pawn.royalty?.AllTitlesForReading;
                this.favor = Traverse.Create(pawn.royalty).Field("favor").GetValue<Dictionary<Faction, int>>();
                this.heirs = Traverse.Create(pawn.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>();
                this.bondedThings = new List<Thing>();
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
                if (pawn.ideo != null && pawn.Ideo != null)
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

                if (pawn.story?.favoriteColor.HasValue ?? false)
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
            if (ModCompatibility.RimJobWorldIsActive)
            {
                this.rjwData = ModCompatibility.GetRjwData(pawn);
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
                        if (finalValue <= 2)
                        {
                            switch (finalValue)
                            {
                                case 0:
                                    skill.passion = Passion.None;
                                    break;
                                case 1:
                                    skill.passion = Passion.Minor;
                                    break;
                                case 2:
                                    skill.passion = Passion.Major;
                                    break;
                                default:
                                    skill.passion = Passion.None;
                                    break;
                            }
                        }
                        else
                        {
                            skill.passion = Passion.None;
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

            this.records = other.records;
            this.battleActive = other.battleActive;
            this.battleExitTick = other.battleExitTick;

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
            this.diedFromCombat = other.diedFromCombat;
            this.hackedWhileOnStack = other.hackedWhileOnStack;
            this.stackGroupID = other.stackGroupID;

            this.sexuality = other.sexuality;
            this.romanceFactor = other.romanceFactor;
            this.psychologyData = other.psychologyData;
            this.rjwData = other.rjwData;
        }

        public void OverwritePawn(Pawn pawnToOverwrite, StackSavingOptionsModExtension extension)
        {
            PawnComponentsUtility.CreateInitialComponents(pawnToOverwrite);
            if (pawnToOverwrite.Faction != this.faction)
            {
                pawnToOverwrite.SetFaction(this.faction);
            }
            if (this.isFactionLeader)
            {
                pawnToOverwrite.Faction.leader = pawnToOverwrite;
            }

            pawnToOverwrite.Name = this.name;
            if (pawnToOverwrite.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = pawnToOverwrite.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    pawnToOverwrite.needs.mood.thoughts.memories.RemoveMemory(pawnToOverwrite.needs.mood.thoughts.memories.Memories[num]);
                }
            }

            if (this.thoughts != null)
            {
                if (this.gender == pawnToOverwrite.gender)
                {
                    this.thoughts.RemoveAll(x => x.def == AC_DefOf.UT_WrongGender);
                    this.thoughts.RemoveAll(x => x.def == AC_DefOf.UT_WrongGenderDouble);
                }
                if (this.race == pawnToOverwrite.kindDef.race)
                {
                    this.thoughts.RemoveAll(x => x.def == AC_DefOf.UT_WrongRace);
                }

                foreach (var thought in this.thoughts)
                {
                    if (thought is Thought_MemorySocial && thought.otherPawn == null)
                    {
                        continue;
                    }
                    try
                    {
                        pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                    }
                    catch { }
                }
            }
            if (extension != null)
            {
                pawnToOverwrite.story.traits.allTraits.RemoveAll(x => !extension.ignoresTraits.Contains(x.def.defName));
            }
            else
            {
                pawnToOverwrite.story.traits.allTraits.Clear();
            }
            if (this.traits != null)
            {
                foreach (var trait in this.traits)
                {
                    if (extension != null && extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                    {
                        continue;
                    }
                    pawnToOverwrite.story.traits.GainTrait(trait);
                }
            }

            pawnToOverwrite.relations.ClearAllRelations();
            var origPawn = GetOriginalPawn(pawnToOverwrite);

            foreach (var pawn2 in this.relatedPawns)
            {
                var otherPawn = GetTruePawn(pawn2);
                if (otherPawn != null)
                {
                    foreach (var rel in otherPawn.relations.DirectRelations)
                    {
                        if (this.name.ToStringFull == rel.otherPawn?.Name.ToStringFull)
                        {
                            rel.otherPawn = pawnToOverwrite;
                        }
                    }

                    foreach (var rel in this.relations)
                    {
                        foreach (var rel2 in otherPawn.relations.DirectRelations)
                        {
                            if (rel.def == rel2.def && rel2.otherPawn?.Name.ToStringFull == pawnToOverwrite.Name.ToStringFull)
                            {
                                rel2.otherPawn = pawnToOverwrite;
                            }
                        }
                    }
                }
            }

            foreach (var rel in this.relations)
            {
                var otherPawn = GetTruePawn(rel.otherPawn);
                if (otherPawn != null && rel != null)
                {
                    var oldRelation = otherPawn.relations.DirectRelations.Where(r => r.def == rel.def && r.otherPawn.Name.ToStringFull == pawnToOverwrite.Name.ToStringFull).FirstOrDefault();
                    if (oldRelation != null)
                    {
                        oldRelation.otherPawn = pawnToOverwrite;
                    }

                    try
                    {
                        pawnToOverwrite.relations.AddDirectRelation(rel.def, otherPawn);
                    }
                    catch { }
                }
            }

            if (origPawn != null)
            {
                origPawn.relations = new Pawn_RelationsTracker(origPawn);
            }

            pawnToOverwrite.skills.skills.Clear();
            if (this.skills != null)
            {
                foreach (var skill in this.skills)
                {
                    var newSkill = new SkillRecord(pawnToOverwrite, skill.def);
                    newSkill.passion = skill.passion;
                    newSkill.levelInt = skill.levelInt;
                    newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                    newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                    pawnToOverwrite.skills.skills.Add(newSkill);
                }
            }
            if (pawnToOverwrite.playerSettings == null) pawnToOverwrite.playerSettings = new Pawn_PlayerSettings(pawnToOverwrite);
            pawnToOverwrite.playerSettings.hostilityResponse = (HostilityResponseMode)this.hostilityMode;

            if (!this.childhood.NullOrEmpty())
            {
                BackstoryDatabase.TryGetWithIdentifier(this.childhood, out var newChildhood, true);
                pawnToOverwrite.story.childhood = newChildhood;
            }
            else
            {
                pawnToOverwrite.story.childhood = null;
            }

            if (!this.adulthood.NullOrEmpty())
            {
                BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out var newAdulthood, true);
                pawnToOverwrite.story.adulthood = newAdulthood;
            }
            else
            {
                pawnToOverwrite.story.adulthood = null;
            }

            pawnToOverwrite.story.title = this.title;

            if (pawnToOverwrite.workSettings == null)
            {
                pawnToOverwrite.workSettings = new Pawn_WorkSettings(pawnToOverwrite);
            }

            var pawnField = Traverse.Create(pawnToOverwrite.workSettings).Field("pawn");
            if (pawnField.GetValue() is null)
            {
                pawnField.SetValue(pawnToOverwrite);
            }

            var prioritiesField = Traverse.Create(pawnToOverwrite.workSettings).Field("priorities");
            if (prioritiesField.GetValue() is null)
            {
                prioritiesField.SetValue(new DefMap<WorkTypeDef, int>());
            }

            pawnToOverwrite.Notify_DisabledWorkTypesChanged();
            if (priorities != null)
            {

                foreach (var priority in priorities)
                {
                    pawnToOverwrite.workSettings.SetPriority(priority.Key, priority.Value);
                }
            }
            if (pawnToOverwrite.guest is null)
            {
                pawnToOverwrite.guest = new Pawn_GuestTracker();
            }
            pawnToOverwrite.guest.guestStatusInt = this.guestStatusInt;
            pawnToOverwrite.guest.interactionMode = this.interactionMode;
            pawnToOverwrite.guest.slaveInteractionMode = this.slaveInteractionMode;
            Traverse.Create(pawnToOverwrite.guest).Field("hostFactionInt").SetValue(this.hostFactionInt);
            pawnToOverwrite.guest.joinStatus = this.joinStatus;
            Traverse.Create(pawnToOverwrite.guest).Field("slaveFactionInt").SetValue(this.slaveFactionInt);
            pawnToOverwrite.guest.lastRecruiterName = this.lastRecruiterName;
            pawnToOverwrite.guest.lastRecruiterOpinion = this.lastRecruiterOpinion;
            pawnToOverwrite.guest.hasOpinionOfLastRecruiter = this.hasOpinionOfLastRecruiter;
            pawnToOverwrite.guest.lastRecruiterResistanceReduce = this.lastRecruiterResistanceReduce;
            pawnToOverwrite.guest.Released = this.releasedInt;
            Traverse.Create(pawnToOverwrite.guest).Field("ticksWhenAllowedToEscapeAgain").SetValue(this.ticksWhenAllowedToEscapeAgain);
            pawnToOverwrite.guest.spotToWaitInsteadOfEscaping = this.spotToWaitInsteadOfEscaping;
            pawnToOverwrite.guest.lastPrisonBreakTicks = this.lastPrisonBreakTicks;
            pawnToOverwrite.guest.everParticipatedInPrisonBreak = this.everParticipatedInPrisonBreak;
            pawnToOverwrite.guest.resistance = this.resistance;
            pawnToOverwrite.guest.will = this.will;
            pawnToOverwrite.guest.ideoForConversion = this.ideoForConversion;
            Traverse.Create(pawnToOverwrite.guest).Field("everEnslaved").SetValue(this.everEnslaved);
            pawnToOverwrite.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = this.getRescuedThoughtOnUndownedBecauseOfPlayer;

            if (pawnToOverwrite.records is null)
            {
                pawnToOverwrite.records = new Pawn_RecordsTracker(pawnToOverwrite);
            }

            Traverse.Create(pawnToOverwrite.records).Field("records").SetValue(this.records);
            Traverse.Create(pawnToOverwrite.records).Field("battleActive").SetValue(this.battleActive);
            Traverse.Create(pawnToOverwrite.records).Field("battleExitTick").SetValue(this.battleExitTick);

            if (pawnToOverwrite.playerSettings is null)
            {
                pawnToOverwrite.playerSettings = new Pawn_PlayerSettings(pawnToOverwrite);
            }
            pawnToOverwrite.playerSettings.AreaRestriction = this.areaRestriction;
            pawnToOverwrite.playerSettings.medCare = this.medicalCareCategory;
            pawnToOverwrite.playerSettings.selfTend = this.selfTend;
            if (pawnToOverwrite.foodRestriction == null) pawnToOverwrite.foodRestriction = new Pawn_FoodRestrictionTracker();
            pawnToOverwrite.foodRestriction.CurrentFoodRestriction = this.foodRestriction;
            if (pawnToOverwrite.outfits == null) pawnToOverwrite.outfits = new Pawn_OutfitTracker();
            pawnToOverwrite.outfits.CurrentOutfit = this.outfit;
            if (pawnToOverwrite.drugs == null) pawnToOverwrite.drugs = new Pawn_DrugPolicyTracker();
            pawnToOverwrite.drugs.CurrentPolicy = this.drugPolicy;
            pawnToOverwrite.ageTracker.AgeChronologicalTicks = this.ageChronologicalTicks;
            if (pawnToOverwrite.timetable == null) pawnToOverwrite.timetable = new Pawn_TimetableTracker(pawnToOverwrite);
            if (this.times != null) pawnToOverwrite.timetable.times = this.times;
            if (pawnToOverwrite.gender != this.gender)
            {
                if (pawnToOverwrite.story.traits.HasTrait(TraitDefOf.BodyPurist))
                {
                    try
                    {
                        pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_WrongGenderDouble);
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
                        pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_WrongGender);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }


            if (pawnToOverwrite.kindDef.race != this.race)
            {
                try
                {
                    pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_WrongRace);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }

            if (ModsConfig.RoyaltyActive)
            {
                if (pawnToOverwrite.royalty == null) pawnToOverwrite.royalty = new Pawn_RoyaltyTracker(pawnToOverwrite);
                if (this.royalTitles != null)
                {
                    foreach (var title in this.royalTitles)
                    {
                        pawnToOverwrite.royalty.SetTitle(title.faction, title.def, false, false, false);
                    }
                }
                if (this.heirs != null)
                {
                    foreach (var heir in this.heirs)
                    {
                        pawnToOverwrite.royalty.SetHeir(heir.Value, heir.Key);
                    }
                }

                if (this.favor != null)
                {
                    foreach (var fav in this.favor)
                    {
                        pawnToOverwrite.royalty.SetFavor(fav.Key, fav.Value);
                    }
                }

                if (this.bondedThings != null)
                {
                    foreach (var bonded in this.bondedThings)
                    {
                        var comp = bonded.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            comp.CodeFor(pawnToOverwrite);
                        }
                    }
                }
                if (this.factionPermits != null)
                {
                    Traverse.Create(pawnToOverwrite.royalty).Field("factionPermits").SetValue(this.factionPermits);
                }
                if (this.permitPoints != null)
                {
                    Traverse.Create(pawnToOverwrite.royalty).Field("permitPoints").SetValue(this.permitPoints);
                }
            }

            if (ModsConfig.IdeologyActive)
            {
                if (this.precept_RoleMulti != null)
                {
                    if (this.precept_RoleMulti.chosenPawns is null)
                    {
                        this.precept_RoleMulti.chosenPawns = new List<IdeoRoleInstance>();
                    }

                    this.precept_RoleMulti.chosenPawns.Add(new IdeoRoleInstance(this.precept_RoleMulti)
                    {
                        pawn = pawnToOverwrite
                    });
                    this.precept_RoleMulti.FillOrUpdateAbilities();
                }
                if (this.precept_RoleSingle != null)
                {
                    this.precept_RoleSingle.chosenPawn = new IdeoRoleInstance(this.precept_RoleMulti)
                    {
                        pawn = pawnToOverwrite
                    };
                    this.precept_RoleSingle.FillOrUpdateAbilities();
                }

                if (this.ideo != null)
                {
                    pawnToOverwrite.ideo.SetIdeo(this.ideo);
                    Traverse.Create(pawnToOverwrite.ideo).Field("certainty").SetValue(this.certainty);
                    Traverse.Create(pawnToOverwrite.ideo).Field("previousIdeos").SetValue(this.previousIdeos);
                    pawnToOverwrite.ideo.joinTick = this.joinTick;
                }
                if (this.favoriteColor.HasValue)
                {
                    pawnToOverwrite.story.favoriteColor = this.favoriteColor.Value;
                }

            }

            if (ModCompatibility.IndividualityIsActive)
            {
                ModCompatibility.SetSyrTraitsSexuality(pawnToOverwrite, this.sexuality);
                ModCompatibility.SetSyrTraitsRomanceFactor(pawnToOverwrite, this.romanceFactor);
            }

            if (ModCompatibility.PsychologyIsActive && this.psychologyData != null)
            {
                ModCompatibility.SetPsychologyData(pawnToOverwrite, this.psychologyData);
            }
            if (ModCompatibility.RimJobWorldIsActive && this.rjwData != null)
            {
                ModCompatibility.SetRjwData(pawnToOverwrite, this.rjwData);
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
            if (pawn != null && pawn.Dead && pawn.Name != null && AlteredCarbonManager.Instance.PawnsWithStacks != null)
            {
                foreach (var otherPawn in AlteredCarbonManager.Instance.PawnsWithStacks)
                {
                    if (otherPawn != null && otherPawn.Name != null && otherPawn.Name.ToStringFull == pawn.Name.ToStringFull)
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
            Scribe_Values.Look(ref this.diedFromCombat, "diedFromCombat");
            Scribe_Values.Look(ref this.hackedWhileOnStack, "hackedWhileOnStack");
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

            Scribe_Deep.Look(ref records, "records");
            Scribe_References.Look(ref battleActive, "battleActive");
            Scribe_Values.Look(ref battleExitTick, "battleExitTick", 0);

            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);
            Scribe_Values.Look<Gender>(ref this.gender, "gender");
            Scribe_Values.Look(ref lastTimeUpdated, "lastTimeUpdated");
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
            Scribe_Deep.Look(ref this.psychologyData, "psychologyData");
            Scribe_Deep.Look(ref rjwData, "rjwData");
        }

        private List<Faction> favorKeys = new List<Faction>();
        private List<int> favorValues = new List<int>();

        private List<Faction> heirsKeys = new List<Faction>();
        private List<Pawn> heirsValues = new List<Pawn>();

        private List<Faction> tmpPermitFactions;
        private List<int> tmpPermitPointsAmounts;
    }
}

