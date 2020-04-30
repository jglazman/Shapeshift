//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public abstract class GridBaseView : MonoBehaviour
	{
		protected abstract string GetSpriteResourceName();

		[SerializeField] private RectTransform _rectTransform = null;
		[SerializeField] private Image _image = null;
		[SerializeField] private GameObject _rootSelected = null;

		public GridIndex Index { get; private set; }
		public int Type { get; private set; }
		public bool IsSelected { get; private set; }

		public void Configure(int x, int y, int type, Vector3 position, float size)
		{
			Index = new GridIndex() { x = x,  y = y };
			SetType(type);
			SetPosition(position);
			SetSize(size);
			SetSelected(false);
		}
		
		public void SetType(int type, bool andEnable=false)
		{
			Type = type;

			if (type >= 0)
			{
				string spriteName = GetSpriteResourceName();
				_image.sprite = SpriteResource.GetSprite(spriteName);
				
				if (andEnable)	// if we are turned off, then something else has to turn us back on. we can't make that decision.
					gameObject.SetActive(true);
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
		
		public void SetSelected(bool isSelected)
		{
			IsSelected = isSelected;
			
			if (_rootSelected != null)
				_rootSelected.SetActive(isSelected);
		}
	}
}
