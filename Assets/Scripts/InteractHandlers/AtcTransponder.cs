using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.InteractHandlers {
	public class AtcTransponder : InteractHandler {

		public static AtcTransponder Me { get; private set; }

		[SerializeField]
		private Transform rotarySwitch;
		[SerializeField]
		private Vector3 rotaryOnA;
		[SerializeField]
		private Vector3 rotaryOnC;
		private Vector3 rotaryOff;

		[SerializeField]
		private Transform identButton;
		[SerializeField]
		private Vector3 identPosPressed;
		private Vector3 identPosInit;

		[SerializeField]
		private Transform rotary1;
		[SerializeField]
		private Transform rotary10;
		[SerializeField]
		private Transform rotary100;
		[SerializeField]
		private Transform rotary1000;
		private Transform[] codeRotaries;
		private static Vector3 rotPerStep = new Vector3 (-45, 0, 0);

		public int rotSelection = 0;
		public bool ident;

		public int[] codeDigits = new int[] { 1, 2, 3, 4 };
		public int altitude = 0;

		public bool operational = true;
		public bool degradedPower = false;
		public bool degradedFreq = false;

		private void Awake () {

			Me = this;

			rotaryOff = rotarySwitch.localEulerAngles;
			SetRotarySwitch (rotSelection);

			identPosInit = identButton.localPosition;
			SetIdent (ident);

			codeRotaries = new Transform[] { rotary1000, rotary100, rotary10, rotary1 };
			SetCode (GetCode ());
		}

		public override void OnInteract () {
			base.OnInteract ();

			ChoicesDialog.ClearChoices ();

			if (rotSelection != 0) {
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Set to OFF" : "Θέση OFF", 
					() => SetRotarySwitch (0));
			}
			if (rotSelection != 1) {
				ChoicesDialog.AddChoice (
					(ViewControl.langEn ? "Seto to ON (Mode A)" : "Θέση ON (Mode A)"), 
					() => SetRotarySwitch (1));
			}
			if (rotSelection != 2) {
				ChoicesDialog.AddChoice (
					(ViewControl.langEn ? "Seto to ALT (Mode A + C)" : "Θέση ALT (Mode A + C)"), 
					() => SetRotarySwitch (2));
			}

			ChoicesDialog.AddChoice (
				ViewControl.langEn ? "Change code" : "Αλλαγή κωδικού", () => {
				SetCode (Random.Range (1000, 6999));
			});

			if (ident) {
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Deactivate IDENT" : "Απενεργοποίηση IDENT", 
					() => SetIdent (false));
			}
			else {
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Activate IDENT" : "Ενεργοποίηση IDENT", 
					() => SetIdent (true));
			}

			ChoicesDialog.AddChoice (
				ViewControl.langEn ? "Replace transponder" : "Αντικατάσταση μηχανήματος",
				() => {
					SetRotarySwitch (0);
					WaitIndicator.ShowWait (
						ViewControl.langEn ? "Replacing ATC Transponder" : "Αντικατάσταση ATC Transponder...", 4f,
					() => Repair (true));
				}, color: 2);

			ChoicesDialog.Display ();
		}

		private void SetRotarySwitch (int pos) {

			switch (pos) {
				case 1:
					rotarySwitch.localEulerAngles = rotaryOnA;
					break;
				case 2:
					rotarySwitch.localEulerAngles = rotaryOnC;
					break;
				default:
					pos = 0;
					rotarySwitch.localEulerAngles = rotaryOff;
					break;
			}
			rotSelection = pos;
		}

		private void SetIdent (bool state) {

			ident = state;
			identButton.localPosition = ident ? identPosPressed : identPosInit;
		}

		private void SetCode (int code) {

			code = Mathf.Clamp (code, 0, 7777);

			codeDigits[0] = Mathf.FloorToInt (code / 1000);
			codeDigits[1] = Mathf.FloorToInt (code % 1000 / 100);
			codeDigits[2] = Mathf.FloorToInt (code % 100 / 10);
			codeDigits[3] = code % 10;

			for (int i = 0; i < codeDigits.Length; i++) {
				codeRotaries[i].localEulerAngles = codeDigits[i] * rotPerStep;
			}
		}

		public int GetCode () {
			return 1000 * codeDigits[0] + 
				100 * codeDigits[1] + 
				10 * codeDigits[2] + 
				codeDigits[3];
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