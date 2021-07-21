using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public class Command_SetBrainTemplate : Command
    {
        public Map map;
        public List<Thing> brainTemplates;
        public Building_SleeveGrower building;
        public bool allowRemoveActiveBrain = false;
        public Command_SetBrainTemplate(Building_SleeveGrower building, bool removeActiveBrain = false)
        {
            this.building = building;
            this.map = building.Map;
            this.allowRemoveActiveBrain = removeActiveBrain;
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            HashSet<ThingDef> brainTemplates = new HashSet<ThingDef>();
            brainTemplates.AddRange(this.map.listerThings.AllThings.Where(x => x.TryGetComp<CompBrainTemplate>() != null).Select(x => x.def));
            foreach (ThingDef brainTemplate in brainTemplates)
            {
                if (brainTemplate != building.ActiveBrainTemplate?.def)
                {
                    list.Add(new FloatMenuOption(brainTemplate.LabelCap, delegate
                    {
                        this.InsertBrainTemplate(brainTemplate);
                    }, MenuOptionPriority.Default, null, null, 29f, null, null));
                }
            }

            if (allowRemoveActiveBrain && building.ActiveBrainTemplate != null)
            {
                list.Add(new FloatMenuOption("AlteredCarbon.RemoveCurrentBrainTemplate".Translate(), delegate
                {
                    this.RemoveActiveBrainTemplate();
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            if (list.Count == 0)
            {
                list.Add(new FloatMenuOption("None".Translate(), null, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }
        private void InsertBrainTemplate(ThingDef brainTemplate)
        {
            building.activeBrainTemplateToBeProcessed = brainTemplate;
        }

        private void RemoveActiveBrainTemplate()
        {
            building.activeBrainTemplateToBeProcessed = null;
            building.removeActiveBrainTemplate = true;
        }
    }
}