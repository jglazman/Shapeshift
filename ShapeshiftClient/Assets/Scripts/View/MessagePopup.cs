//
// Copyright (c) 2020 Jeremy Glazman
//

using TMPro;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class MessagePopup : GenericPopupView
	{
		[SerializeField] private TextMeshProUGUI _messageText = null;

		private void SetMessageText(string message)
		{
			_messageText.text = message;
		}


		public static void ShowMessage(string message)
		{
			var popup = PopupViewController.Open<MessagePopup>();
			popup.SetMessageText(message);
		}
	}
}
