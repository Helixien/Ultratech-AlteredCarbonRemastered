using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    public class ITab_StackBackupContents : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(432f, 480f);
        private Vector2 scrollPosition;
        public Building_StackStorage Building_StackStorage => this.SelThing as Building_StackStorage;
        public ITab_StackBackupContents()
        {
            this.size = WinSize;
            this.labelKey = "AlteredCarbon.Backup";
        }
        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(5f, 0f, this.size.x, this.size.y - 20f).ContractedBy(10f);
            GUI.BeginGroup(viewRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            float num = 0;
            var backedUpStacks = Building_StackStorage.StoredBackedUpStacks.ToList();
            Widgets.ListSeparator(ref num, viewRect.width, "AlteredCarbon.BackedUpStacksInMatrix".Translate(backedUpStacks.Count()));
            var scrollRect = new Rect(0, num, viewRect.width - 16, viewRect.height);
            var outerRect = scrollRect;
            outerRect.width += 16;
            outerRect.height -= 20;
            scrollRect.height = backedUpStacks.Count() * 28f;
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollRect);

            foreach (var backedUpStack in backedUpStacks)
            {
                DrawThingRow(ref num, viewRect.width, backedUpStack);
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }
        private void DrawThingRow(ref float y, float width, PersonaData personaData)
        {
            Rect rect1 = new Rect(0.0f, y, width, 28f);
            Rect rect2 = new Rect(rect1.width - 38f, y, 24f, 24f);
            TooltipHandler.TipRegion(rect2, "AlteredCarbon.Erase".Translate());
            if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Icons/Erase", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                this.Building_StackStorage.backedUpStacks.Remove(personaData.pawnID);
            }

            Rect installStackRect = rect2;
            installStackRect.x -= 28;

            TooltipHandler.TipRegion(installStackRect, "AlteredCarbon.BackupNow".Translate());
            if (Widgets.ButtonImage(installStackRect, ContentFinder<Texture2D>.Get("UI/Icons/Backup", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                var pawn = AlteredCarbonManager.Instance.PawnsWithStacks.FirstOrDefault(x => x.thingIDNumber == personaData.pawnID);
                if (pawn is null)
                {
                    Messages.Message("AlteredCarbon.CannotBackupNoSuchPawn".Translate(personaData.PawnNameColored), Building_StackStorage, MessageTypeDefOf.CautionInput);
                }
                else if (pawn.MapHeld != Building_StackStorage.MapHeld)
                {
                    Messages.Message("AlteredCarbon.CannotBackupPawnOnAnotherLocation".Translate(personaData.PawnNameColored), Building_StackStorage, MessageTypeDefOf.CautionInput);
                }
                else if (!Building_StackStorage.compPower.PowerOn)
                {
                    Messages.Message("AlteredCarbon.CannotBackupNoPower".Translate(personaData.PawnNameColored), Building_StackStorage, MessageTypeDefOf.CautionInput);
                }
                else if (Building_StackStorage.CanBackup(pawn))
                {
                    Building_StackStorage.Backup(pawn);
                }
            }
            rect1.width -= 54f;
            Rect rect3 = rect1;
            rect3.xMin = rect3.xMax - 40f;
            rect1.width -= 15f;

            if (Mouse.IsOver(rect1))
            {
                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(rect1, TexUI.HighlightTex);
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect rect4 = new Rect(6f, y, rect1.width - 36f, rect1.height);
            var pawnLabel = personaData.PawnNameColored.Truncate(rect4.width);
            Widgets.Label(rect4, pawnLabel);

            var timeRect = new Rect(rect1.xMax - 200, rect1.y, 200, rect1.height);
            Widgets.Label(timeRect, "AlteredCarbon.TimeSinceLastBackup".Translate((Find.TickManager.TicksGame - personaData.lastTimeUpdated).ToStringTicksToPeriod()));
            y += 28;
        }
    }
}
