using RimWorld;
using System.Xml;
using Verse;

namespace AlteredCarbon
{
	public class SkillOffsets : IExposable
	{
		public SkillDef skill;

		public int offset;
		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
			offset = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
		}

        public void ExposeData()
        {
			Scribe_Defs.Look(ref skill, "skill");
			Scribe_Values.Look(ref offset, "offset", 0);
        }
	}
}
