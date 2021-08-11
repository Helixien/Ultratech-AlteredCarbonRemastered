using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(MapDeiniter))]
    [HarmonyPatch("PassPawnsToWorld")]
    internal static class PassPawnsToWorld_Patch
    {
        private static void Prefix(Map map)
        {
            for (int num = map.mapPawns.AllPawns.Count - 1; num >= 0; num--)
            {
                Log.Message("PassPawnsToWorld_Patch - Prefix - Pawn pawn = map.mapPawns.AllPawns[num]; - 1", true);
                Pawn pawn = map.mapPawns.AllPawns[num];
                Log.Message("PassPawnsToWorld_Patch - Prefix - if ((pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer) && pawn.HasStack()) - 2", true);
                if ((pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer) && pawn.HasStack())
                {
                    Log.Message("PassPawnsToWorld_Patch - Prefix - pawn.DeSpawn(DestroyMode.Vanish); - 3", true);
                    pawn.DeSpawn(DestroyMode.Vanish);
                    Log.Message("PassPawnsToWorld_Patch - Prefix - TaggedString label = \"Death\".Translate() + \": \" + pawn.LabelShortCap; - 4", true);
                    TaggedString label = "Death".Translate() + ": " + pawn.LabelShortCap;
                    Log.Message("PassPawnsToWorld_Patch - Prefix - TaggedString taggedString = \"PawnDied\".Translate(pawn.LabelShortCap, pawn.Named(\"PAWN\")); - 5", true);
                    TaggedString taggedString = "PawnDied".Translate(pawn.LabelShortCap, pawn.Named("PAWN"));
                    Log.Message("PassPawnsToWorld_Patch - Prefix - Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.Death, pawn, null, null); - 6", true);
                    Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.Death, pawn, null, null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Messages), "Message", new Type[]
    {
                typeof(string),
                typeof(LookTargets),
                typeof(MessageTypeDef),
                typeof(bool)
    })]
    internal static class Message_Patch
    {
        private static bool Prefix(string text, LookTargets lookTargets, MessageTypeDef def)
        {
            Log.Message("Message_Patch - Prefix - if (def == MessageTypeDefOf.PawnDeath && lookTargets.TryGetPrimaryTarget().Thing is Pawn pawn && (pawn.IsEmptySleeve() || pawn.HasStack())) - 7", true);
            if (def == MessageTypeDefOf.PawnDeath && lookTargets.TryGetPrimaryTarget().Thing is Pawn pawn && (pawn.IsEmptySleeve() || pawn.HasStack()))
            {
                Log.Message("Message_Patch - Prefix - return false; - 8", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("NotifyPlayerOfKilled")]
    internal static class DeadPawnMessageReplacement
    {
                public static bool disableKilledEffect = false;
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff, Caravan caravan)
        {
            Log.Message("DeadPawnMessageReplacement - Prefix - if (disableKilledEffect) - 11", true);
            if (disableKilledEffect)
            {
                try
                {

                    Log.Message("DeadPawnMessageReplacement - Prefix - if (!___pawn.IsEmptySleeve() && ___pawn.HasStack()) - 12", true);
                    if (!___pawn.IsEmptySleeve() && ___pawn.HasStack())
                    {
                        Log.Message("DeadPawnMessageReplacement - Prefix - TaggedString taggedString = \"\"; - 13", true);
                        TaggedString taggedString = "";
                        taggedString = (dinfo.HasValue ? "AlteredCarbon.SleveOf".Translate() + dinfo.Value.Def.deathMessage
                                        .Formatted(___pawn.LabelShortCap, ___pawn.Named("PAWN")) : ((hediff == null)
                                        ? "AlteredCarbon.PawnDied".Translate(___pawn.LabelShortCap, ___pawn.Named("PAWN"))
                                        : "AlteredCarbon.PawnDiedBecauseOf".Translate(___pawn.LabelShortCap, hediff.def.LabelCap,
                                        ___pawn.Named("PAWN"))));
                        Log.Message("DeadPawnMessageReplacement - Prefix - taggedString = taggedString.AdjustedFor(___pawn); - 15", true);
                        taggedString = taggedString.AdjustedFor(___pawn);
                        Log.Message("DeadPawnMessageReplacement - Prefix - TaggedString label = \"AlteredCarbon.SleeveDeath\".Translate() + \": \" + ___pawn.LabelShortCap; - 16", true);
                        TaggedString label = "AlteredCarbon.SleeveDeath".Translate() + ": " + ___pawn.LabelShortCap;
                        Log.Message("DeadPawnMessageReplacement - Prefix - Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NegativeEvent, ___pawn); - 17", true);
                        Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NegativeEvent, ___pawn);
                    }
                }
                catch (Exception ex)
                {
                    Log.Message("DeadPawnMessageReplacement - Prefix - Log.Error(ex.ToString()); - 18", true);
                    Log.Error(ex.ToString());
                }
                disableKilledEffect = false;
                Log.Message("DeadPawnMessageReplacement - Prefix - return false; - 20", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
    public class AppendThoughts_ForHumanlike_Patch
    {
                public static bool disableKilledEffect = false;
        public static bool Prefix(Pawn victim)
        {
            Log.Message("AppendThoughts_ForHumanlike_Patch - Prefix - if (disableKilledEffect) - 23", true);
            if (disableKilledEffect)
            {
                Log.Message("AppendThoughts_ForHumanlike_Patch - Prefix - disableKilledEffect = false; - 24", true);
                disableKilledEffect = false;
                Log.Message("AppendThoughts_ForHumanlike_Patch - Prefix - return false; - 25", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_Relations")]
    public class AppendThoughts_Relations_Patch
    {
                public static bool disableKilledEffect = false;
        public static bool Prefix(Pawn victim)
        {
            Log.Message("AppendThoughts_Relations_Patch - Prefix - if (disableKilledEffect) - 28", true);
            if (disableKilledEffect)
            {
                Log.Message("AppendThoughts_Relations_Patch - Prefix - disableKilledEffect = false; - 29", true);
                disableKilledEffect = false;
                Log.Message("AppendThoughts_Relations_Patch - Prefix - return false; - 30", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SocialCardUtility), "GetPawnSituationLabel")]
    public class Dead_Patch
    {
        public static bool Prefix(Pawn pawn, Pawn fromPOV, ref string __result)
        {
            Log.Message("Dead_Patch - Prefix - if (AlteredCarbonManager.Instance.deadPawns.Contains(pawn) && AlteredCarbonManager.Instance.stacksIndex.ContainsKey(pawn.thingIDNumber)) - 32", true);
            if (AlteredCarbonManager.Instance.deadPawns.Contains(pawn) && AlteredCarbonManager.Instance.stacksIndex.ContainsKey(pawn.thingIDNumber))
            {
                Log.Message("Dead_Patch - Prefix - __result = \"AlteredCarbon.NoSleeve\".Translate(); - 33", true);
                __result = "AlteredCarbon.NoSleeve".Translate();
                Log.Message("Dead_Patch - Prefix - return false; - 34", true);
                return false;
            }
            var stackHediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
                    x.def == AC_DefOf.UT_CorticalStack);
            Log.Message("Dead_Patch - Prefix - if (stackHediff != null && pawn.Dead) - 36", true);
            if (stackHediff != null && pawn.Dead)
            {
                Log.Message("Dead_Patch - Prefix - __result = \"AlteredCarbon.Sleeve\".Translate(); - 37", true);
                __result = "AlteredCarbon.Sleeve".Translate();
                Log.Message("Dead_Patch - Prefix - return false; - 38", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Faction), "Notify_LeaderDied")]
    public static class Notify_LeaderDied_Patch
    {
                public static bool disableKilledEffect = false;
        public static bool Prefix()
        {
            Log.Message("Notify_LeaderDied_Patch - Prefix - if (disableKilledEffect) - 41", true);
            if (disableKilledEffect)
            {
                Log.Message("Notify_LeaderDied_Patch - Prefix - disableKilledEffect = false; - 42", true);
                disableKilledEffect = false;
                Log.Message("Notify_LeaderDied_Patch - Prefix - return false; - 43", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StatsRecord), "Notify_ColonistKilled")]
    public static class Notify_ColonistKilled_Patch
    {
                public static bool disableKilledEffect = false;
        public static bool Prefix()
        {
            Log.Message("Notify_ColonistKilled_Patch - Prefix - if (disableKilledEffect) - 46", true);
            if (disableKilledEffect)
            {
                Log.Message("Notify_ColonistKilled_Patch - Prefix - disableKilledEffect = false; - 47", true);
                disableKilledEffect = false;
                Log.Message("Notify_ColonistKilled_Patch - Prefix - return false; - 48", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), "Notify_PawnKilled")]
    public static class Notify_PawnKilled_Patch
    {
                public static bool disableKilledEffect = false;
        public static bool Prefix()
        {
            Log.Message("Notify_PawnKilled_Patch - Prefix - if (disableKilledEffect) - 51", true);
            if (disableKilledEffect)
            {
                Log.Message("Notify_PawnKilled_Patch - Prefix - disableKilledEffect = false; - 52", true);
                disableKilledEffect = false;
                Log.Message("Notify_PawnKilled_Patch - Prefix - return false; - 53", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Ideo), "RecacheColonistBelieverCount")]
    public static class RecacheColonistBelieverCount_Patch
    {
                public static bool includeStackPawns = false;
        public static void Prefix()
        {
            Log.Message("RecacheColonistBelieverCount_Patch - Prefix - includeStackPawns = true; - 56", true);
            includeStackPawns = true;
        }
        public static void Postfix()
        {
            Log.Message("RecacheColonistBelieverCount_Patch - Postfix - includeStackPawns = false; - 57", true);
            includeStackPawns = false;
        }
    }

    [HarmonyPatch(typeof(PawnsFinder), "AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep", MethodType.Getter)]
    public static class AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch
    {
        public static void Postfix(ref List<Pawn> __result)
        {
            Log.Message("AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch - Postfix - if (RecacheColonistBelieverCount_Patch.includeStackPawns) - 58", true);
            if (RecacheColonistBelieverCount_Patch.includeStackPawns)
            {
                Log.Message("AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch - Postfix - var pawns = AlteredCarbonManager.Instance.PawnsWithStacks.Concat(AlteredCarbonManager.Instance.deadPawns ?? Enumerable.Empty<Pawn>()).ToList(); - 59", true);
                var pawns = AlteredCarbonManager.Instance.PawnsWithStacks.Concat(AlteredCarbonManager.Instance.deadPawns ?? Enumerable.Empty<Pawn>()).ToList();
                Log.Message("AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch - Postfix - foreach (var pawn in pawns) - 60", true);
                foreach (var pawn in pawns)
                {
                    Log.Message("AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch - Postfix - if (pawn?.ideo != null && pawn.Ideo != null && !__result.Contains(pawn)) - 61", true);
                    if (pawn?.ideo != null && pawn.Ideo != null && !__result.Contains(pawn))
                    {
                        Log.Message("AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch - Postfix - __result.Add(pawn); - 62", true);
                        __result.Add(pawn);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public class Pawn_Kill_Patch
    {
        public static void Prefix(out Caravan __state, Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            Log.Message("Pawn_Kill_Patch - Prefix - __state = null; - 63", true);
            __state = null;
            try
            {
                Log.Message("Pawn_Kill_Patch - Prefix - if (dinfo.HasValue && dinfo.Value.Def == DamageDefOf.Crush && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse) - 64", true);
                if (dinfo.HasValue && dinfo.Value.Def == DamageDefOf.Crush && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse)
                {
                    Log.Message("Pawn_Kill_Patch - Prefix - return; - 65", true);
                    return;
                }
                Log.Message("Pawn_Kill_Patch - Prefix - if (__instance != null && (__instance.HasStack() || __instance.IsEmptySleeve())) - 66", true);
                if (__instance != null && (__instance.HasStack() || __instance.IsEmptySleeve()))
                {
                    Log.Message("Pawn_Kill_Patch - Prefix - Notify_ColonistKilled_Patch.disableKilledEffect = true; - 67", true);
                    Notify_ColonistKilled_Patch.disableKilledEffect = true;
                    Log.Message("Pawn_Kill_Patch - Prefix - Notify_PawnKilled_Patch.disableKilledEffect = true; - 68", true);
                    Notify_PawnKilled_Patch.disableKilledEffect = true;
                    Log.Message("Pawn_Kill_Patch - Prefix - Notify_LeaderDied_Patch.disableKilledEffect = true; - 69", true);
                    Notify_LeaderDied_Patch.disableKilledEffect = true;
                    Log.Message("Pawn_Kill_Patch - Prefix - AppendThoughts_ForHumanlike_Patch.disableKilledEffect = true; - 70", true);
                    AppendThoughts_ForHumanlike_Patch.disableKilledEffect = true;
                    Log.Message("Pawn_Kill_Patch - Prefix - AppendThoughts_Relations_Patch.disableKilledEffect = true; - 71", true);
                    AppendThoughts_Relations_Patch.disableKilledEffect = true;
                    Log.Message("Pawn_Kill_Patch - Prefix - DeadPawnMessageReplacement.disableKilledEffect = true; - 72", true);
                    DeadPawnMessageReplacement.disableKilledEffect = true;
                }
                var stackHediff = __instance.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
                Log.Message("Pawn_Kill_Patch - Prefix - if (stackHediff != null) - 74", true);
                if (stackHediff != null)
                {
                    Log.Message("Pawn_Kill_Patch - Prefix - if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(__instance)) - 75", true);
                    if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(__instance))
                    {
                        Log.Message("Pawn_Kill_Patch - Prefix - stackHediff.PersonaData.diedFromCombat = true; - 76", true);
                        stackHediff.PersonaData.diedFromCombat = true;
                    }
                    LessonAutoActivator.TeachOpportunity(AC_DefOf.UT_DeadPawnWithStack, __instance, OpportunityType.Important);
                    Log.Message("Pawn_Kill_Patch - Prefix - AlteredCarbonManager.Instance.deadPawns.Add(__instance); - 78", true);
                    AlteredCarbonManager.Instance.deadPawns.Add(__instance);
                    Log.Message("Pawn_Kill_Patch - Prefix - __state = __instance.GetCaravan(); - 79", true);
                    __state = __instance.GetCaravan();
                }
                Log.Message("Pawn_Kill_Patch - Prefix - if (AlteredCarbonManager.Instance.stacksIndex.TryGetValue(__instance.thingIDNumber, out var corticalStack)) - 80", true);
                if (AlteredCarbonManager.Instance.stacksIndex.TryGetValue(__instance.thingIDNumber, out var corticalStack))
                {
                    Log.Message("Pawn_Kill_Patch - Prefix - if (LookTargets_Patch.targets.TryGetValue(__instance, out var targets)) - 81", true);
                    if (LookTargets_Patch.targets.TryGetValue(__instance, out var targets))
                    {
                        Log.Message("Pawn_Kill_Patch - Prefix - foreach (var target in targets) - 82", true);
                        foreach (var target in targets)
                        {
                            Log.Message("Pawn_Kill_Patch - Prefix - target.targets.Remove(__instance); - 83", true);
                            target.targets.Remove(__instance);
                            Log.Message("Pawn_Kill_Patch - Prefix - target.targets.Add(corticalStack); - 84", true);
                            target.targets.Add(corticalStack);
                        }
                    }
                }
            }
            catch { };
        }
        public static void Postfix(Caravan __state, Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            Log.Message("Pawn_Kill_Patch - Postfix - if (__state != null && __state.PawnsListForReading.Any()) - 86", true);
            if (__state != null && __state.PawnsListForReading.Any())
            {
                Log.Message("Pawn_Kill_Patch - Postfix - var stackHediff = __instance.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack; - 87", true);
                var stackHediff = __instance.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
                Log.Message("Pawn_Kill_Patch - Postfix - if (stackHediff.def.spawnThingOnRemoved != null) - 88", true);
                if (stackHediff.def.spawnThingOnRemoved != null)
                {
                    Log.Message("Pawn_Kill_Patch - Postfix - var corticalStackThing = ThingMaker.MakeThing(stackHediff.def.spawnThingOnRemoved) as CorticalStack; - 89", true);
                    var corticalStackThing = ThingMaker.MakeThing(stackHediff.def.spawnThingOnRemoved) as CorticalStack;
                    Log.Message("Pawn_Kill_Patch - Postfix - if (stackHediff.PersonaData.hasPawn) - 90", true);
                    if (stackHediff.PersonaData.hasPawn)
                    {
                        Log.Message("Pawn_Kill_Patch - Postfix - stackHediff.PersonaData.CopyDataFrom(stackHediff.PersonaData); - 91", true);
                        stackHediff.PersonaData.CopyDataFrom(stackHediff.PersonaData);
                    }
                    else
                    {
                        Log.Message("Pawn_Kill_Patch - Postfix - stackHediff.PersonaData.CopyPawn(__instance); - 92", true);
                        stackHediff.PersonaData.CopyPawn(__instance);
                    }
                    AlteredCarbonManager.Instance.RegisterStack(corticalStackThing);
                    Log.Message("Pawn_Kill_Patch - Postfix - AlteredCarbonManager.Instance.RegisterSleeve(__instance, corticalStackThing.PersonaData.stackGroupID); - 94", true);
                    AlteredCarbonManager.Instance.RegisterSleeve(__instance, corticalStackThing.PersonaData.stackGroupID);
                    Log.Message("Pawn_Kill_Patch - Postfix - CaravanInventoryUtility.GiveThing(__state, corticalStackThing); - 95", true);
                    CaravanInventoryUtility.GiveThing(__state, corticalStackThing);
                }
                var head = __instance.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
                Log.Message("Pawn_Kill_Patch - Postfix - if (head != null) - 97", true);
                if (head != null)
                {
                    Log.Message("Pawn_Kill_Patch - Postfix - Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, __instance, head); - 98", true);
                    Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, __instance, head);
                    Log.Message("Pawn_Kill_Patch - Postfix - hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut; - 99", true);
                    hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                    Log.Message("Pawn_Kill_Patch - Postfix - hediff_MissingPart.IsFresh = true; - 100", true);
                    hediff_MissingPart.IsFresh = true;
                    Log.Message("Pawn_Kill_Patch - Postfix - __instance.health.AddHediff(hediff_MissingPart); - 101", true);
                    __instance.health.AddHediff(hediff_MissingPart);
                }
                __instance.health.RemoveHediff(stackHediff);
            }
        }

    }

    [HarmonyPatch(typeof(LookTargets), MethodType.Constructor, new Type[] { typeof(Thing) })]
    public static class LookTargets_Patch
    {
                public static Dictionary<Pawn, List<LookTargets>> targets = new Dictionary<Pawn, List<LookTargets>>();
        public static void Postfix(LookTargets __instance, Thing t)
        {
            Log.Message("LookTargets_Patch - Postfix - if (t is Pawn pawn) - 104", true);
            if (t is Pawn pawn)
            {
                Log.Message("LookTargets_Patch - Postfix - if (targets.ContainsKey(pawn)) - 105", true);
                if (targets.ContainsKey(pawn))
                {
                    Log.Message("LookTargets_Patch - Postfix - targets[pawn].Add(__instance); - 106", true);
                    targets[pawn].Add(__instance);
                }
                else
                {
                    Log.Message("LookTargets_Patch - Postfix - targets[pawn] = new List<LookTargets> { __instance }; - 107", true);
                    targets[pawn] = new List<LookTargets> { __instance };
                }
            }
        }
    }

	[HarmonyPatch(typeof(ColonistBarColonistDrawer), "HandleClicks")]
	public static class HandleClicks_Patch
	{
		public static bool Prefix(Rect rect, Pawn colonist, int reorderableGroup, out bool reordering)
		{
			reordering = false;
			if (colonist.Dead)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 1 && Mouse.IsOver(rect))
                {
					Event.current.Use();
					if (AlteredCarbonManager.Instance.stacksIndex.TryGetValue(colonist.thingIDNumber, out var corticalStack))
					{
						if (corticalStack != null)
						{
							CameraJumper.TryJumpAndSelect(colonist);
						}
					}
				}

				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && Mouse.IsOver(rect))
				{
					Event.current.Use();
					if (AlteredCarbonManager.Instance.stacksIndex.TryGetValue(colonist.thingIDNumber, out var corticalStack))
					{
						if (corticalStack is null)
						{
							CameraJumper.TryJumpAndSelect(colonist);
						}
						else
						{
							CameraJumper.TryJumpAndSelect(corticalStack);
						}
					}
					else
					{
						CameraJumper.TryJumpAndSelect(colonist);
					}
				}
				reordering = ReorderableWidget.Reorderable(reorderableGroup, rect, useRightButton: true);
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
				{
					Event.current.Use();
				}
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(ColonistBarColonistDrawer), "DrawColonist")]
	[StaticConstructorOnStartup]
	public static class DrawColonist_Patch
	{
		private static readonly Texture2D Icon_StackDead = ContentFinder<Texture2D>.Get("UI/Icons/StackDead");

		public static bool Prefix(Rect rect, Pawn colonist, Map pawnMap, bool highlight, bool reordering,
			Dictionary<string, string> ___pawnLabelsCache, Vector2 ___PawnTextureSize,
			Texture2D ___MoodBGTex, Vector2[] ___bracketLocs)
		{
			if (colonist.Dead && colonist.HasStack())
			{
				float alpha = Find.ColonistBar.GetEntryRectAlpha(rect);
				ApplyEntryInAnotherMapAlphaFactor(pawnMap, ref alpha);
				if (reordering)
				{
					alpha *= 0.5f;
				}
				Color color2 = GUI.color = new Color(1f, 1f, 1f, alpha);
				GUI.DrawTexture(rect, ColonistBar.BGTex);
				if (colonist.needs != null && colonist.needs.mood != null)
				{
					Rect position = rect.ContractedBy(2f);
					float num = position.height * colonist.needs.mood.CurLevelPercentage;
					position.yMin = position.yMax - num;
					position.height = num;
					GUI.DrawTexture(position, ___MoodBGTex);
				}
				if (highlight)
				{
					int thickness = (rect.width <= 22f) ? 2 : 3;
					GUI.color = Color.white;
					Widgets.DrawBox(rect, thickness);
					GUI.color = color2;
				}
				Rect rect2 = rect.ContractedBy(-2f * Find.ColonistBar.Scale);
				if ((colonist.Dead ? Find.Selector.SelectedObjects.Contains(colonist.Corpse) : Find.Selector.SelectedObjects.Contains(colonist)) && !WorldRendererUtility.WorldRenderedNow)
				{
					DrawSelectionOverlayOnGUI(colonist, rect2, ___bracketLocs);
				}

				GUI.DrawTexture(GetPawnTextureRect(rect.position, ___PawnTextureSize), PortraitsCache.Get(colonist, ColonistBarColonistDrawer.PawnTextureSize, Rot4.South, ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f, true, true, true, true, null));
				GUI.color = new Color(1f, 1f, 1f, alpha * 0.8f);

				float num3 = 20f * Find.ColonistBar.Scale;
				Vector2 pos2 = new Vector2(rect.x + 1f, rect.yMax - num3 - 1f);
				DrawIcon(Icon_StackDead, ref pos2, "ActivityIconMedicalRest".Translate());
				GUI.color = color2;

				float num2 = 4f * Find.ColonistBar.Scale;
				Vector2 pos = new Vector2(rect.center.x, rect.yMax - num2);
				GenMapUI.DrawPawnLabel(colonist, pos, alpha, rect.width + Find.ColonistBar.SpaceBetweenColonistsHorizontal - 2f, ___pawnLabelsCache);
				Text.Font = GameFont.Small;
				GUI.color = Color.white;
				return false;
			}
			return true;
		}

		private static void DrawIcon(Texture2D icon, ref Vector2 pos, string tooltip)
		{
			float num = 20f * Find.ColonistBar.Scale;
			Rect rect = new Rect(pos.x, pos.y, num, num);
			GUI.DrawTexture(rect, icon);
			TooltipHandler.TipRegion(rect, tooltip);
			pos.x += num;
		}

		public static Rect GetPawnTextureRect(Vector2 pos, Vector2 ___PawnTextureSize)
		{
			float x = pos.x;
			float y = pos.y;
			Vector2 vector = ___PawnTextureSize * Find.ColonistBar.Scale;
			return new Rect(x + 1f, y - (vector.y - Find.ColonistBar.Size.y) - 1f, vector.x, vector.y).ContractedBy(1f);
		}

		private static void ApplyEntryInAnotherMapAlphaFactor(Map map, ref float alpha)
		{
			if (map == null)
			{
				if (!WorldRendererUtility.WorldRenderedNow)
				{
					alpha = Mathf.Min(alpha, 0.4f);
				}
			}
			else if (map != Find.CurrentMap || WorldRendererUtility.WorldRenderedNow)
			{
				alpha = Mathf.Min(alpha, 0.4f);
			}
		}

		private static void DrawSelectionOverlayOnGUI(Pawn colonist, Rect rect, Vector2[] ___bracketLocs)
		{
			Thing obj = colonist;
			if (colonist.Dead)
			{
				obj = colonist.Corpse;
			}
			float num = 0.4f * Find.ColonistBar.Scale;
			SelectionDrawerUtility.CalculateSelectionBracketPositionsUI<object>(textureSize: new Vector2((float)SelectionDrawerUtility.SelectedTexGUI.width * num, (float)SelectionDrawerUtility.SelectedTexGUI.height * num), bracketLocs: ___bracketLocs, obj: (object)obj, rect: rect, selectTimes: SelectionDrawer.SelectTimes, jumpDistanceFactor: 20f * Find.ColonistBar.Scale);
			DrawSelectionOverlayOnGUI(___bracketLocs, num);
		}

		private static void DrawSelectionOverlayOnGUI(Vector2[] bracketLocs, float selectedTexScale)
		{
			int num = 90;
			for (int i = 0; i < 4; i++)
			{
				Widgets.DrawTextureRotated(bracketLocs[i], SelectionDrawerUtility.SelectedTexGUI, num, selectedTexScale);
				num += 90;
			}
		}
	}
}

