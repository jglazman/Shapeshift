using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public static class SpriteResource
	{
		private static Dictionary<string,Sprite> _spriteCache = new Dictionary<string,Sprite>();

		
		public static Sprite GetSprite(string spriteResourceName)
		{
			if (!_spriteCache.TryGetValue(spriteResourceName, out var sprite))
			{
				sprite = Resources.Load<Sprite>(spriteResourceName);
				
				Assert.IsNotNull(sprite, $"[SpriteResource] Missing sprite in Resources: {spriteResourceName}");
				
				_spriteCache[spriteResourceName] = sprite;
			}

			return sprite;
		}
		
		public static void ClearCache()
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
