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
	public class CompSleeveGrowerPostDraw : ThingComp
	{
		public Graphic glass;
		public Graphic Glass
		{
			get
			{
				if (glass == null)
				{
					glass = GraphicDatabase.Get<Graphic_Single>("Things/Building/Misc/SleeveGrower/SleeveGrowingVatTop", ShaderDatabase.CutoutComplex, this.parent.def.graphicData.drawSize, Color.white);
				}
				return glass;
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			var vector = this.parent.DrawPos + Altitudes.AltIncVect;
			vector.y += 6;
			Glass.Draw(vector, Rot4.North, this.parent);
		}
	}
}
