using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	public class AdfPanelDisplay : MonoBehaviour {

		[SerializeField]
		private Text freqText;

		[SerializeField]
		private Image annunciatorImage;

		private int lastFreq = 0;

		public void SetFrequency (int freq) {

			if (lastFreq == freq) return;
			lastFreq = freq;
			freqText.text = freq.ToString ("D4");
		}

		public void SetAnnunciator (bool illuminate) {

			annunciatorImage.enabled = illuminate;
		}
	}
}