using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	public class TheoryPanel : MonoBehaviour {

		[SerializeField]
		private Transform contentPanel;
		[SerializeField]
		private Button prevBtn;
		[SerializeField]
		private Button nextBtn;
		[SerializeField]
		private Image progressBar;

		private int curChild = -1;

		private void OnEnable () {
			GoToChild (0);
		}

		private void Update () {
		
			if (Input.GetKeyDown (KeyCode.Space) || Input.GetKeyDown (KeyCode.Return)) {
				ShiftChild (1);
			}
		}

		public void ShiftChild (int shift) {
			GoToChild (curChild + shift);
		}

		public void GoToChild (int ind) {

			int prev = curChild;
			curChild = Mathf.Clamp (ind, 0, contentPanel.childCount - 1);
			if (prev == curChild) return;

			for (int i = 0; i < contentPanel.childCount; i++) {
				var c = contentPanel.GetChild (i);
				var fd = c.GetComponent<FadeableDialog> ();
				if (fd) {
					if (i == curChild) fd.FadeIn ();
					else fd.FadeOut ();
				}
				else c.gameObject.SetActive (i == curChild);
			}

			prevBtn.interactable = curChild > 0;
			nextBtn.interactable = curChild < contentPanel.childCount - 1;
			progressBar.fillAmount = curChild / (float)(contentPanel.childCount - 1);
		}
	}
}