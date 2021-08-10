using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    class AlteredCarbonSettings : ModSettings
    {
        public int baseGrowingTimeDuration = 900000;
        public int baseBeautyLevel = 105000;
        public int baseQualityLevel = 210000;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseGrowingTimeDuration, "baseGrowingTimeDuration", 900000);
            Scribe_Values.Look(ref baseBeautyLevel, "baseBeautyLevel", 105000);
            Scribe_Values.Look(ref baseQualityLevel, "baseQualityLevel", 210000);
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            rect.y += 5f;
            Rect rect2 = rect.RightPart(.70f).Rounded();
            Widgets.Label(rect, "AlteredCarbon.growingTimeDuration".Translate());
            baseGrowingTimeDuration = (int)Widgets.HorizontalSlider(rect2, baseGrowingTimeDuration, 1000, 9000000, true, baseGrowingTimeDuration.ToStringTicksToPeriod());
            listingStandard.Gap(listingStandard.verticalSpacing);

            rect.y += 5f + Text.LineHeight;
            rect2 = rect.RightPart(.70f).Rounded();
            Widgets.Label(rect, "AlteredCarbon.baseBeautyLevel".Translate());
            baseBeautyLevel = (int)Widgets.HorizontalSlider(rect2, baseBeautyLevel, 1000, 9000000, true, baseBeautyLevel.ToStringTicksToPeriod());
            listingStandard.Gap(listingStandard.verticalSpacing);

            rect.y += 5f + Text.LineHeight;
            rect2 = rect.RightPart(.70f).Rounded();
            Widgets.Label(rect, "AlteredCarbon.baseQualityLevel".Translate());
            baseQualityLevel = (int)Widgets.HorizontalSlider(rect2, baseQualityLevel, 1000, 9000000, true, baseQualityLevel.ToStringTicksToPeriod());
            listingStandard.Gap(listingStandard.verticalSpacing);
            listingStandard.Gap(70);
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                baseGrowingTimeDuration = 900000;
                baseBeautyLevel = 105000;
                baseQualityLevel = 210000;
            }
            listingStandard.End();
        }

    }
}
