//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public class UrlButton : GenericButton
	{
		[SerializeField] private string _url = "https://www.refugezero.com";
		
		public override void OnClick()
		{
			if (!string.IsNullOrEmpty(_url))
				Application.OpenURL(_url);
		}
	}
}
