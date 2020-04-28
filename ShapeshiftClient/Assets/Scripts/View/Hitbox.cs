//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	// https://answers.unity.com/questions/844524/ugui-how-to-increase-hitzone-click-area-button-rec.html
	public class Hitbox : Graphic
	{
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}
