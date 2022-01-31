using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InteractHandlers {
	public class CommNavTestSet : InteractHandler {

		private Canvas myCanvas;

		public GameObject coaxial;
		public Antenna aircraftAntenna;
		public Antenna aircraftVorAntenna;

		private Headset headset;

		public CommNavSystem[] commNavSystems;

		[SerializeField]
		private GameObject[] pages;
		public int curPage = 0;
		private int toPage = -1;
		private float pageChangeTimer;
		private const float pageChangeDelay = .7f;

		[SerializeField]
		private string[] pageTitles = new string[] {
			"COMMS TEST",
			"COMMS TEST",
			"VOR TEST",
			"ILS TEST",
		};

		private readonly string[] pageOptionTitles = new string[] {
			"Μετάβαση σε έλεγχο εκπομπής (COMM TX)",
			"Μετάβαση σε έλεγχο λήψης (COMM RX)",
			"Μετάβαση σε έλεγχο VOR (NAV)",
		};
		private readonly string[] pageOptionTitlesEN = new string[] {
			"Go to Transmission test (COMM TX)",
			"Go to Reception test (COMM RX)",
			"Go to VOR test (NAV)",
		};

		[SerializeField]
		private Text titleLabel;

		[SerializeField]
		private Text txFreqLabel;
		[SerializeField]
		private Text txAutoLabel;
		[SerializeField]
		private Text txRfLvLabel;
		[SerializeField]
		private FillBar txRfLvBar;
		[SerializeField]
		private Text txModLvLabel;
		[SerializeField]
		private FillBar txModLvBar;

		[SerializeField]
		private Text rxFreqLabel;
		[SerializeField]
		private Text rxRfGenLabel;
		[SerializeField]
		private Text rxModGenLabel;

		[SerializeField]
		private Text vorFreqLabel;
		[SerializeField]
		private Text vorGenLabel;
		[SerializeField]
		private Text vorDirLabel;
		[SerializeField]
		private Text vorFlagLabel;
		[SerializeField]
		private Transform vorDirIndicator;

		[SerializeField]
		private Transform acVorNeedleDigital;
		[SerializeField]
		private Transform acVorNeedleAnalog;
		[SerializeField]
		private Transform acVorFlag;

		public const float localVorFreq = 114.4f;
		public const float localVorBearing = 60f;

		private AudioSource auRx;

		public bool turnedOn = false;
		public int connectedTo = 0;

		public float trxFreq = 125f;
		public bool txAuto = true;
		public bool rxRfGen = false;
		public bool rxModGen = false;
		
		public float vorFreq = 108f;
		public bool vorGen = false;
		public float vorBearing = 45f;
		private const int vorBearingStep = 45;
		public bool vorFlag = false;

		private float refreshTimer;
		private const float refreshInterval = .3f;

		private void Start () {

			myCanvas = GetComponent<Canvas> ();
			myCanvas.enabled = turnedOn;

			SetConnected (connectedTo);

			headset = GetComponent<Headset> ();

			titleLabel.text = pageTitles[curPage];

			txRfLvBar.SetFill (0f);
			txModLvBar.SetFill (0f);

			auRx = GetComponent<AudioSource> ();

			SetVorGen (vorGen);
			SetVorBearing (vorBearing);
			SetVorFreq (vorFreq);
			SetVorFlag (vorFlag);
		}

		private void Update () {

			if (toPage != -1) {
				pageChangeTimer -= Time.deltaTime;
				if (pageChangeTimer < 0f) {
					curPage = toPage;
					for (int i = 0; i < pages.Length; i++) {
						pages[i].SetActive (i == curPage);
					}
					titleLabel.text = pageTitles[curPage];
					toPage = -1;
				}
				return;
			}

			var transp = connectedTo > 0 ? commNavSystems[connectedTo - 1] : null;
			bool functioning = transp && transp.operational && transp.turnedOn;

			bool audio = functioning && turnedOn && transp.freq == trxFreq &&
				!headset.talking && headset.connectedTo == connectedTo &&
				curPage == 1 && rxRfGen && rxModGen;

			if (audio != auRx.isPlaying) {
				if (audio) auRx.Play ();
				else auRx.Stop ();
			}

			refreshTimer -= Time.deltaTime;
			if (refreshTimer > 0f) return;

			refreshTimer = refreshInterval;

			if (curPage == 0) {

				functioning = functioning && (txAuto || transp.freq == trxFreq);
				if (functioning && txAuto && headset.talking && transp.freq != trxFreq) {
					SetTrxFreq (transp.freq);
				}

				var rfLv = functioning && headset.talking ?
					18f + .5f * Random.value : 0f;
				txRfLvLabel.text = "RF LEVEL: " +
					(rfLv > 0 ? rfLv.ToString ("#.#") + " W" : "NO SIGNAL");
				txRfLvBar.SetFill (rfLv);

				var modLv = functioning && headset.talking ?
					.4f + .1f * Random.value : 0f;
				txModLvLabel.text = "MOD LEVEL: " + Mathf.RoundToInt (modLv * 100) + "%";
				txModLvBar.SetFill (modLv);
			}
			//else if (curPage == 1) {

			//	functioning = functioning && transp.freq == trxFreq;
			//}

			// VOR
			bool functioningVor = transp && transp.operational && transp.turnedOn &&
				curPage == 2 && transp.navFreq == vorFreq;
			var curBearing = vorBearing;
			bool curFlag = vorFlag;

			if (!functioningVor) {
				foreach (var t in commNavSystems) {
					if (t.operational && t.turnedOn && t.navFreq == localVorFreq) {
						transp = t;
						functioningVor = true;
						curBearing = localVorBearing;
						curFlag = false;
						break;
					}
				}
			}
			
			bool unflagged = functioningVor && !curFlag;
			acVorNeedleDigital.gameObject.SetActive (unflagged);
			acVorFlag.gameObject.SetActive (!unflagged);
			
			if (functioningVor) {
				acVorNeedleAnalog.localEulerAngles = Vector3.left * curBearing;
				acVorNeedleDigital.localEulerAngles = Vector3.left * curBearing;
			}
		}

		private void ChangePage (int toPage) {

			this.toPage = toPage;
			for (int i = 0; i < pages.Length; i++) {
				pages[i].SetActive (false);
			}
			pageChangeTimer = pageChangeDelay;
		}

		public override void OnInteract () {
			base.OnInteract ();

			ChoicesDialog.ClearChoices ();

			ChoicesDialog.AddChoice (
				turnedOn ? 
				(ViewControl.langEn ? "Turn device off" : "Απενεργοποίηση συσκευής") :
				(ViewControl.langEn ? "Turn device on" : "Ενεργοποίηση συσκευής"),
				() => ToggleTurnedOn ());

			if (connectedTo != 0) {
				ChoicesDialog.AddChoice (
					(ViewControl.langEn ? "Disconnect cable from COMM/NAV " : "Αποσύνδεση καλωδίου από COMM/NAV ") + connectedTo,
					() => SetConnected (0));
			}
			else {
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Connect cable to COMM/NAV 1" : "Σύνδεση καλωδίου σε COMM/NAV 1",
					() => SetConnected (1));
				ChoicesDialog.AddChoice (
					ViewControl.langEn ? "Connect cable to COMM/NAV 2" : "Σύνδεση καλωδίου σε COMM/NAV 2",
					() => SetConnected (2));
			}

			if (turnedOn) {
				// COMM TX, COMM RX
				if (curPage < 2) {

					ChoicesDialog.AddChoice (
						ViewControl.langEn ? "Choose frequency..." : "Επιλογή συχνότητας...", 
						() => SelectFreqDialog (), false, color: 3);
				}
				// COMM RX
				if (curPage == 1) {

					ChoicesDialog.AddChoice (
						rxRfGen ?
						(ViewControl.langEn ? "Deactivate audio signal" : "Απενεργοποίηση ακουστικού σήματος") :
						(ViewControl.langEn ? "Activate audio signal" : "Ενεργοποίηση ακουστικού σήματος"),
						() => ToggleRxGens (), color: 3);
				}
				// VOR
				if (curPage == 2) {

					ChoicesDialog.AddChoice (
						vorGen ?
						(ViewControl.langEn ? "Deactivate VOR signal" : "Απενεργοποίηση γεννήτριας VOR") :
						(ViewControl.langEn ? "Activate VOR signal" : "Ενεργοποίηση γεννήτριας VOR"),
						() => SetVorGen (!vorGen), color: 3);
					ChoicesDialog.AddChoice (
						ViewControl.langEn ? "Choose frequency..." : "Επιλογή συχνότητας...", 
						() => SelectNavFreqDialog (), false, color: 3);
					if (vorGen) {
						ChoicesDialog.AddChoice (
							ViewControl.langEn ? "Choose bearing..." : "Επιλογή bearing...", 
							() => SelectVorBearingDialog (), false, color: 3);
					}
					ChoicesDialog.AddChoice (
						(vorFlag ?
						(ViewControl.langEn ? "Deactivate" : "Απενεργοποίηση") :
						(ViewControl.langEn ? "Activate" : "Ενεργοποίηση")) +
						" flag check",
						() => SetVorFlag (!vorFlag), color: 3);
				}
				
				// pages
				for (int i = 0; i < pages.Length; i++) {
					if (i == curPage) continue;
					int ii = i;
					ChoicesDialog.AddChoice (
						ViewControl.langEn ? pageOptionTitlesEN[i] : pageOptionTitles[i],
						() => ChangePage (ii), color: 1);
				}
			}

			ChoicesDialog.Display ();
		}

		private void SelectFreqDialog () {

			ChoicesDialog.ClearChoices ();
			
			ChoicesDialog.AddChoice (
				ViewControl.langEn ? "Auto" : "Αυτόματα", 
				() => SetTxAuto (true));
			ChoicesDialog.AddChoice (
				"118.00 MHz", () => {
					SetTxAuto (false);
					SetTrxFreq (118f, true);
				});
			ChoicesDialog.AddChoice (
				"125.00 MHz", () => {
					SetTxAuto (false);
					SetTrxFreq (125f, true);
				});
			ChoicesDialog.AddChoice (
				"136.00 MHz", () => {
					SetTxAuto (false);
					SetTrxFreq (136f, true);
				});
		}

		private void SelectNavFreqDialog () {

			ChoicesDialog.ClearChoices ();

			ChoicesDialog.AddChoice (
				"108.00 MHz", () => {
					SetVorFreq (108f);
				});
			ChoicesDialog.AddChoice (
				"112.00 MHz", () => {
					SetVorFreq (112f);
				});
			ChoicesDialog.AddChoice (
				"117.95 MHz", () => {
					SetVorFreq (117.95f);
				});
		}

		private void SelectVorBearingDialog () {

			ChoicesDialog.ClearChoices ();

			for (int b = 0; b < 360; b += vorBearingStep) {
				int bb = b;
				ChoicesDialog.AddChoice (
					b + (ViewControl.langEn ? " degrees" : " μοίρες"), () => {
						SetVorBearing (bb);
					});
			}
		}

		private void SetTrxFreq (float freq, bool remindMatch = false) {

			trxFreq = freq;
			var frqText = trxFreq > 0 ? trxFreq.ToString ("0.00") + " MHz" : "---";
			txFreqLabel.text = "TX FREQ: " + frqText;
			rxFreqLabel.text = "RX FREQ: " + frqText;

			if ((!txAuto || curPage == 1) && remindMatch) {

				var transp = connectedTo > 0 ? commNavSystems[connectedTo - 1] : null;
				if (!transp || transp.freq != trxFreq) {
					Notifications.ShowNotif (
						ViewControl.langEn ? 
						"Remember to set same frequency to the aircraft's COMM/NAV." : 
						"Θυμηθείτε να επιλέξετε ίδια συχνότητα και στο COMM/NAV του αεροσκάφους.", 1, 6f);
				}
			}
		}

		private void SetTxAuto (bool state) {

			txAuto = state;
			txAutoLabel.text = txAuto ? "AUTO" : "MANUAL";
		}

		private void ToggleRxGens () {

			SetRxGens (!rxRfGen);
		}

		private void SetRxGens (bool state) {

			SetRxRfGen (state);
			SetModGen (state);
		}

		private void SetRxRfGen (bool state) {

			rxRfGen = state;
			rxRfGenLabel.text = "RF GEN: "  + (state ? "ON" : "OFF");
		}

		private void SetModGen (bool state) {

			rxModGen = state;
			rxModGenLabel.text = "MOD GEN: " + (state ? "ON" : "OFF");
		}

		private void SetVorGen (bool state) {

			vorGen = state;
			vorGenLabel.text = "VOR GEN: " + (state ? "ON" : "OFF");
			vorDirIndicator.gameObject.SetActive (state);
			vorDirLabel.gameObject.SetActive (state);
		}

		private void SetVorFreq (float freq, bool remindMatch = false) {

			vorFreq = freq;
			var frqText = vorFreq > 0 ? vorFreq.ToString ("0.00") + " MHz" : "---";
			vorFreqLabel.text = "FREQ: " + frqText;

			if (remindMatch) {

				var transp = connectedTo > 0 ? commNavSystems[connectedTo - 1] : null;
				if (!transp || transp.navFreq != vorFreq) {
					Notifications.ShowNotif (
						ViewControl.langEn ?
						"Remember to set same frequency to the aircraft's COMM/NAV." :
						"Θυμηθείτε να επιλέξετε ίδια συχνότητα και στο COMM/NAV του αεροσκάφους.", 1, 6f);
				}
			}
		}

		private void SetVorBearing (float bear) {

			vorBearing = bear;
			vorDirLabel.text = "BEARING: " + Mathf.RoundToInt(bear) + " DEG";
			vorDirIndicator.localRotation = Quaternion.Euler (0, 0, -bear);
		}

		private void SetVorFlag (bool state) {

			vorFlag = state;
			vorFlagLabel.text = "FLAG: " + (state ? "*** ON ***" : "OFF");
		}

		private void SetConnected (int toSystem) {

			connectedTo = toSystem;
			coaxial.SetActive (connectedTo > 0);
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