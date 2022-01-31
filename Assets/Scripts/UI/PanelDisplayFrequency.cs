using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	class PanelDisplayFrequency : MonoBehaviour {

		[SerializeField]
		private Text decText;
		[SerializeField]
		private Text fracText;

		private float lastFreq = 0f;

		public void SetFrequency (float freq) {

			if (lastFreq == freq) return;
			lastFreq = freq;
			decText.text = Mathf.FloorToInt (freq).ToString ("D3");
			fracText.text = Mathf.RoundToInt (freq % 1f * 100f).ToString ("D2");
		}
	}
}
