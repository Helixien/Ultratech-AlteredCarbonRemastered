using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static RimWorld.QuestGen.QuestGen_Pawns;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class Patch_SpawnSetup
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad && __instance.RaceProps.Humanlike && __instance.kindDef.HasModExtension<StackSpawnModExtension>())
            {
                var extension = __instance.kindDef.GetModExtension<StackSpawnModExtension>();
                if (extension.SpawnsWithStack && Rand.Chance((float)extension.ChanceToSpawnWithStack / 100f))
                {
                    BodyPartRecord neckRecord = __instance.def.race.body.AllParts.FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
                    var hediff = HediffMaker.MakeHediff(AC_DefOf.UT_CorticalStack, __instance, neckRecord) as Hediff_CorticalStack;
                    hediff.PersonaData.gender = __instance.gender;
                    hediff.PersonaData.race = __instance.kindDef.race;
                    if (AlteredCarbonManager.Instance.stacksRelationships != null)
                    {
                        hediff.PersonaData.stackGroupID = AlteredCarbonManager.Instance.stacksRelationships.Count + 1;
                    }
                    else
                    {
                        hediff.PersonaData.stackGroupID = 0;
                    }
                    __instance.health.AddHediff(hediff, neckRecord);
                    AlteredCarbonManager.Instance.RegisterPawn(__instance);
                    AlteredCarbonManager.Instance.TryAddRelationships(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(QuestPart_BestowingCeremony))]
    [HarmonyPatch("MakeLord")]
    public static class MakeLord_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(QuestPart_BestowingCeremony __instance, Lord __result)
        {
            if (__instance.bestower.kindDef == PawnKindDefOf.Empire_Royal_Bestower)
            {
                RoyalTitleDef titleAwardedWhenUpdating = __instance.target.royalty.GetTitleAwardedWhenUpdating(__instance.bestower.Faction,
                    __instance.target.royalty.GetFavor(__instance.bestower.Faction));
                if (titleAwardedWhenUpdating.defName == "Baron" || titleAwardedWhenUpdating.defName == "Count")
                {
                    ThingOwner<Thing> innerContainer = __instance.bestower.inventory.innerContainer;
                    innerContainer.TryAdd(ThingMaker.MakeThing(AC_DefOf.UT_EmptyCorticalStack), 1);
                }
            }
        }
    }

    [HarmonyPatch(typeof(LordJob_BestowingCeremony))]
    [HarmonyPatch("FinishCeremony")]
    public static class FinishCeremony_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(LordJob_BestowingCeremony __instance, Pawn pawn)
        {
            if (__instance.bestower.kindDef == PawnKindDefOf.Empire_Royal_Bestower)
            {
                ThingOwner<Thing> innerContainer = __instance.bestower.inventory.innerContainer;
                for (int num = innerContainer.Count - 1; num >= 0; num--)
                {
                    if (innerContainer[num].def == AC_DefOf.UT_EmptyCorticalStack)
                    {
                        innerContainer.TryDrop(innerContainer[num], ThingPlaceMode.Near, out Thing lastResultingThing);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Reward_BestowingCeremony), "StackElements", MethodType.Getter)]
    public static class StackElements_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Reward_BestowingCeremony __instance, ref IEnumerable<GenUI.AnonymousStackElement> __result)
        {
            if (__instance.royalTitle.defName == "Baron" || __instance.royalTitle.defName == "Count")
            {
                var list = __result.ToList();
                var item = QuestPartUtility.GetStandardRewardStackElement(AC_DefOf.UT_EmptyCorticalStack.label.CapitalizeFirst(), AC_DefOf.UT_EmptyCorticalStack.uiIcon, () => AC_DefOf.UT_EmptyCorticalStack.description, delegate
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(AC_DefOf.UT_EmptyCorticalStack));
                });
                list.Insert(1, item);
                __result = list;
            }
        }
    }
}

