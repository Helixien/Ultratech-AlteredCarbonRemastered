using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class AlteredCarbonManager : GameComponent
    {
        public static AlteredCarbonManager Instance;
        public AlteredCarbonManager()
        {
            Instance = this;
            ResetStaticData();
        }

        public AlteredCarbonManager(Game game)
        {
            Instance = this;
            ResetStaticData();
        }

        public void ResetStaticData()
        {
            Building_StackStorage.building_StackStorages?.Clear();
            CorticalStack.corticalStacks?.Clear();
        }

        public void ResetStackLimitIfNeeded(ThingDef def)
        {
            if (def.stackLimit != 1)
            {
                def.stackLimit = 1;
                def.drawGUIOverlay = false;
            }
        }

        public void PreInit()
        {
            if (this.stacksIndex == null) this.stacksIndex = new Dictionary<int, CorticalStack>();
            if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>();
            if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>();
            if (this.deadPawns == null) this.deadPawns = new HashSet<Pawn>();
            ResetStackLimitIfNeeded(AC_DefOf.UT_FilledCorticalStack);
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PreInit();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            PreInit();
        }
        public void TryAddRelationships(Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
            if (hediff != null && this.stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out var stackData))
            {
                if (stackData.originalPawn != null)
                {
                    if (pawn != stackData.originalPawn)
                    {
                        pawn.relations.AddDirectRelation(AC_DefOf.UT_Original, stackData.originalPawn);
                        stackData.originalPawn.relations.AddDirectRelation(AC_DefOf.UT_Copy, pawn);
                    }
                    else if (stackData.copiedPawns != null)
                    {
                        foreach (var copiedPawn in stackData.copiedPawns)
                        {
                            if (pawn != copiedPawn)
                            {
                                pawn.relations.AddDirectRelation(AC_DefOf.UT_Original, copiedPawn);
                                copiedPawn.relations.AddDirectRelation(AC_DefOf.UT_Copy, pawn);
                            }
                        }
                    }
                }
                else if (stackData.copiedPawns != null)
                {
                    foreach (var copiedPawn in stackData.copiedPawns)
                    {
                        if (pawn != copiedPawn)
                        {
                            pawn.relations.AddDirectRelation(AC_DefOf.UT_Copy, copiedPawn);
                            copiedPawn.relations.AddDirectRelation(AC_DefOf.UT_Copy, pawn);
                        }
                    }
                }
            }
            else
            {
                foreach (var stackGroup in this.stacksRelationships)
                {
                    if (stackGroup.Value.copiedPawns != null)
                    {
                        if (pawn == stackGroup.Value.originalPawn && stackGroup.Value.copiedPawns != null)
                        {
                            foreach (var copiedPawn in stackGroup.Value.copiedPawns)
                            {
                                if (pawn != copiedPawn)
                                {
                                    pawn.relations.AddDirectRelation(AC_DefOf.UT_Original, copiedPawn);
                                    copiedPawn.relations.AddDirectRelation(AC_DefOf.UT_Copy, pawn);
                                }
                            }
                        }
                        else if (stackGroup.Value.copiedPawns != null)
                        {
                            foreach (var copiedPawn in stackGroup.Value.copiedPawns)
                            {
                                if (pawn == copiedPawn && stackGroup.Value.originalPawn != null)
                                {
                                    pawn.relations.AddDirectRelation(AC_DefOf.UT_Original, stackGroup.Value.originalPawn);
                                    stackGroup.Value.originalPawn.relations.AddDirectRelation(AC_DefOf.UT_Copy, pawn);
                                }
                            }
                        }
                    }
                }
            }

            if (pawn.IsCopy())
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_JustCopy);
                var otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherPawn))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_LostMySpouse, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, otherPawn))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_LostMyFiance, otherPawn);
                }

                otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
                if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherPawn))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.UT_LostMyLover, otherPawn);
                }
            }
        }

        public void ReplacePawnWithStack(Pawn pawn, CorticalStack stack)
        {
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
            if (hediff != null)
            {
                stack.PersonaData.stackGroupID = hediff.PersonaData.stackGroupID;
                if (this.stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out var stackData))
                {
                    if (stackData.originalPawn == pawn)
                    {
                        stackData.originalPawn = null;
                        stackData.originalStack = stack;
                    }
                    else if (stackData.copiedPawns.Contains(pawn))
                    {
                        stackData.copiedPawns.Remove(pawn);
                        if (stackData.copiedStacks == null) 
                            stackData.copiedStacks = new List<CorticalStack>();
                        stackData.copiedStacks.Add(stack);
                    }
                }
            }
        }

        public void ReplaceStackWithPawn(CorticalStack stack, Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
            if (hediff != null)
            {
                hediff.PersonaData.stackGroupID = stack.PersonaData.stackGroupID;
                if (this.stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out var stackData))
                {
                    if (stackData.originalStack == stack)
                    {
                        stackData.originalStack = null;
                        stackData.originalPawn = pawn;
                    }
                    else if (stackData.copiedStacks?.Contains(stack) ?? false)
                    {
                        stackData.copiedStacks.Remove(stack);
                        if (stackData.copiedPawns == null) stackData.copiedPawns = new List<Pawn>();
                        stackData.copiedPawns.Add(pawn);
                    }
                }
                this.pawnsWithStacks.Add(pawn);
            }
        }

        public void RegisterStack(CorticalStack stack)
        {
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            if (!this.stacksRelationships.ContainsKey(stack.PersonaData.stackGroupID))
            {
                this.stacksRelationships[stack.PersonaData.stackGroupID] = new StacksData();
            }
            if (stack.PersonaData.isCopied)
            {
                if (this.stacksRelationships[stack.PersonaData.stackGroupID].copiedStacks == null) 
                    this.stacksRelationships[stack.PersonaData.stackGroupID].copiedStacks = new List<CorticalStack>();
                this.stacksRelationships[stack.PersonaData.stackGroupID].copiedStacks.Add(stack);
            }
            else
            {
                this.stacksRelationships[stack.PersonaData.stackGroupID].originalStack = stack;
            }
        }

        public void RegisterPawn(Pawn pawn)
        {
            if (this.stacksRelationships == null) this.stacksRelationships = new Dictionary<int, StacksData>();
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
            if (hediff != null)
            {
                if (!this.stacksRelationships.ContainsKey(hediff.PersonaData.stackGroupID))
                {
                    this.stacksRelationships[hediff.PersonaData.stackGroupID] = new StacksData();
                }
                if (hediff.PersonaData.isCopied)
                {
                    if (this.stacksRelationships[hediff.PersonaData.stackGroupID].copiedPawns == null) this.stacksRelationships[hediff.PersonaData.stackGroupID].copiedPawns = new List<Pawn>();

                    this.stacksRelationships[hediff.PersonaData.stackGroupID].copiedPawns.Add(pawn);
                }
                else
                {
                    this.stacksRelationships[hediff.PersonaData.stackGroupID].originalPawn = pawn;
                }
                if (this.emptySleeves != null)
                {
                    this.emptySleeves.Remove(pawn);
                }
                this.pawnsWithStacks.Add(pawn);
            }
        }

        public void RegisterSleeve(Pawn pawn, int stackGroupID = -1)
        {
            if (this.pawnsWithStacks == null) this.pawnsWithStacks = new HashSet<Pawn>();
            if (this.emptySleeves == null) this.emptySleeves = new HashSet<Pawn>();
            this.pawnsWithStacks.Remove(pawn);
            this.emptySleeves.Add(pawn);
            if (stackGroupID != -1 && this.stacksRelationships.ContainsKey(stackGroupID))
            {
                if (this.stacksRelationships[stackGroupID].deadPawns == null)
                    this.stacksRelationships[stackGroupID].deadPawns = new List<Pawn>();
                this.stacksRelationships[stackGroupID].deadPawns.Add(pawn);
            }
        }

        public int GetStackGroupID(CorticalStack corticalStack)
        {
            if (corticalStack.PersonaData.stackGroupID != 0) return corticalStack.PersonaData.stackGroupID;

            if (this.stacksRelationships != null)
            {
                return this.stacksRelationships.Count + 1;
            }
            else
            {
                return 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            this.pawnsWithStacks.RemoveWhere(x => x is null || x.Destroyed);
            Scribe_Collections.Look<int, CorticalStack>(ref this.stacksIndex, "stacksIndex", LookMode.Value, LookMode.Reference, ref this.pawnKeys, ref this.stacksValues);
            Scribe_Collections.Look<Pawn>(ref this.pawnsWithStacks, "pawnsWithStacks", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref this.emptySleeves, "emptySleeves", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref this.deadPawns, saveDestroyedThings: true, "deadPawns", LookMode.Reference);
            Scribe_Collections.Look<int, StacksData>(ref this.stacksRelationships, "stacksRelationships", LookMode.Value, LookMode.Deep, ref stacksRelationshipsKeys, ref stacksRelationshipsValues);
            this.pawnsWithStacks.RemoveWhere(x => x is null || x.Destroyed);
            Instance = this;
        }

        public Dictionary<int, StacksData> stacksRelationships = new Dictionary<int, StacksData>();
        private List<int> stacksRelationshipsKeys = new List<int>();
        private List<StacksData> stacksRelationshipsValues = new List<StacksData>();

        private HashSet<Pawn> pawnsWithStacks = new HashSet<Pawn>();
        public HashSet<Pawn> PawnsWithStacks
        {
            get
            {
                if (pawnsWithStacks is null)
                {
                    pawnsWithStacks = new HashSet<Pawn>();
                }
                return pawnsWithStacks.Where(x => x != null).ToHashSet();
            }
        }

        public HashSet<Pawn> emptySleeves = new HashSet<Pawn>();
        public HashSet<Pawn> deadPawns = new HashSet<Pawn>();

        private Dictionary<int, CorticalStack> stacksIndex;
        public Dictionary<int, CorticalStack> StacksIndex
        {
            get
            {
                if (stacksIndex is null)
                {
                    stacksIndex = new Dictionary<int, CorticalStack>();
                }
                return stacksIndex;
            }
        }
        private List<int> pawnKeys = new List<int>();
        private List<CorticalStack> stacksValues = new List<CorticalStack>();
    }
}