//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	/// <summary>
	/// One-time application setup.
	/// </summary>
	public class Bootstrap : MonoBehaviour
	{
		private int _numFrames = 0;
		
		private void Update()
		{
			// give the application a moment to wake up.
			// this helps normalize the experience across devices (especially Android).
			_numFrames++;
			if (_numFrames == 2)
			{
				Game.Initialize();
			}
		}
	}
}
