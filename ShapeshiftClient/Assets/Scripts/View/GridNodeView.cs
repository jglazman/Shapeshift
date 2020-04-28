//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class GridNodeView : MonoBehaviour
	{
		[SerializeField] private RectTransform _rectTransform = null;
		[SerializeField] private Image _image = null;

		public uint X { get; private set; }
		public uint Y { get; private set; }
		public GridNodeType NodeType { get; private set; }

		public void Configure(uint x, uint y, GridNodeType nodeType, Vector3 position, float size)
		{
			X = x;
			Y = y;
			SetNodeType(nodeType);
			SetPosition(position);
			SetSize(size);
		}
		
		public void SetNodeType(GridNodeType nodeType)
		{
			NodeType = nodeType;
			
			_image.sprite = GetSprite(nodeType);
		}

		public void SetPosition(Vector3 position)
		{
			_rectTransform.localPosition = position;
		}

		public void SetSize(float size)
		{
			_rectTransform.sizeDelta = new Vector2(size, size);
		}
		
		
		private static Dictionary<GridNodeType,Sprite> _spriteCache = new Dictionary<GridNodeType, Sprite>();

		private static Sprite GetSprite(GridNodeType nodeType)
		{
			if (!_spriteCache.TryGetValue(nodeType, out var sprite))
			{
				// load sprites via naming convention, so they can be dynamically created and used by designers
				string spriteName = GetGridNodeSpriteResourceName(nodeType);
				sprite = Resources.Load<Sprite>(spriteName);
				Assert.IsNotNull(sprite, $"[GridNodeView] Missing sprite in Resources: {spriteName}");
				
				_spriteCache[nodeType] = sprite;
			}

			return sprite;
		}

		private static string GetGridNodeSpriteResourceName(GridNodeType nodeType)
		{
			return $"GridNode-{nodeType}";
		}

		public static void ClearSpriteCache()
		{
			foreach (var kvp in _spriteCache)
			{
				var sprite = kvp.Value;
				if (sprite != null)
					Resources.UnloadAsset(sprite);
			}
			
			_spriteCache.Clear();
		}
	}
}
