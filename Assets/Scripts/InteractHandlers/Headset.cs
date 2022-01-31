using Assets.Scripts.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InteractHandlers {
	public class Headset : MonoBehaviour {

		public static Headset Me { get; private set; }

		private CommNavTestSet testSet;

		[SerializeField]
		private GameObject iconDisconnect;

		[SerializeField]
		private Text numConnect;

		[SerializeField]
		private FadeableDialog[] speechBubbles;

		public int connectedTo = 1;
		public bool talking = false;
		public bool listening = false;

		private void Awake () {

			Me = this;
			testSet = GetComponent<CommNavTestSet> ();
		}

		public void HeadsetDialog () {

			ChoicesDialog.ClearChoices ();

			if (connectedTo != 0) {
				ChoicesDialog.AddChoice (
					(ViewControl.langEn ?
						"Disconnect headset from COMM" : 
						"Αποσύνδεση headset από COMM ") + connectedTo,
					() => SetConnected (0));
				ChoicesDialog.AddChoice (
					ViewControl.langEn ?
						"Voice test" : "Δοκιμή ομιλίας",
					() => StartCoroutine (nameof (Speak)));
			}
			else {
				ChoicesDialog.AddChoice (
					ViewControl.langEn ?
						"Connect headset to COMM 1" : "Σύνδεση headset σε COMM 1",
					() => SetConnected (1));
				ChoicesDialog.AddChoice (
					ViewControl.langEn ?
						"Connect headset to COMM 2" : "Σύνδεση headset σε COMM 2",
					() => SetConnected (2));
			}

			ChoicesDialog.Display ();
		}

		private IEnumerator Speak () {

			var transp = connectedTo > 0 ? testSet.commNavSystems[connectedTo - 1] : null;
			bool works = transp && transp.turnedOn && transp.operational &&
				(testSet.trxFreq == transp.freq || (testSet.curPage == 0 && testSet.txAuto));

			ShowBubble (0);
			if (testSet.connectedTo == connectedTo) {
				
				Notifications.ShowNotif (ViewControl.langEn ?
						"You: \"Test, test, one two...\"" : 
						"Εσύ: \"Τεστ, τεστ, ένα δύο...\"");
				
				talking = true;
				yield return new WaitForSeconds (2f);
				talking = false;

				if (works && testSet.turnedOn && testSet.curPage == 0) {
					Notifications.ShowNotif (ViewControl.langEn ?
						"You can hear yourself on the test set output." : 
						"Άκουσες τον εαυτό σου στη συσκευή ελέγχου.", 0);
				}
				else {
					Notifications.ShowNotif (ViewControl.langEn ?
						"You couldn't hear yourself on the test set output." :
						"Δεν άκουσες τον εαυτό σου στη συσκευή ελέγχου.", 1);
				}
			}
			else {
				
				Notifications.ShowNotif (
					(ViewControl.langEn ? "You:" : "Εσύ:") +
					" \"Hangar to ATC, radio check\"");
				
				talking = true;
				yield return new WaitForSeconds (2f);
				talking = false;
				yield return new WaitForSeconds (1f);

				if (works && testSet.aircraftAntenna.operational) {
					listening = true;
					ShowBubble (1);
					Notifications.ShowNotif (
						(ViewControl.langEn ? "Tower:" : "Πύργος ελέγχου:") + 
						" \"ATC to hangar, loud and clear\"", 0);
					yield return new WaitForSeconds (2.5f);
					listening = false;
				}
				else {
					Notifications.ShowNotif (ViewControl.langEn ?
						"No response received from tower." : 
						"Καμία απάντηση από τον πύργο ελέγχου.", 1);
				}
			}
		}

		private void SetConnected (int conn) {

			connectedTo = conn;
			iconDisconnect.SetActive (conn == 0);
			numConnect.enabled = conn > 0;
			if (conn > 0) numConnect.text = conn.ToString ();
		}

		private void ShowBubble (int index) {

			speechBubbles[index].FadeIn ();
			speechBubbles[index].ScheduleFadeOut (3f);
		}
	}
}