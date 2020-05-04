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
		public string ID { get; private set; }
		public bool IsEmpty => string.IsNullOrEmpty(ID);
		public bool IsSelected { get; private set; }
		public float Size { get; private set; }
		

		public void Configure(int x, int y, string id, Vector3 position, float size)
		{
			SetGridIndex(x, y);
			SetPosition(position);
			SetSize(size);
			SetSelected(false);
			SetId(id, true);
		}

		public virtual void Invalidate()
		{
			SetId(null);
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
		
		protected virtual void SetId(string id, bool andEnable=false)
		{
			ID = id;

			if (!string.IsNullOrEmpty(id))
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

		public void EditMode_SetId(string id)
		{
			SetId(id, false);
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
