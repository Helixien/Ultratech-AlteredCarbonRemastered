﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class BackstoryDef : Def
	{
		public static BackstoryDef Named(string defName)
		{
			return DefDatabase<BackstoryDef>.GetNamed(defName, true);
		}

		public override void ResolveReferences()
		{
			if (!this.addToDatabase)
			{
				Log.Error("0 Not Adding backstory " + this.saveKeyIdentifier);
				return;
			}

			if (BackstoryDatabase.allBackstories.ContainsKey(this.saveKeyIdentifier))
			{
				Log.Error("1 Not Adding backstory " + this.saveKeyIdentifier);
				return;
			}
			Backstory backstory = new Backstory();

			if (this.forcedTraits?.Any() ?? false)
            {
				backstory.forcedTraits = new List<TraitEntry>();
				foreach (var trait in this.forcedTraits.Where(x => Rand.RangeInclusive(0, 100) < x.chance))
				{
					backstory.forcedTraits.Add(new TraitEntry(trait.defName, trait.degree));
				}
			}

			if (this.disallowedTraits?.Any() ?? false)
            {
				backstory.disallowedTraits = new List<TraitEntry>();
				foreach (var trait in this.disallowedTraits.Where(x => Rand.RangeInclusive(0, 100) < x.chance))
				{
					backstory.disallowedTraits.Add(new TraitEntry(trait.defName, trait.degree));
				}
			}

			backstory.SetTitle(this.title, this.title);
			if (!GenText.NullOrEmpty(this.titleShort))
			{
				backstory.SetTitleShort(this.titleShort, this.titleShort);
			}
			else
			{
				backstory.SetTitleShort(backstory.title, backstory.title);
			}
			if (!GenText.NullOrEmpty(this.baseDescription))
			{
				backstory.baseDesc = this.baseDescription;
			}

			Traverse.Create(backstory).Field("bodyTypeGlobal").SetValue(this.bodyTypeGlobal);
			Traverse.Create(backstory).Field("bodyTypeMale").SetValue(this.bodyTypeMale);
			Traverse.Create(backstory).Field("bodyTypeFemale").SetValue(this.bodyTypeFemale);
			if (skillGains?.Any() ?? false)
            {
				var skillGainsDict = skillGains.ToDictionary(x => x.skill.defName, y => y.minLevel);
				Traverse.Create(backstory).Field("skillGains").SetValue(skillGainsDict);
            }

			backstory.slot = this.slot;
			backstory.shuffleable = this.shuffleable;
			backstory.spawnCategories = this.spawnCategories;

			if (this.workDisables.Any())
			{
				using (List<WorkTags>.Enumerator enumerator2 = this.workDisables.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						WorkTags workTags2 = enumerator2.Current;
						backstory.workDisables |= workTags2;
					}
				}
			}
			else
            {
				backstory.workDisables = WorkTags.None;
            }

			backstory.PostLoad();
			backstory.ResolveReferences();
			backstory.identifier = this.saveKeyIdentifier;

			if (!backstory.ConfigErrors(true).Any())
			{
				BackstoryDatabase.AddBackstory(backstory);
			}
			else
			{
				Log.Error("2 Not Adding backstory " + backstory + " with id " + backstory.identifier);
				foreach (var err in backstory.ConfigErrors(true))
                {
					Log.Error(backstory + " - " + err);
                }
			}
		}

        public string baseDescription;

		public string bodyTypeGlobal = "";

		public string bodyTypeMale = "Male";

		public string bodyTypeFemale = "Female";

		public string title;

		public string titleShort;

		public BackstorySlot slot = BackstorySlot.Childhood;

		public bool shuffleable = true;

		public bool addToDatabase = true;

		public List<WorkTags> workDisables = new List<WorkTags>();

		public List<string> spawnCategories = new List<string>();

		public List<SkillRequirement> skillGains;

		public List<TraitEntryBackstory> forcedTraits = new List<TraitEntryBackstory>();

		public List<TraitEntryBackstory> disallowedTraits = new List<TraitEntryBackstory>();

		public string saveKeyIdentifier;
	}

	public class TraitEntryBackstory
	{
		public TraitDef defName;

		public int degree;

		public int chance;
	}
}
