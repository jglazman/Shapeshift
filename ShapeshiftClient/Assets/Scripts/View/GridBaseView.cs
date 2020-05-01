//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public abstract class GridBaseView : MonoBehaviour, IPooledObject
	{
		protected abstract string GetSpriteResourceName();

		public void OnReturnToPool()
		{
			Invalidate();
		}

		public GameObject GetGameObject()
		{
			return gameObject;
		}

		[SerializeField] private RectTransform _rectTransform = null;
		[SerializeField] private Image _image = null;
		[SerializeField] private GameObject _rootSelected = null;

		private GridIndex _gridIndex;
		
		public GridIndex Index => _gridIndex;
		public int Type { get; private set; }
		public bool IsSelected { get; private set; }
		public float Size { get; private set; }

		public void Configure(int x, int y, int type, Vector3 position, float size)
		{
			SetGridIndex(x, y);
			SetPosition(position);
			SetSize(size);
			SetSelected(false);
			SetType(type, true);
		}

		public virtual void Invalidate()
		{
			SetType(-1);
			SetGridIndex(-1, -1);
		}

		public void SetGridIndex(GridIndex index)
		{
			SetGridIndex(index.x, index.y);
		}
		
		public void SetGridIndex(int x, int y)
		{
			_gridIndex.x = x;
			_gridIndex.y = y;
		}
		
		protected void SetType(int type, bool andEnable=false)
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

		public void EditMode_SetType(int type)
		{
			SetType(type, false);
		}

		protected void SetPosition(Vector3 position)
		{
			_rectTransform.localPosition = position;
		}

		protected void SetSize(float size)
		{
			Size = size;
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
