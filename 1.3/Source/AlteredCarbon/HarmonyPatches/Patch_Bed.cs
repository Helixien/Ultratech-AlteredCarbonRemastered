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
}

