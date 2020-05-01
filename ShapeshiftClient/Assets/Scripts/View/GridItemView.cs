//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class GridItemView : GridBaseView
	{
		// TODO: this should be data-driven
		private static readonly int MoveActionTweenId = TweenConfig.ConvertGuidToId("Linear");
		
		// TODO: this should be data-driven
		public static int NumItemTypes => 6;

		public int ItemType => this.Type;

		public bool IsBusy => _tween != null && !_tween.IsDone;

		private TweenObject _tween = null;
		
		protected override string GetSpriteResourceName()
		{
			return $"GridItem-{ItemType}";
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


		public void DoCreateAction(int itemType)
		{
			SetType(itemType, true);
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
