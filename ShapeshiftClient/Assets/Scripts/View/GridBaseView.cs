//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public abstract class GridBaseView : MonoBehaviour
	{
		[SerializeField] private RectTransform _rectTransform = null;
		[SerializeField] private Image _image = null;

		public uint X { get; private set; }
		public uint Y { get; private set; }
		public int Type { get; private set; }

		public void Configure(uint x, uint y, int type, Vector3 position, float size)
		{
			X = x;
			Y = y;
			SetType(type);
			SetPosition(position);
			SetSize(size);
		}
		
		public void SetType(int type)
		{
			Type = type;

			if (type >= 0)
			{
				string spriteName = GetSpriteResourceName();
				_image.sprite = SpriteResource.GetSprite(spriteName);
				// gameObject.SetActive(true);	// if we are turned off, then something else has to turn us back on. we can't make that decision.
			}
			else
			{
				_image.sprite = null;
				gameObject.SetActive(false);	// our view state is now invalid, so turn off
			}
		}

		public void SetPosition(Vector3 position)
		{
			_rectTransform.localPosition = position;
		}

		public void SetSize(float size)
		{
			_rectTransform.sizeDelta = new Vector2(size, size);
		}

		protected abstract string GetSpriteResourceName();
	}
}
