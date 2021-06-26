using System;
using System.Collections;
using System.Collections.Generic;
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
			backstory.slot = this.slot;
			backstory.shuffleable = this.shuffleable;
			if (GenList.NullOrEmpty<string>(this.spawnCategories))
			{
				return;
			}
			backstory.spawnCategories = this.spawnCategories;
			if (this.workDisables.Count > 0)
			{
				using (List<WorkTags>.Enumerator enumerator2 = this.workDisables.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						WorkTags workTags2 = enumerator2.Current;
						backstory.workDisables |= workTags2;
					}
					goto IL_1E9;
				}
			}
			backstory.workDisables = 0;
		IL_1E9:
			backstory.ResolveReferences();
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

		public List<WorkTags> workAllows = new List<WorkTags>();

		public List<WorkTags> workDisables = new List<WorkTags>();

		public List<string> spawnCategories = new List<string>();

		public List<TraitEntry> forcedTraits = new List<TraitEntry>();

		public List<TraitEntry> disallowedTraits = new List<TraitEntry>();

		public string saveKeyIdentifier;
	}
}
