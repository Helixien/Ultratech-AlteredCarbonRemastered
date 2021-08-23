using System;
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
			base.ResolveReferences();
			if (!this.addToDatabase)
			{
				return;
			}
			if (BackstoryDatabase.allBackstories.ContainsKey(this.saveKeyIdentifier))
			{
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

			if (GenText.NullOrEmpty(this.title))
			{
				return;
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
			else
			{
				backstory.baseDesc = "Empty.";
			}
			Traverse.Create(backstory).Field("bodyTypeGlobal").SetValue(this.bodyTypeGlobal);
			Traverse.Create(backstory).Field("bodyTypeMale").SetValue(this.bodyTypeMale);
			Traverse.Create(backstory).Field("bodyTypeFemale").SetValue(this.bodyTypeFemale);
			if (skillGains?.Any() ?? false)
            {
				Traverse.Create(backstory).Field("skillGains").SetValue(skillGains.ToDictionary(x => x.skill.defName, y => y.minLevel));
            }
			backstory.slot = this.slot;
			backstory.shuffleable = this.shuffleable;
			if (GenList.NullOrEmpty<string>(this.spawnCategories))
			{
				return;
			}
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
			backstory.identifier = this.saveKeyIdentifier;
			bool flag = false;
			foreach (string text in backstory.ConfigErrors(false))
			{
				if (!flag)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				BackstoryDatabase.AddBackstory(backstory);
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
