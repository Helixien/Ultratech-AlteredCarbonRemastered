using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CorticalStack : Hediff_Implant
    {
        public Name name;
        public Pawn origPawn;
        public int hostilityMode;
        public Area areaRestriction;
        public MedicalCareCategory medicalCareCategory;
        public bool selfTend;
        public long ageChronologicalTicks;
        public List<TimeAssignmentDef> times;
        public FoodRestriction foodRestriction;
        public Outfit outfit;
        public DrugPolicy drugPolicy;
        public Faction faction;
        public bool isFactionLeader;
        public List<Thought_Memory> thoughts;
        public List<Trait> traits;
        public List<DirectPawnRelation> relations;
        public HashSet<Pawn> relatedPawns;
        public List<SkillRecord> skills;
        public string childhood;
        public string adulthood;
        public string pawnID;
        public Dictionary<WorkTypeDef, int> priorities;

        public bool hasPawn = false;

        public Gender gender;
        public ThingDef race;

        public List<RoyalTitle> royalTitles;
        public Dictionary<Faction, int> favor = new Dictionary<Faction, int>();
        public Dictionary<Faction, Pawn> heirs = new Dictionary<Faction, Pawn>();
        public List<Thing> bondedThings = new List<Thing>();
        public List<FactionPermit> factionPermits = new List<FactionPermit>();
        public Dictionary<Faction, int> permitPoints = new Dictionary<Faction, int>();
        public bool isCopied = false;
        public int stackGroupID;

        public int sexuality;
        public float romanceFactor;
        public PsychologyData psychologyData;

        public List<SkillOffsets> negativeSkillsOffsets;
        public List<SkillOffsets> negativeSkillPassionsOffsets;

        public override void PostAdd(DamageInfo? dinfo)
        {
            this.Part = pawn.def.race.body.AllParts.FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
            base.PostAdd(dinfo);
            var emptySleeveHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_EmptySleeve);
            if (emptySleeveHediff != null)
            {
                pawn.health.RemoveHediff(emptySleeveHediff);
            }
        }

        public override bool ShouldRemove => false;
        public void SavePawn(Pawn pawn)
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
            this.relations = pawn.relations?.DirectRelations;
            this.relatedPawns = pawn.relations?.RelatedPawns?.ToHashSet();
            foreach (var otherPawn in pawn.relations?.RelatedPawns)
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
            this.priorities = new Dictionary<WorkTypeDef, int>();
            if (pawn.workSettings != null && Traverse.Create(pawn.workSettings).Field("priorities").GetValue<DefMap<WorkTypeDef, int>>() != null)
            {
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    this.priorities[w] = pawn.workSettings.GetPriority(w);
                }
            }
            this.hasPawn = true;
            this.pawnID = pawn.ThingID;
            if (ModLister.RoyaltyInstalled && pawn.royalty != null)
            {
                this.royalTitles = pawn.royalty?.AllTitlesForReading;
                this.favor = Traverse.Create(pawn.royalty).Field("favor").GetValue<Dictionary<Faction, int>>();
                this.heirs = Traverse.Create(pawn.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>();
                foreach (var map in Find.Maps)
                {
                    foreach (var thing in map.listerThings.AllThings)
                    {
                        var comp = thing.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(thing);
                        }
                    }
                    foreach (var gear in pawn.apparel?.WornApparel)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.equipment?.AllEquipmentListForReading)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.inventory?.innerContainer)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                }
                this.factionPermits = Traverse.Create(pawn.royalty).Field("factionPermits").GetValue<List<FactionPermit>>();
                this.permitPoints = Traverse.Create(pawn.royalty).Field("permitPoints").GetValue<Dictionary<Faction, int>>();
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

        public override void Notify_PawnDied()
        {
            if (!this.hasPawn)
            {
                SavePawn(this.pawn);
            }
            base.Notify_PawnDied();
        }

        public override void Notify_PawnKilled()
        {
            if (!this.hasPawn)
            {
                SavePawn(this.pawn);
            }
            base.Notify_PawnKilled();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!this.pawn.Dead)
            {
                Notify_ColonistKilled_Patch.DisableKilledEffect = true;
                this.pawn.Kill(null);
                Notify_ColonistKilled_Patch.DisableKilledEffect = false;
            }
            //Log.Message("2 Killing the pawn: " + this.stackGroupID, true);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.stackGroupID, "stackGroupID", 0);
            //if (Scribe.mode == LoadSaveMode.PostLoadInit)
            //{
            //    Log.Message(this + " - " + this.stackGroupID, true);
            //}
            Scribe_Values.Look<bool>(ref this.isCopied, "isCopied", false, false);
            Scribe_Deep.Look<Name>(ref this.name, "name", new object[0]);
            Scribe_References.Look<Pawn>(ref this.origPawn, "origPawn", true);

            Scribe_Values.Look<int>(ref this.hostilityMode, "hostilityMode");
            Scribe_References.Look<Area>(ref this.areaRestriction, "areaRestriction", false);
            Scribe_Values.Look<MedicalCareCategory>(ref this.medicalCareCategory, "medicalCareCategory", 0, false);
            Scribe_Values.Look<bool>(ref this.selfTend, "selfTend", false, false);
            Scribe_Values.Look<long>(ref this.ageChronologicalTicks, "ageChronologicalTicks", 0, false);

            Scribe_References.Look<Outfit>(ref this.outfit, "outfit", false);
            Scribe_References.Look<FoodRestriction>(ref this.foodRestriction, "foodPolicy", false);
            Scribe_References.Look<DrugPolicy>(ref this.drugPolicy, "drugPolicy", false);

            Scribe_Collections.Look<TimeAssignmentDef>(ref this.times, "times");
            Scribe_Collections.Look<Thought_Memory>(ref this.thoughts, "thoughts");
            Scribe_References.Look<Faction>(ref this.faction, "faction", true);
            Scribe_Values.Look<bool>(ref this.isFactionLeader, "isFactionLeader", false, false);

            Scribe_Values.Look<string>(ref this.childhood, "childhood", null, false);
            Scribe_Values.Look<string>(ref this.adulthood, "adulthood", null, false);
            Scribe_Values.Look<string>(ref this.pawnID, "pawnID", null, false);
            Scribe_Collections.Look<Trait>(ref this.traits, "traits");
            Scribe_Collections.Look<SkillRecord>(ref this.skills, "skills");
            Scribe_Collections.Look<DirectPawnRelation>(ref this.relations, "relations");
            Scribe_Collections.Look<Pawn>(ref this.relatedPawns, true, "relatedPawns", LookMode.Reference);

            Scribe_Collections.Look<WorkTypeDef, int>(ref this.priorities, "priorities");
            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);
            Scribe_Values.Look<Gender>(ref this.gender, "gender", 0, false);
            Scribe_Defs.Look<ThingDef>(ref this.race, "race");
            Scribe_Collections.Look<SkillOffsets>(ref negativeSkillsOffsets, "negativeSkillsOffsets", LookMode.Deep);
            Scribe_Collections.Look<SkillOffsets>(ref negativeSkillPassionsOffsets, "negativeSkillPassionsOffsets", LookMode.Deep);

            if (ModLister.RoyaltyInstalled)
            {
                Scribe_Collections.Look<Faction, int>(ref this.favor, "favor",
                    LookMode.Reference, LookMode.Value,
                    ref this.favorKeys, ref this.favorValues);
                Scribe_Collections.Look<Faction, Pawn>(ref this.heirs, "heirs",
                    LookMode.Reference, LookMode.Reference,
                    ref this.heirsKeys, ref this.heirsValues);

                Scribe_Collections.Look<Thing>(ref this.bondedThings, "bondedThings", LookMode.Reference);
                Scribe_Collections.Look<RoyalTitle>(ref this.royalTitles, "royalTitles", LookMode.Deep);
                Scribe_Collections.Look(ref permitPoints, "permitPoints", LookMode.Reference, LookMode.Value, ref tmpPermitFactions, ref tmpPermitPointsAmounts);
                Scribe_Collections.Look(ref factionPermits, "permits", LookMode.Deep);
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

