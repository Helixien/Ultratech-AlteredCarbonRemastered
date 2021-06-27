using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	public class CompSleeveCasketPostDraw : ThingComp
	{
		public Graphic glass;
		public Graphic Glass
		{
			get
			{
				if (glass == null)
				{
					glass = GraphicDatabase.Get<Graphic_Multi>("Things/Building/Furniture/Bed/SleeveCasketTop",
						ShaderDatabase.CutoutComplex, this.parent.def.graphicData.drawSize, Color.white);
				}
				return glass;
			}
		}
		public override void PostDraw()
		{
			base.PostDraw();
			var vector = this.parent.DrawPos + Altitudes.AltIncVect;
			vector.y += 3;
			Glass.Draw(vector, this.parent.Rotation, this.parent);
		}
	}
}
