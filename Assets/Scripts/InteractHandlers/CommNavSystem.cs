using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.InteractHandlers {
	public class CommNavSystem : InteractHandler {

		[SerializeField]
		private Transform rotarySwitch;
		[SerializeField]
		private Vector3 rotaryOn;
		private Vector3 rotaryOff;

		private Canvas myCanvas;
		private CommNavPanelDisplay display;

		public bool turnedOn;
		public float freq = 125f;
		public float navFreq = 108f;

		public bool operational = true;
		public bool degradedPower = false;
		public bool degradedFreq = false;

		private void Awake () {

			myCanvas = GetComponent<Canvas> ();
			myCanvas.enabled = turnedOn;

			rotaryOff = rotarySwitch.localEulerAngles;
			SetRotarySwitch (turnedOn ? 1f : 0f);

			display = GetComponent<CommNavPanelDisplay> ();
			display.SetIndicator (0);

			SetTrxFreq (freq);
			SetNavFreq (navFreq);
		}

		public override void OnInteract () {
			base.OnInteract ();

			ChoicesDialog.ClearChoices ();

			if (turnedOn) {
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Turn off" : "Απενεργοποίηση", 
					() => SetRotarySwitch (0f));
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Change COMM frequency..." : "Αλλαγή συχνότητας COMM...", 
					() => SelectFreqDialog (), false);
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Change NAV frequency..." : "Αλλαγή συχνότητας NAV...", 
					() => SelectNavFreqDialog (), false);
			}
			else {
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Turn on" : "Ενεργοποίηση", 
					() => SetRotarySwitch (1f));
			}
			ChoicesDialog.AddChoice (
				ViewControl.langEn ? "Replace unit" : "Αντικατάσταση μηχανήματος",
				() => {
					SetRotarySwitch (0f);
					WaitIndicator.ShowWait (
						ViewControl.langEn ? "Replacing COMM/NAV..." : "Αντικατάσταση COMM/NAV...", 4f,
						() => Repair (true));
					}, 
				color: 2);

			ChoicesDialog.Display ();
		}

		private void SelectFreqDialog () {

			ChoicesDialog.ClearChoices ();

			ChoicesDialog.AddChoice (
				"118.00 MHz", () => {
					SetTrxFreq (118f);
				});
			ChoicesDialog.AddChoice (
				"125.00 MHz", () => {
					SetTrxFreq (125f);
				});
			ChoicesDialog.AddChoice (
				"136.00 MHz", () => {
					SetTrxFreq (136f);
				});
		}

		private void SelectNavFreqDialog () {

			ChoicesDialog.ClearChoices ();

			ChoicesDialog.AddChoice (
				"108.00 MHz", () => {
					SetNavFreq (108f);
				});
			ChoicesDialog.AddChoice (
				"112.00 MHz", () => {
					SetNavFreq (112f);
				});
			ChoicesDialog.AddChoice (
				"114.40 MHz", () => {
					SetNavFreq (114.4f);
				});
			ChoicesDialog.AddChoice (
				"117.95 MHz", () => {
					SetNavFreq (117.95f);
				});
		}

		private void SetTrxFreq (float ff) {

			freq = ff;
			display.SetCommUse (freq);
		}

		private void SetNavFreq (float ff) {

			navFreq = ff;
			display.SetNavUse (navFreq);
		}

		private void SetRotarySwitch (float pos) {

			turnedOn = pos > 0f;
			rotarySwitch.localEulerAngles = Vector3.Lerp (rotaryOff, rotaryOn, pos);
			myCanvas.enabled = turnedOn;
		}

		public void Repair (bool showNotif) {

			operational = true;
			degradedPower = false;
			degradedFreq = false;
			if (showNotif) Notifications.ShowNotif (
				ViewControl.langEn ? "Replaced with a new unit." : 
				"Έγινε αντικατάσταση με λειτουργικό μηχάνημα.", 0);
		}
	}
}