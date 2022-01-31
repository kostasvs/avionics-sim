using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.InteractHandlers {
	public class DebugHandler : InteractHandler {

		public override void OnInteract () {
			base.OnInteract ();
			
			//Debug.Log ("interact invoked", gameObject);
			
			ChoicesDialog.ClearChoices ();
			ChoicesDialog.AddChoice ("Choice 1", () => Notifications.ShowNotif ("choice 1"));
			ChoicesDialog.AddChoice ("Choice 2", () => Notifications.ShowNotif ("choice 2", 0));
			ChoicesDialog.AddChoice ("Choice 3", () => Notifications.ShowNotif ("choice 3", 1, 1));
			ChoicesDialog.Display ();
		}
	}
}