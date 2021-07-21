using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_SleeveBodyStats : Hediff
    {
        public List<SkillOffsets> skillsOffsets;

        public List<SkillOffsets> skillPassionsOffsets;
        public void ApplyEffects()
        {
            List<SkillOffsets> negativeSkillsOffset = new List<SkillOffsets>();
            if (this.skillsOffsets != null)
            {
                foreach (var skillOffset in this.skillsOffsets)
                {
                    var curLevel = pawn.skills.GetSkill(skillOffset.skill).Level + skillOffset.offset;
                    if (curLevel > 20) curLevel = 20;

                    var negativeSkillOffset = new SkillOffsets
                    {
                        skill = skillOffset.skill,
                        offset = pawn.skills.GetSkill(skillOffset.skill).Level - curLevel
                    };
                    negativeSkillsOffset.Add(negativeSkillOffset);
                    pawn.skills.GetSkill(skillOffset.skill).Level = curLevel;
                }
            }

            List<SkillOffsets> negativeSkillsPassionOffset = new List<SkillOffsets>();
            if (this.skillPassionsOffsets != null)
            {
                foreach (var skillPassionOffset in this.skillPassionsOffsets)
                {
                    var skill = pawn.skills.GetSkill(skillPassionOffset.skill);
                    var finalValue = (int)skill.passion + skillPassionOffset.offset;

                    var negativeSkillOffset = new SkillOffsets
                    {
                        skill = skillPassionOffset.skill,
                        offset = (int)skill.passion - skillPassionOffset.offset
                    };
                    negativeSkillsPassionOffset.Add(negativeSkillOffset);
                    if (finalValue <= 2)
                    {
                        switch (finalValue)
                        {
                            case 0:
                                skill.passion = Passion.None;
                                break;
                            case 1:
                                skill.passion = Passion.Minor;
                                break;
                            case 2:
                                skill.passion = Passion.Major;
                                break;
                            default:
                                skill.passion = Passion.None;
                                break;
                        }
                    }
                    else
                    {
                        skill.passion = Passion.Major;
                    }
                }
            }

            var corticalHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AlteredCarbonDefOf.AC_CorticalStack) as Hediff_CorticalStack;
            if (corticalHediff != null)
            {
                corticalHediff.negativeSkillsOffsets = negativeSkillsOffset;
                corticalHediff.negativeSkillPassionsOffsets = negativeSkillsPassionOffset;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref skillsOffsets, "skillsOffsets", LookMode.Deep);
            Scribe_Collections.Look(ref skillPassionsOffsets, "skillPassionsOffsets", LookMode.Deep);
        }
    }
}

