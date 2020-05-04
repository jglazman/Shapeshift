//
// Copyright (c) 2020 Jeremy Glazman
//

using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class GridItemView : GridBaseView
	{
		// TODO: this should be data-driven
		private static readonly int MoveActionTweenId = TweenConfig.ConvertGuidToId("EaseIn");
		
		[SerializeField] private TextMeshProUGUI _tileText = null;

		private TweenObject _tween = null;

		public bool IsBusy => _tween != null && !_tween.IsDone;
		
		public GridItemConfig GridItemConfig => GameConfig.GetGridItem(ID);
		
		protected override string GetSpriteResourceName()
		{
			return GridItemConfig.ImageName;
		}

		public override void Invalidate()
		{
			if (_tween != null)
			{
				_tween.Cancel();
				_tween = null;
			}

			base.Invalidate();
		}

		protected override void SetId(string id, bool andEnable = false)
		{
			base.SetId(id, andEnable);

			// HACK: show letters dynamically so we don't have to generate all 26 tiles
			if (!string.IsNullOrEmpty(id) && GridItemConfig.MatchIndex >= 65)
			{
				_tileText.text = $"{(char)GridItemConfig.MatchIndex}";
				_tileText.gameObject.SetActive(true);
			}
			else
			{
				_tileText.gameObject.SetActive(false);
				_tileText.text = null;
			}
		}


		public void DoCreateAction(string itemId, float speed)
		{
			SetId(itemId, true);

			// TODO: FX when the item is created. for now just spawn it above the top of the board.
			var endPos = transform.localPosition;
			var startPos = endPos;
			startPos.y += Size;
			transform.localPosition = startPos;

			DoMoveAction(endPos, speed);
		}

		public void DoMoveAction(Vector3 destination, float speed)
		{
			if (IsBusy)
				_tween.Cancel();

			if (speed <= 0f)
			{
				transform.localPosition = destination;
			}
			else
			{
				var startPos = transform.localPosition;
				float duration = Vector3.Distance(startPos, destination) / speed;
				
				_tween = new TweenObject(MoveActionTweenId, duration, transform, startPos, destination, true);
				Tween.Run(_tween);
			}
		}
	}
}
