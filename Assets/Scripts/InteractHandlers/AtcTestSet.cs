using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InteractHandlers {
	public class AtcTestSet : InteractHandler {

		private Canvas myCanvas;

		public GameObject coaxial;
		public Antenna antenna;
		public Antenna aircraftAntenna;

		public bool turnedOn = false;
		public bool directConnect = true;

		[SerializeField]
		private Color annunciatorOff = Color.black;
		private Color annunciatorOn = Color.black;

		[SerializeField]
		private Image annModeA;
		[SerializeField]
		private Text labelModeA;
		private const string codeText = "CODE: ";
		private const string codeTextNull = "----";

		[SerializeField]
		private Image annModeC;
		[SerializeField]
		private Text labelModeC;
		private const string altText = "ALT: ";
		private const string altTextNull = "----";

		[SerializeField]
		private Image annFreq;
		[SerializeField]
		private Image annPower;
		[SerializeField]
		private Image annIdent;

		private float refreshTimer;
		private const float refreshInterval = .5f;

		private void Start () {

			myCanvas = GetComponent<Canvas> ();
			myCanvas.enabled = turnedOn;

			annunciatorOn = annModeA.color;

			SetDirectConnect (directConnect);
		}

		private void Update () {

			refreshTimer -= Time.deltaTime;
			if (refreshTimer > 0f) return;

			refreshTimer = refreshInterval;

			var curAntenna = directConnect ? antenna : aircraftAntenna;
			var transp = AtcTransponder.Me;
			bool atcFunctioning = curAntenna.operational && transp.operational &&
				transp.rotSelection != 0;
			bool atcModeC = transp.rotSelection == 2;
			bool atcPowerOk = (directConnect || !curAntenna.degradedPower) &&
				!transp.degradedPower;
			bool atcFreqOk = !transp.degradedFreq;
			bool atcIdent = transp.ident;

			annModeA.color = atcFunctioning ? annunciatorOn : annunciatorOff;
			annModeC.color = atcFunctioning && atcModeC ? annunciatorOn : annunciatorOff;
			annFreq.color = atcFunctioning && atcFreqOk ? annunciatorOn : annunciatorOff;
			annPower.color = atcFunctioning && atcPowerOk ? annunciatorOn : annunciatorOff;
			annIdent.color = atcFunctioning && atcIdent ? annunciatorOn : annunciatorOff;

			labelModeA.text = codeText +
				(atcFunctioning ? transp.GetCode ().ToString () : codeTextNull);
			
			labelModeC.text = altText +
				(atcFunctioning && atcModeC ? 
				transp.altitude.ToString ("D4") : altTextNull);
		}

		public override void OnInteract () {
			base.OnInteract ();

			ChoicesDialog.ClearChoices ();

			ChoicesDialog.AddChoice (
				turnedOn ? 
				(ViewControl.langEn ? "Turn device off" : "Απενεργοποίηση συσκευής") :
				(ViewControl.langEn ? "Turn device on" : "Ενεργοποίηση συσκευής"),
				() => ToggleTurnedOn ());

			ChoicesDialog.AddChoice (
				directConnect ?
				(ViewControl.langEn ? "Disconnect cable" : "Αποσύνδεση καλωδίου") :
				(ViewControl.langEn ? "Connect cable" : "Σύνδεση καλωδίου"),
				() => ToggleDirectConnect ());
			
			ChoicesDialog.Display ();
		}

		private void ToggleDirectConnect () {

			SetDirectConnect (!directConnect);
		}

		private void SetDirectConnect (bool state) {

			directConnect = state;
			coaxial.SetActive (directConnect);
			antenna.gameObject.SetActive (!directConnect);
			aircraftAntenna.gameObject.SetActive (!directConnect);
		}

		private void ToggleTurnedOn () {

			SetTurnedOn (!turnedOn);
		}

		private void SetTurnedOn (bool state) {

			turnedOn = state;
			myCanvas.enabled = turnedOn;
		}
	}
}