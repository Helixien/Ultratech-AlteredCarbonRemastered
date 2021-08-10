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
				Pawn pawn = map.mapPawns.AllPawns[num];
				if ((pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer) && pawn.HasStack())
                {
					pawn.DeSpawn(DestroyMode.Vanish);
					TaggedString label = "Death".Translate() + ": " + pawn.LabelShortCap;
					TaggedString taggedString = "PawnDied".Translate(pawn.LabelShortCap, pawn.Named("PAWN"));
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
			if (def == MessageTypeDefOf.PawnDeath && lookTargets.TryGetPrimaryTarget().Thing is Pawn pawn && (pawn.IsEmptySleeve() || pawn.HasStack()))
            {
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
			if (disableKilledEffect)
			{
				try
				{

					if (!___pawn.IsEmptySleeve() && ___pawn.HasStack())
					{
						TaggedString taggedString = "";
						taggedString = (dinfo.HasValue ? "AlteredCarbon.SleveOf".Translate() + dinfo.Value.Def.deathMessage
								.Formatted(___pawn.LabelShortCap, ___pawn.Named("PAWN")) : ((hediff == null)
								? "AlteredCarbon.PawnDied".Translate(___pawn.LabelShortCap, ___pawn.Named("PAWN"))
								: "AlteredCarbon.PawnDiedBecauseOf".Translate(___pawn.LabelShortCap, hediff.def.LabelCap,
								___pawn.Named("PAWN"))));
						taggedString = taggedString.AdjustedFor(___pawn);
						TaggedString label = "AlteredCarbon.SleeveDeath".Translate() + ": " + ___pawn.LabelShortCap;
						Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NegativeEvent, ___pawn);
					}
				}
				catch (Exception ex)
                {
					Log.Error(ex.ToString());
                }
				disableKilledEffect = false;
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
			if (disableKilledEffect)
			{
				disableKilledEffect = false;
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
			if (disableKilledEffect)
			{
				disableKilledEffect = false;
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
			if (AlteredCarbonManager.Instance.deadPawns.Contains(pawn) && AlteredCarbonManager.Instance.stacksIndex.ContainsKey(pawn.thingIDNumber))
			{
				__result = "AlteredCarbon.NoSleeve".Translate();
				return false;
			}
			var stackHediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
				x.def == AC_DefOf.UT_CorticalStack);
			if (stackHediff != null && pawn.Dead)
			{
				__result = "AlteredCarbon.Sleeve".Translate();
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
			if (disableKilledEffect)
			{
				disableKilledEffect = false;
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
			if (disableKilledEffect)
			{
				disableKilledEffect = false;
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
			if (disableKilledEffect)
			{
				disableKilledEffect = false;
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
			includeStackPawns = true;
		}
		public static void Postfix()
        {
			includeStackPawns = false;
		}
	}

	[HarmonyPatch(typeof(PawnsFinder), "AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep", MethodType.Getter)]
	public static class AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch
	{
		public static void Postfix(ref List<Pawn> __result)
		{
			if (RecacheColonistBelieverCount_Patch.includeStackPawns)
            {
				var pawns = AlteredCarbonManager.Instance.PawnsWithStacks.Concat(AlteredCarbonManager.Instance.deadPawns ?? Enumerable.Empty<Pawn>()).ToList();
				foreach (var pawn in pawns)
                {
					if (pawn?.ideo != null && pawn.Ideo != null && !__result.Contains(pawn))
                    {
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
			__state = null;
			try
			{
				if (dinfo.HasValue && dinfo.Value.Def == DamageDefOf.Crush && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse)
				{
					return;
				}
				if (__instance != null && (__instance.HasStack() || __instance.IsEmptySleeve()))
				{
					Notify_ColonistKilled_Patch.disableKilledEffect = true;
					Notify_PawnKilled_Patch.disableKilledEffect = true;
					Notify_LeaderDied_Patch.disableKilledEffect = true;
					AppendThoughts_ForHumanlike_Patch.disableKilledEffect = true;
					AppendThoughts_Relations_Patch.disableKilledEffect = true;
					DeadPawnMessageReplacement.disableKilledEffect = true;
				}
				var stackHediff = __instance.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
				if (stackHediff != null)
				{
					if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(__instance))
                    {
						stackHediff.PersonaData.diedFromCombat = true;
                    }
					LessonAutoActivator.TeachOpportunity(AC_DefOf.UT_DeadPawnWithStack, __instance, OpportunityType.Important);
					AlteredCarbonManager.Instance.deadPawns.Add(__instance);
					__state = __instance.GetCaravan();
				}
				if (AlteredCarbonManager.Instance.stacksIndex.TryGetValue(__instance.thingIDNumber, out var corticalStack))
				{
					if (LookTargets_Patch.targets.TryGetValue(__instance, out var targets))
					{
						foreach (var target in targets)
						{
							target.targets.Remove(__instance);
							target.targets.Add(corticalStack);
						}
					}
				}
			}
			catch { };
		}
		public static void Postfix(Caravan __state, Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
			if (__state != null && __state.PawnsListForReading.Any())
            {
				var stackHediff = __instance.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.UT_CorticalStack) as Hediff_CorticalStack;
				if (stackHediff.def.spawnThingOnRemoved != null)
				{
					var corticalStackThing = ThingMaker.MakeThing(stackHediff.def.spawnThingOnRemoved) as CorticalStack;
					if (stackHediff.PersonaData.hasPawn)
					{
						stackHediff.PersonaData.CopyDataFrom(stackHediff.PersonaData);
					}
					else
					{
						stackHediff.PersonaData.CopyPawn(__instance);
					}
					AlteredCarbonManager.Instance.RegisterStack(corticalStackThing);
					AlteredCarbonManager.Instance.RegisterSleeve(__instance, corticalStackThing.PersonaData.stackGroupID);
					CaravanInventoryUtility.GiveThing(__state, corticalStackThing);
				}
				var head = __instance.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
				if (head != null)
				{
					Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, __instance, head);
					hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
					hediff_MissingPart.IsFresh = true;
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
			if (t is Pawn pawn)
			{
				if (targets.ContainsKey(pawn))
				{
					targets[pawn].Add(__instance);
				}
				else
				{
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

