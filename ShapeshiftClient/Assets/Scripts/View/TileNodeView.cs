//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class TileNodeView : MonoBehaviour
	{
		[SerializeField] private RectTransform _rectTransform = null;
		[SerializeField] private Image _image = null;

		private TileNodeType _nodeType;

		public void SetNodeType(TileNodeType nodeType)
		{
			_nodeType = nodeType;
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
		
		
		private static Dictionary<TileNodeType,Sprite> _spriteCache = new Dictionary<TileNodeType, Sprite>();

		private static Sprite GetSprite(TileNodeType nodeType)
		{
			if (!_spriteCache.TryGetValue(nodeType, out var sprite))
			{
				sprite = Resources.Load<Sprite>($"TileNode-{nodeType}");
				_spriteCache[nodeType] = sprite;
			}

			return sprite;
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
