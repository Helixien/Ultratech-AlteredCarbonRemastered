using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    public class ITab_StackStorageContents : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(432f, 480f);
        private Vector2 scrollPosition;
        public Building_StackStorage Building_StackStorage => this.SelThing as Building_StackStorage;
        public ITab_StackStorageContents()
        {
            this.size = WinSize;
            this.labelKey = "AC.StackStorage";
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0.0f, 20f, this.size.x, this.size.y - 20f).ContractedBy(10f);
            Rect position = new Rect(rect.x, rect.y, rect.width, rect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0.0f, 0.0f, position.width, position.height);
            Rect viewRect = outRect;
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

            float labelWidth = viewRect.width - 26f;
            float num = 0.0f;

            DoAllowOption(ref num, viewRect, labelWidth, "AC.AllowColonistStacks", ref Building_StackStorage.allowColonistCorticalStacks);
            DoAllowOption(ref num, viewRect, labelWidth, "AC.AllowStrangerStacks", ref Building_StackStorage.allowStrangerCorticalStacks);
            DoAllowOption(ref num, viewRect, labelWidth, "AC.AllowHostileStacks", ref Building_StackStorage.allowHostileCorticalStacks);

            var storedStacks = Building_StackStorage.StoredStacks.ToList();
            Widgets.ListSeparator(ref num, viewRect.width, "AC.CorticalStacksInMatrix".Translate(storedStacks.Count(), Building_StackStorage.MaxFilledStackCapacity));
            foreach (var corticalStack in storedStacks)
            {
                bool showDuplicateStatus = storedStacks.Where(x => x.PersonaData.stackGroupID == corticalStack.PersonaData.stackGroupID).Count() >= 2;
                DrawThingRow(ref num, viewRect.width, corticalStack, showDuplicateStatus);
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoAllowOption(ref float num, Rect viewRect, float labelWidth, string optionKey, ref bool option)
        {
            Rect labelRect = new Rect(0f, num, viewRect.width, 24);
            Widgets.DrawHighlightIfMouseover(labelRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            labelRect.width = labelWidth - labelRect.xMin;
            labelRect.yMax += 5f;
            labelRect.yMin -= 5f;
            Widgets.Label(labelRect, optionKey.Translate().Truncate(labelRect.width));
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Checkbox(new Vector2(labelWidth, num), ref option, 24, disabled: false, paintable: true);
            num += 24f;
        }
        private void DrawThingRow(ref float y, float width, CorticalStack corticalStack, bool showDuplicateStatus)
        {
            Rect rect1 = new Rect(0.0f, y, width, 28f);
            Widgets.InfoCardButton(rect1.width - 24f, y, corticalStack);
            rect1.width -= 24f;

            Rect rect2 = new Rect(rect1.width - 28f, y, 24f, 24f);
            TooltipHandler.TipRegion(rect2, "DropThing".Translate());
            if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                this.Building_StackStorage.innerContainer.TryDrop(corticalStack, ThingPlaceMode.Near, out var droppedThing);
            }
            rect1.width -= 24f;
            Rect rect3 = rect1;
            rect3.xMin = rect3.xMax - 40f;
            rect1.width -= 15f;

            if (Mouse.IsOver(rect1))
            {
                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(rect1, TexUI.HighlightTex);
            }

            Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), corticalStack, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect rect4 = new Rect(36f, y, rect1.width - 36f, rect1.height);
            var pawnLabel = corticalStack.PersonaData.PawnNameColored.Truncate(rect4.width);
            if (showDuplicateStatus)
            {
                pawnLabel += " (" + (corticalStack.PersonaData.isCopied ? "AC.Copy".Translate() : "AC.Original".Translate()) + ")";
            }
            Widgets.Label(rect4, pawnLabel);
            string str2 = corticalStack.DescriptionDetailed;
            TooltipHandler.TipRegion(rect1, str2);
            y += 28f;
        }
    }
}
