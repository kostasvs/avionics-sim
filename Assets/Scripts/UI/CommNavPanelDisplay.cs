using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	public class CommNavPanelDisplay : MonoBehaviour {

		[SerializeField]
		private PanelDisplayFrequency commUse;

		[SerializeField]
		private PanelDisplayFrequency commStby;

		[SerializeField]
		private PanelDisplayFrequency navUse;

		[SerializeField]
		private PanelDisplayFrequency navStby;

		[SerializeField]
		private Text trIndicator;
		private const string trTransmit = "T";
		private const string trReceive = "R";

		public void SetComm (float freq, bool standby) {
			(standby ? commStby : commUse).SetFrequency (freq);
		}

		public void SetCommUse (float freq) {
			SetComm (freq, false);
		}

		public void SetCommStby (float freq) {
			SetComm (freq, true);
		}

		public void SetNav (float freq, bool standby) {
			(standby ? navStby : navUse).SetFrequency (freq);
		}

		public void SetNavUse (float freq) {
			SetNav (freq, false);
		}

		public void SetNavStby (float freq) {
			SetNav (freq, true);
		}

		public void SetIndicator (int transmitReceive) {

			switch (transmitReceive) {
				case 1:
					trIndicator.enabled = true;
					trIndicator.text = trTransmit;
					break;
				case 2:
					trIndicator.enabled = true;
					trIndicator.text = trReceive;
					break;
				default:
					trIndicator.enabled = false;
					break;
			}
		}
	}
}