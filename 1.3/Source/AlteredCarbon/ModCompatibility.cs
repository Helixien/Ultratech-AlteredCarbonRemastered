using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	public static class ModCompatibility
	{
		public static Color GetSkinColorFirst(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("skin").first;
			}
			else
			{
				return Color.white;
			}
		}

		public static Color GetSkinColorSecond(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("skin").second;
			}
			else
			{
				return Color.white;
			}
		}
		public static void SetSkinColorFirst(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("skin", color, null);
			}
		}
		public static void SetSkinColorSecond(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("skin", null, color);
			}
		}


		public static Color GetHairColorFirst(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("hair").first;
			}
			else
			{
				return Color.white;
			}
		}

		public static Color GetHairColorSecond(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("hair").second;
			}
			else
			{
				return Color.white;
			}
		}
		public static void SetHairColorFirst(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("hair", color, null);
				pawn.story.hairColor = color;
			}
		}
		public static void SetHairColorSecond(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("hair", null, color);
			}
		}
		public static void SetAlienHead(Pawn pawn, string head)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.crownType = head;
			}
		}


		public static string GetAlienHead(Pawn pawn)
		{
			string sRet = "(unknown)";
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				sRet = alienComp.crownType;
			}
			return sRet;
		}
		public static List<string> GetAlienHeadPaths(Pawn pawn)
        {
			var alienDef = pawn.def as AlienRace.ThingDef_AlienRace;
			return alienDef.alienRace.generalSettings.alienPartGenerator.aliencrowntypes;
		}

		public static void CopyBodyAddons(Pawn source, Pawn to)
		{
			var sourceComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(source);
			if (sourceComp != null && sourceComp.addonGraphics != null && sourceComp.addonVariants != null)
            {
				var toComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(to);
				if (toComp != null)
                {
					toComp.addonGraphics = sourceComp.addonGraphics.ListFullCopy();
					toComp.addonVariants = sourceComp.addonVariants.ListFullCopy();
                }
			}
		}
		public static List<Color> GetRacialColorPresets(ThingDef thingDef, string channelName)
		{
			ColorGenerator generator = null;
			AlienRace.ThingDef_AlienRace raceDef = thingDef as AlienRace.ThingDef_AlienRace;
			for (int ii = 0; ii < raceDef.alienRace.generalSettings.alienPartGenerator.colorChannels.Count(); ++ii)
            {
				if (raceDef.alienRace.generalSettings.alienPartGenerator.colorChannels[ii].name == channelName)
                {
					generator = raceDef.alienRace.generalSettings.alienPartGenerator.colorChannels[ii].first;
					break;
                }
			}
			if (generator != null)
			{
				ColorGenerator_Options options = generator as ColorGenerator_Options;
				if (options != null)
				{
					return options.options.Where((ColorOption option) => {
						return option.only.a > -1.0f;
					}).Select((ColorOption option) => {
						return option.only;
					}).ToList();
				}
				ColorGenerator_Single single = generator as ColorGenerator_Single;
				if (single != null)
				{
					return new List<Color>() { single.color };
				}
				ColorGenerator_White white = generator as ColorGenerator_White;
				if (white != null)
				{
					return new List<Color>() { Color.white };
				}
			}
			return new List<Color>();
		}

		public static int GetSyrTraitsSexuality(Pawn pawn)
        {
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				return (int)comp.sexuality;
			}
			return -1;
		}
		public static float GetSyrTraitsRomanceFactor(Pawn pawn)
		{
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				return comp.RomanceFactor;
			}
			return -1f;
		}

		public static void SetSyrTraitsSexuality(Pawn pawn, int sexuality)
		{
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				comp.sexuality = (SyrTraits.CompIndividuality.Sexuality)sexuality;
			}
		}
		public static void SetSyrTraitsRomanceFactor(Pawn pawn, float romanceFactor)
		{
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				comp.RomanceFactor = romanceFactor;
			}
		}

		public static PsychologyData GetPsychologyData(Pawn pawn)
		{
			var comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
			if (comp != null)
			{
				var psychologyData = new PsychologyData();
				var sexualityTracker = comp.Sexuality;
				psychologyData.sexDrive = sexualityTracker.sexDrive;
				psychologyData.romanticDrive = sexualityTracker.romanticDrive;
				psychologyData.kinseyRating = sexualityTracker.kinseyRating;
				psychologyData.knownSexualities = Traverse.Create(sexualityTracker).Field<Dictionary<Pawn, int>>("knownSexualities").Value;
				return psychologyData;
			}
			return null;
		}

		public static void SetPsychologyData(Pawn pawn, PsychologyData psychologyData)
		{
			var comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
			if (comp != null)
			{
				var sexualityTracker = new Psychology.Pawn_SexualityTracker(pawn);
				sexualityTracker.sexDrive = psychologyData.sexDrive;
				sexualityTracker.romanticDrive = psychologyData.romanticDrive;
				sexualityTracker.kinseyRating = psychologyData.kinseyRating;
				Traverse.Create(sexualityTracker).Field<Dictionary<Pawn, int>>("knownSexualities").Value = psychologyData.knownSexualities;
				comp.Sexuality = sexualityTracker;
			}
		}

		public static void UpdateGenderRestrictions(ThingDef raceDef, out bool allowMales, out bool allowFemales)
        {
			float maleProb = ((AlienRace.ThingDef_AlienRace)raceDef).alienRace.generalSettings.maleGenderProbability;
			allowMales = maleProb != 0.0f;
			allowFemales = maleProb != 1.0f;
        }

		public static List<HairDef> GetPermittedHair(ThingDef raceDef)
        {
			if (((AlienRace.ThingDef_AlienRace)raceDef).alienRace.styleSettings[typeof(HairDef)].styleTags == null)
			{
				//no good way to distinguish between alien specific hair and hair suitable for generic humanlikes
				return DefDatabase<HairDef>.AllDefs.ToList();
			}
            else
			{
				List<string> allowedTags = ((AlienRace.ThingDef_AlienRace)raceDef).alienRace.styleSettings[typeof(HairDef)].styleTags.ToList();
				return DefDatabase<HairDef>.AllDefs.Where(x => x.styleTags.Intersect(allowedTags).Any()).ToList();
			}
		}

		public static List<BodyTypeDef> GetAllowedBodyTypes(ThingDef raceDef)
        {
			var alienRace = raceDef as AlienRace.ThingDef_AlienRace;
			if (alienRace.alienRace?.generalSettings?.alienPartGenerator?.alienbodytypes?.Any() ?? false)
            {
				return alienRace.alienRace.generalSettings.alienPartGenerator.alienbodytypes;
            }
			return DefDatabase<BodyTypeDef>.AllDefsListForReading;
		}
		public static List<ThingDef> GetGrowableRaces(List<ThingDef> excluded)
		{
			return DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where(x => !excluded.Contains(x)).Cast<ThingDef>().ToList();
		}
		static ModCompatibility()
        {
			AlienRacesIsActive = HasActiveModWithPackageID("erdelf.HumanoidAlienRaces");
			IndividualityIsActive = ModLister.HasActiveModWithName("[SYR] Individuality");
			PsychologyIsActive = HasActiveModWithPackageID("Community.Psychology.UnofficialUpdate");
			RimJobWorldIsActive = HasActiveModWithPackageID("rim.job.world");
			if (RimJobWorldIsActive)
            {
				raceGroupDef_HelperType = AccessTools.TypeByName("RaceGroupDef_Helper");
				tryGetRaceGroupDef = raceGroupDef_HelperType.GetMethods().FirstOrDefault(x => x.Name == "TryGetRaceGroupDef");
			}
			DubsBadHygieneActive = HasActiveModWithPackageID("Dubwise.DubsBadHygiene");
		}

		public static void FillThirstNeed(Pawn pawn, float value) 
		{
			var need = pawn.needs.TryGetNeed<DubsBadHygiene.Need_Thirst>();
			if (need != null)
            {
				need.CurLevel += value;
            }
		}

		public static void FillHygieneNeed(Pawn pawn, float value)
		{
			var need = pawn.needs.TryGetNeed<DubsBadHygiene.Need_Hygiene>();
			if (need != null)
			{
				need.CurLevel += value;
			}
		}
		private static MethodInfo tryGetRaceGroupDef;
		private static Type raceGroupDef_HelperType;
		public static bool RJWAllowsThisFor(this HediffDef hediffDef, Pawn pawn)
		{
			try
			{
				var part = DefDatabase<rjw.RacePartDef>.GetNamedSilentFail(hediffDef.defName);
				if (part != null)
                {
					var parms = new object[2];
					parms[0] = pawn.kindDef;

					if ((bool)tryGetRaceGroupDef.Invoke(null, parms))
					{
						var def = parms[1] as rjw.RaceGroupDef;
						if (hediffDef.IsBreasts())
						{
							if (def.femaleBreasts != null || def.maleBreasts != null)
                            {
								if (def.femaleBreasts != null && def.femaleBreasts.Contains(hediffDef.defName))
								{
									return true;
								}
								else if (def.maleBreasts != null && def.maleBreasts.Contains(hediffDef.defName))
								{
									return true;
								}
								return false;
							}
						}

						if (hediffDef.IsGenitals())
						{
							if (def.femaleGenitals != null || def.maleGenitals != null)
							{
								if (def.femaleGenitals != null && def.femaleGenitals.Contains(hediffDef.defName))
								{
									return true;
								}
								else if (def.maleGenitals != null && def.maleGenitals.Contains(hediffDef.defName))
								{
									return true;
								}
								return false;
							}
						}
						if (hediffDef.IsAnus())
						{
							if (def.anuses != null)
                            {
								if (!def.anuses.Contains(hediffDef.defName))
                                {
									return false;
                                }
                            }
						}

						if (hediffDef.IsOvipositor())
                        {
							if (!def.oviPregnancy)
                            {
								return false;
                            }
                        }
					}
				}

				if (hediffDef.IsGenitals())
                {
					return hediffDef == rjw.Genital_Helper.average_penis || hediffDef == rjw.Genital_Helper.average_vagina;
				}
				else if (hediffDef.IsBreasts())
                {
					return hediffDef == rjw.Genital_Helper.average_breasts;
                }
				else if (hediffDef.IsAnus())
                {
					return hediffDef == rjw.Genital_Helper.average_anus;
                }
				else if (hediffDef.IsOvipositor())
                {
					return false;
                }
			}
			catch (Exception ex)
			{
				Log.Error("ERROR: " + ex);
			}
			return true;
		}

		private static bool IsOvipositor(this HediffDef hediffDef)
        {
			return hediffDef.defName.ToLower().Contains("ovipositor");
		}
		private static bool IsBreasts(this HediffDef hediffDef)
        {
			return hediffDef.defName.ToLower().Contains("breasts");
		}
		private static bool IsGenitals(this HediffDef hediffDef)
		{
			return hediffDef.defName.ToLower().Contains("penis") || hediffDef.defName.ToLower().Contains("vagina");
		}
		private static bool IsAnus(this HediffDef hediffDef)
		{
			return hediffDef.defName.ToLower().Contains("anus");
		}

		public static RJWData GetRjwData(Pawn pawn)
		{
			RJWData rjwData = null;
			var dataStore = Find.World.GetComponent<rjw.DataStore>();
			if (dataStore != null)
			{
				rjwData = new RJWData();
				var pawnData = dataStore.GetPawnData(pawn);
				foreach (var fieldInfo in typeof(rjw.PawnData).GetFields())
				{
					try
					{
						var newField = rjwData.GetType().GetField(fieldInfo.Name);
						newField.SetValue(rjwData, fieldInfo.GetValue(pawnData));
					}
					catch { }
				}
			}
			var comp = ThingCompUtility.TryGetComp<rjw.CompRJW>(pawn);
			if (comp != null)
			{
				if (rjwData is null)
                {
					rjwData = new RJWData();
                }
				rjwData.quirksave = comp.quirksave;
				rjwData.orientation = (OrientationAC)(int)comp.orientation;
				rjwData.NextHookupTick = comp.NextHookupTick;
			}
			return rjwData;
		}

		public static void SetRjwData(Pawn pawn, RJWData rjwData)
		{
			var dataStore = Find.World.GetComponent<rjw.DataStore>();
			if (dataStore != null)
			{
				var pawnData = dataStore.GetPawnData(pawn);
				if (pawnData != null)
                {
					foreach (var fieldInfo in typeof(RJWData).GetFields())
					{
						try
						{
							var newField = pawnData.GetType().GetField(fieldInfo.Name);
							newField.SetValue(pawnData, fieldInfo.GetValue(rjwData));
						}
						catch { }
					}
					if (pawnData.Hero)
                    {
						foreach (var otherPawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
                        {
							if (otherPawn != pawn)
                            {
								var otherPawnData = dataStore.GetPawnData(otherPawn);
								otherPawnData.Hero = false;
							}
						}
                    }
				}
			}

			var comp = ThingCompUtility.TryGetComp<rjw.CompRJW>(pawn);
			if (comp != null)
			{
				comp.quirksave = rjwData.quirksave;
				comp.quirks = new System.Text.StringBuilder(comp.quirksave);

				comp.orientation = (rjw.Orientation)(int)rjwData.orientation;
				comp.NextHookupTick = rjwData.NextHookupTick;
			}
		}

		private static bool HasActiveModWithPackageID(string packageID)
		{
			var mods = ModLister.AllInstalledMods.ToList();
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].Active && mods[i].PackageIdPlayerFacing == packageID)
				{
					return true;
				}
			}
			return false;
		}

		public static bool AlienRacesIsActive;
		public static bool IndividualityIsActive;
		public static bool PsychologyIsActive;
		public static bool RimJobWorldIsActive;
		public static bool DubsBadHygieneActive;
	}

}
