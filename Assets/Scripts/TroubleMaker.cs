using Assets.Scripts.InteractHandlers;
using Assets.Scripts.UI;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts {
	public class TroubleMaker : MonoBehaviour {

		[SerializeField]
		private Antenna commAntenna;
		[SerializeField]
		private Antenna vorAntenna;
		[SerializeField]
		private Antenna atcAntenna;

		[SerializeField]
		private CommNavSystem[] commNavSystems;
		[SerializeField]
		private AtcTransponder transponder;

		private int curFailure = -1;
		private readonly string[] descriptions = new string[] {
			"Βλάβη στην κεραία COMM",
			"Βλάβη στο NAV/COMM 1",
			"Βλάβη στο NAV/COMM 2",
			"Βλάβη στην κεραία VOR",
			"Βλάβη στην κεραία ATC",
			"Βλάβη στο ATC transponder",
			"Απόκλιση συχνότητας στο ATC transponder",
		};

		private readonly string[] descriptionsEN = new string[] {
			"COMM antenna failure",
			"NAV/COMM 1 failure",
			"NAV/COMM 2 failure",
			"VOR antenna failure",
			"ATC antenna failure",
			"ATC transponder failure",
			"ATC transponder frequency offset",
		};

		public void ClearAllFailures (bool showNotif) {

			commAntenna.Repair (false);
			vorAntenna.Repair (false);
			atcAntenna.Repair (false);
			foreach (var s in commNavSystems) s.Repair (false);
			transponder.Repair (false);
			if (showNotif) Notifications.ShowNotif (
				ViewControl.langEn ? "All failures cleared." : 
				"Απαλείφθηκαν όλες οι βλάβες.", 0);
			curFailure = -1;
		}

		public void InsertFailure () {

			ClearAllFailures (false);

			int min, max;
			switch (UseCaseToggle.useCase) {
				case 0:
					min = 0;
					max = 100;
					break;
				case 1:
					min = 100;
					max = 200;
					break;
				case 2:
					min = 200;
					max = 300;
					break;
				default:
					min = 0;
					max = 300;
					break;
			}
			var rnd = Random.Range (min, max);

			if (rnd < 33) {
				curFailure = 0;
				commAntenna.operational = false;
			}
			else if (rnd < 66) {
				curFailure = 1;
				commNavSystems[0].operational = false;
			}
			else if (rnd < 99) {
				curFailure = 2;
				commNavSystems[1].operational = false;
			}
			else if (rnd < 133) {
				curFailure = 3;
				vorAntenna.operational = false;
			}
			else if (rnd < 166) {
				curFailure = 1;
				commNavSystems[0].operational = false;
			}
			else if (rnd < 199) {
				curFailure = 2;
				commNavSystems[1].operational = false;
			}
			else if (rnd < 133) {
				curFailure = 4;
				atcAntenna.operational = false;
			}
			else if (rnd < 166) {
				curFailure = 5;
				transponder.operational = false;
			}
			else {
				curFailure = 6;
				transponder.degradedFreq = true;
			}
			Notifications.ShowNotif (ViewControl.langEn ? 
				"Random failure inserted." : "Έγινε εισαγωγή τυχαίας βλάβης.");
		}

		public void RevealFailure () {

			if (curFailure < 0) {
				Notifications.ShowNotif (ViewControl.langEn ?
					"No failure inserted." : "Δεν έχει εισαχθεί κάποια βλάβη.");
			}
			else if (IsFailureFixed ()) {
				Notifications.ShowNotif (
					ViewControl.langEn ? 
					("The inserted failure was:\n" + 
					descriptionsEN[curFailure] +
					"\nFailure rectified successfully.") :
					("Η βλάβη που εισήχθηκε ήταν:\n" +
					descriptions[curFailure] +
					"\nΗ βλάβη διορθώθηκε επιτυχώς."), 0, 10);
			}
			else {
				Notifications.ShowNotif (ViewControl.langEn ?
					("The inserted failure is:\n" +
					descriptionsEN[curFailure] +
					"\nFailure not yet rectified.") :
					("Η βλάβη που εισήχθηκε είναι:\n" +
					descriptions[curFailure] +
					"\nΗ βλάβη δεν έχει διορθωθεί ακόμα."), 1, 10);
			}
		}

		public void ShowFailureStatus () {

			if (curFailure < 0) {
				Notifications.ShowNotif (ViewControl.langEn ?
					"No failure inserted." : "Δεν έχει εισαχθεί κάποια βλάβη.");
			}
			else if (IsFailureFixed ()) {
				Notifications.ShowNotif (ViewControl.langEn ?
					"Failure rectified successfully." : "Η βλάβη διορθώθηκε επιτυχώς.", 0);
			}
			else {
				Notifications.ShowNotif (ViewControl.langEn ?
					"Failure not yet rectified." : "Η βλάβη δεν έχει διορθωθεί ακόμα.", 1);
			}
		}

		private bool IsFailureFixed () {

			switch (curFailure) {
				case 0:
					return commAntenna.operational;
				case 1:
					return commNavSystems[0].operational;
				case 2:
					return commNavSystems[1].operational;
				case 3:
					return vorAntenna.operational;
				case 4:
					return atcAntenna.operational;
				case 5:
					return transponder.operational;
				case 6:
					return !transponder.degradedFreq;
				default:
					return true;
			}
		}

		public void ShowDialog () {

			ChoicesDialog.ClearChoices ();

			ChoicesDialog.AddChoice (
				ViewControl.langEn ?
					"Clear all failures" : "Καθαρισμός όλων των βλαβών",
				() => ClearAllFailures (true));
			ChoicesDialog.AddChoice (
				ViewControl.langEn ?
					"Insert random failure" : "Εισαγωγή τυχαίας βλάβης",
				() => InsertFailure ());
			ChoicesDialog.AddChoice (
				ViewControl.langEn ?
					"Did I fix the failure?" : "Έχω διορθώσει την βλάβη;",
				() => ShowFailureStatus ());
			ChoicesDialog.AddChoice (
				ViewControl.langEn ?
					"What is the failure?" : "Ποια είναι η βλάβη;",
				() => RevealFailure ());

			ChoicesDialog.Display ();
		}
	}
}