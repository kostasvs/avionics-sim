using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts {
	public class Antenna : InteractHandler {

		public bool operational = true;
		public bool degradedPower = false;

		public override void OnInteract () {
			base.OnInteract ();

			ChoicesDialog.ClearChoices ();

			ChoicesDialog.AddChoice (
				ViewControl.langEn ? "Replace antenna" : "Αντικατάσταση κεραίας",
				() => WaitIndicator.ShowWait (
					ViewControl.langEn ?
					"Replacing antenna..." : "Αντικατάσταση κεραίας...", 4f,
				() => Repair(true)), color: 2);

			ChoicesDialog.Display ();
		}

		public void Repair (bool showNotif) {

			operational = true;
			degradedPower = false;
			if (showNotif) Notifications.ShowNotif (
				ViewControl.langEn ? "Replaced with a new unit." : 
				"Έγινε αντικατάσταση με λειτουργική κεραία.", 0);
		}
	}
}