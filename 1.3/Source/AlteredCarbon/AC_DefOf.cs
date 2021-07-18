using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AlteredCarbon
{
	[DefOf]
	public static class AC_DefOf
	{
		public static JobDef UT_ExtractStack;

		public static JobDef UT_DuplicateStack;

		public static JobDef UT_InsertBrainTemplate;

		public static JobDef UT_ExtractActiveBrainTemplate;

		public static JobDef UT_StartIncubatingProcess;

		public static JobDef UT_CancelIncubatingProcess;

		public static HediffDef UT_CorticalStack;

		public static HediffDef UT_Sleeve_Quality_Low;

		public static HediffDef UT_Sleeve_Quality_Standart;

		public static HediffDef UT_Sleeve_Quality_High;

		public static HediffDef UT_EmptySleeve;

		public static HediffDef UT_SleeveShock;

		public static HediffDef UT_SleeveBodyData;

		public static ThingDef UT_EmptyCorticalStack;

		public static ThingDef UT_FilledCorticalStack;

		public static ThingDef UT_AncientStack;

		public static ThingDef UT_SleeveIncubator;
		public static ThingDef UT_OrganIncubator;
		public static ThingDef UT_SleeveCasket;
		public static ThingDef UT_CorticalStackStorage;
		public static ThingDef UT_DecryptionBench;

		public static RecipeDef UT_HackBiocodedThings;

		public static RecipeDef UT_WipeFilledCorticalStack;

		public static RecipeDef UT_InstallCorticalStack;

		public static RecipeDef UT_InstallEmptyCorticalStack;

		public static RecipeDef UT_HackFilledCorticalStack;

		public static SpecialThingFilterDef UT_AllowStacksColonist;
		public static SpecialThingFilterDef UT_AllowStacksStranger;
		public static SpecialThingFilterDef UT_AllowStacksHostile;

		public static ThoughtDef UT_WrongGender;

		public static ThoughtDef UT_WrongGenderDouble;

		public static ThoughtDef UT_WrongRace;

		public static ThoughtDef UT_NewSleeve;

		public static ThoughtDef UT_NewSleeveDouble;

		public static ThoughtDef UT_MansBody;

		public static ThoughtDef UT_WomansBody;

		public static ThoughtDef UT_JustCopy;

		public static ThoughtDef UT_LostMySpouse;

		public static ThoughtDef UT_LostMyFiance;

		public static ThoughtDef UT_LostMyLover;

		public static ThoughtDef UT_SomethingIsWrong;

		public static PawnRelationDef UT_Original;

		public static PawnRelationDef UT_Copy;

		public static DesignationDef UT_ExtractStackDesignation;

		public static ConceptDef UT_DeadPawnWithStack;

		public static DutyDef UT_TakeStacks;
	}
}

