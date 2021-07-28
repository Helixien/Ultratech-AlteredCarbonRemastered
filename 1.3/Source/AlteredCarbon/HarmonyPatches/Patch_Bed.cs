using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	public static class BedPatches
    {
		static BedPatches()
        {
			MethodInfo method = typeof(BedPatches).GetMethod("Prefix");
			foreach (Type type in GenTypes.AllSubclassesNonAbstract(typeof(Need)))
			{
				MethodInfo method2 = type.GetMethod("NeedInterval");
				try
                {
					HarmonyInit.harmonyInstance.Patch(method2, new HarmonyMethod(method), null, null);

				}
				catch (Exception ex)
				{
				};
			}
		}

		public static bool Prefix(Need __instance, Pawn ___pawn)
        {
			if (___pawn != null && ___pawn.CurrentBed() is Building_SleeveCasket && Rand.Chance(0.8f))
            {
				return false;
            }
			return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
    public static class IsValidBedFor_Patch
    {
        private static bool Prefix(Thing bedThing, Pawn sleeper)
        {
            if (bedThing != null && sleeper != null && !sleeper.IsEmptySleeve() && bedThing is Building_SleeveCasket)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(RestUtility), "Reset")]
    public static class Reset_Patch
    {
        private static void Postfix(ref List<ThingDef> ___bedDefsBestToWorst_RestEffectiveness, ref List<ThingDef> ___bedDefsBestToWorst_Medical)
        {
            ___bedDefsBestToWorst_RestEffectiveness.Remove(AC_DefOf.UT_SleeveCasket);
            ___bedDefsBestToWorst_Medical.Remove(AC_DefOf.UT_SleeveCasket);
        }
    }

}

