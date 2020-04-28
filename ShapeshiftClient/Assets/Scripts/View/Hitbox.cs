//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class Hitbox : Graphic
	{
		[SerializeField] private GameObject _forwardHitsToObject = null;

		public GameObject Target => _forwardHitsToObject != null ? _forwardHitsToObject : gameObject;

		
		// https://answers.unity.com/questions/844524/ugui-how-to-increase-hitzone-click-area-button-rec.html
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}
