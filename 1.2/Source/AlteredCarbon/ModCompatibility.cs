using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;


namespace AlteredCarbon
{
	public static class ModCompatibility
	{
		public static Color GetSkinColor(Pawn pawn)
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
		public static void SetSkinColor(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
            {
				alienComp.GetChannel("skin").first = color;
            }
		}
		public static void SetHairColor(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.GetChannel("hair").first = color;
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
			if (!((AlienRace.ThingDef_AlienRace)raceDef).alienRace.hairSettings.hasHair)
            {
				return new List<HairDef>();
            }
			else if (((AlienRace.ThingDef_AlienRace)raceDef).alienRace.hairSettings.hairTags == null)
			{
				//no good way to distinguish between alien specific hair and hair suitable for generic humanlikes
				return DefDatabase<HairDef>.AllDefs.ToList();
			}
            else
			{
				List<string> allowedTags = ((AlienRace.ThingDef_AlienRace)raceDef).alienRace.hairSettings.hairTags.ToList();
				return DefDatabase<HairDef>.AllDefs.Where(x => x.hairTags.Intersect(allowedTags).Any()).ToList();
			}
		}

		public static List<ThingDef> GetGrowableRaces(List<ThingDef> excluded)
		{
			return DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where(x => !excluded.Contains(x)).Cast<ThingDef>().ToList();
		}

		public static bool AlienRacesIsActive => ModLister.HasActiveModWithName("Humanoid Alien Races 2.0");
		public static bool IndividualityIsActive => ModLister.HasActiveModWithName("[SYR] Individuality");
		public static bool PsychologyIsActive => ModLister.AllInstalledMods.Where(x => x.Active && (x.PackageId.ToLower() == "community.psychology.unofficialupdate")).Any();
	}

}
