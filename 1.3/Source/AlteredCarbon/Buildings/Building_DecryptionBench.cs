using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AlteredCarbon
{
    public class Building_DecryptionBench : Building_WorkTable
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_Action
            {
                defaultLabel = "AC.HackStack".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/ConvertStack"),
            };
        }
    }
}