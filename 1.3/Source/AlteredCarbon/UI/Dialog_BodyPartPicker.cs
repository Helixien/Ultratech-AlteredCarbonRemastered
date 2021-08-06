﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class HediffStage
    {
        public HediffDef hediffDef;
        public int stageInd;
    }
    public class Dialog_BodyPartPicker : Window
    {
        private Pawn pawn;

        private CustomizeSleeveWindow parent;

        public override Vector2 InitialSize
        {
            get
            {
                var size = parent.InitialSize;
                size.Scale(new Vector2(0.8f, 0.8f));
                return size;
            }
        }

        private bool CanBeAppliedTo(RecipeDef recipeDef, Pawn pawn)
        {
            if (recipeDef.AllRecipeUsers != null)
            {
                if (!recipeDef.AllRecipeUsers.Contains(pawn.def))
                {
                    return false;
                }
            }
            if (pawn.def.AllRecipes != null)
            {
                if (!pawn.def.AllRecipes.Contains(recipeDef))
                {
                    return false;
                }
            }

            if (ModCompatibility.RimJobWorldIsActive && !ModCompatibility.RJWAllowsThisFor(recipeDef.addsHediff, pawn))
            {
                return false;
            }

            return true;
        }
        public Dialog_BodyPartPicker(Pawn pawn, CustomizeSleeveWindow parent)
        {
            this.pawn = pawn;
            this.parent = parent;
            var bodyParts = pawn.RaceProps.body.AllParts;
            foreach (var part in bodyParts)
            {
                var hediffsWithStages = new List<HediffStage>();
                foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
                {
                    if (recipe.addsHediff != null && recipe.appliedOnFixedBodyParts.Contains(part.def) && !hediffsWithStages.Any(x => x.hediffDef == recipe.addsHediff)
                        && typeof(Hediff_Implant).IsAssignableFrom(recipe.addsHediff.hediffClass) == false && CanBeAppliedTo(recipe, pawn))
                    {
                        if (recipe.addsHediff.stages != null)
                        {
                            for (var i = 0; i < recipe.addsHediff.stages.Count; i++)
                            {
                                hediffsWithStages.Add(new HediffStage { hediffDef = recipe.addsHediff, stageInd = i });
                            }
                        }
                        else
                        {
                            hediffsWithStages.Add(new HediffStage { hediffDef = recipe.addsHediff, stageInd = 0 });
                        }
                    }
                }
                if (hediffsWithStages.Any())
                {
                    hediffsForParts[part] = hediffsWithStages;
                    partIndex[part] = Rand.RangeInclusive(0, hediffsWithStages.Count - 1);
                }
            }
        }

        public Dictionary<BodyPartRecord, List<HediffStage>> hediffsForParts = new Dictionary<BodyPartRecord, List<HediffStage>>();
        public Dictionary<BodyPartRecord, int> partIndex = new Dictionary<BodyPartRecord, int>();

        private string GetLabel(HediffDef hediffDef, int stageIndex)
        {
            var label = hediffDef.LabelCap;
            if (hediffDef.stages != null)
            {
                var stage = hediffDef.stages[stageIndex];
                if (stage.label != null)
                {
                    label += " (" + stage.label.ToLower() + ")";
                }
            }
            return label;
        }
        public override void DoWindowContents(Rect inRect)
        {
            var areaInstallBodyParts = inRect.ContractedBy(15f);
            var spaceBetweenButtons = 40;
            if (hediffsForParts.Any())
            {
                float listHeight = (hediffsForParts.Count() * spaceBetweenButtons);
                Rect scrollRect = new Rect(areaInstallBodyParts.x, areaInstallBodyParts.y, areaInstallBodyParts.width - 43f, listHeight);

                Widgets.BeginScrollView(areaInstallBodyParts, ref scrollVector, scrollRect);
                GUI.BeginGroup(scrollRect);
                Vector2 pos = new Vector2(0, 0);
                foreach (var data in hediffsForParts)
                {
                    var bodyPartLabel = new Rect(pos.x, pos.y, 100, CustomizeSleeveWindow.buttonHeight);
                    Widgets.Label(bodyPartLabel, data.Key.LabelCap + ": ");
                    var btnBodyPartChangeArrowLeft = new Rect(bodyPartLabel.xMax + 5, pos.y, CustomizeSleeveWindow.buttonHeight, CustomizeSleeveWindow.buttonHeight);
                    var btnBodyPartChangeSelection = new Rect(btnBodyPartChangeArrowLeft.xMax + 2, pos.y, 200, CustomizeSleeveWindow.buttonHeight);
                    var btnBodyPartChangeArrowRight = new Rect(btnBodyPartChangeSelection.xMax + 2, pos.y, CustomizeSleeveWindow.buttonHeight, CustomizeSleeveWindow.buttonHeight);
                    var outline = new Rect(btnBodyPartChangeArrowLeft.x, pos.y, btnBodyPartChangeArrowLeft.width + btnBodyPartChangeSelection.width + btnBodyPartChangeArrowRight.width, CustomizeSleeveWindow.buttonHeight);
                    Widgets.DrawHighlight(outline);

                    if (CustomizeSleeveWindow.ButtonTextSubtleCentered(btnBodyPartChangeArrowLeft, "<"))
                    {
                        if (partIndex[data.Key] == 0)
                        {
                            partIndex[data.Key] = data.Value.Count() - 1;
                        }
                        else
                        {
                            partIndex[data.Key]--;
                        }
                    }

                    var curInd = partIndex[data.Key];
                    var hediffStage = data.Value[curInd];
                    var hediffDef = hediffStage.hediffDef;

                    if (CustomizeSleeveWindow.ButtonTextSubtleCentered(btnBodyPartChangeSelection, GetLabel(hediffStage.hediffDef, hediffStage.stageInd)))
                    {
                        FloatMenuUtility.MakeMenu<HediffStage>(data.Value, x => GetLabel(x.hediffDef, x.stageInd), (HediffStage stage) => delegate
                        {
                            partIndex[data.Key] = data.Value.IndexOf(stage);
                        });
                    }
                    if (CustomizeSleeveWindow.ButtonTextSubtleCentered(btnBodyPartChangeArrowRight, ">"))
                    {
                        if (partIndex[data.Key] == data.Value.Count() - 1)
                        {
                            partIndex[data.Key] = 0;
                        }
                        else
                        {
                            partIndex[data.Key]++;
                        }
                    }

                    pos.y += spaceBetweenButtons;
                }
                GUI.EndGroup();
                Widgets.EndScrollView();
            }

            var btnAccept = new Rect(InitialSize.x * .5f - CustomizeSleeveWindow.buttonWidth / 2 - CustomizeSleeveWindow.buttonOffsetFromButton / 2 - CustomizeSleeveWindow.buttonWidth / 2, 
                InitialSize.y - CustomizeSleeveWindow.buttonHeight - 38, CustomizeSleeveWindow.buttonWidth, CustomizeSleeveWindow.buttonHeight);
            var btnCancel = new Rect(InitialSize.x * .5f + CustomizeSleeveWindow.buttonWidth / 2 + CustomizeSleeveWindow.buttonOffsetFromButton / 2 - CustomizeSleeveWindow.buttonWidth / 2, 
                InitialSize.y - CustomizeSleeveWindow.buttonHeight - 38, CustomizeSleeveWindow.buttonWidth, CustomizeSleeveWindow.buttonHeight);
            if (Widgets.ButtonText(btnAccept, "Accept".Translate().CapitalizeFirst()))
            {
                foreach (var data in hediffsForParts)
                {
                    var curInd = partIndex[data.Key];
                    var hediffStage = data.Value[curInd];
                    var hediffDef = hediffStage.hediffDef;

                    var hediff = ModCompatibility.RimJobWorldIsActive ? rjw.SexPartAdder.MakePart(hediffDef, pawn, data.Key) : HediffMaker.MakeHediff(hediffDef, pawn, data.Key);
                    var stages = hediffDef.stages;
                    if (stages != null)
                    {
                        var stage = stages[hediffStage.stageInd];
                        hediff.Severity = stage.minSeverity;
                    }
                    else
                    {
                        hediff.Severity = 0;
                    }
                    pawn.health.AddHediff(hediff);
                }
                this.Close();
            }
            if (Widgets.ButtonText(btnCancel, "AlteredCarbon.Cancel".Translate().CapitalizeFirst()))
            {
                this.Close();
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private Vector2 scrollVector;
    }
}
