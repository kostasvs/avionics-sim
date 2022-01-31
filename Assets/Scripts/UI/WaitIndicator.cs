using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	public class WaitIndicator : MonoBehaviour {

		public static WaitIndicator Me { get; private set; }
		private FadeableDialog fd;

		[SerializeField]
		private Image loader;
		[SerializeField]
		private Text label;

		private float duration;
		private float timer;
		
		private UnityAction onClose;

		void Start () {

			Me = this;
			fd = GetComponent<FadeableDialog> ();
			gameObject.SetActive (false);
		}

		private void Update () {

			if (!fd.IsOpened ()) return;
			timer += Time.deltaTime;
			loader.fillAmount = Mathf.Clamp01 (timer / duration);

			if (timer >= duration) {
				fd.FadeOut ();
				if (onClose != null) {
					onClose.Invoke ();
					onClose = null;
				}
			}
		}

		public static void ShowWait (string text, float dur, UnityAction onDone) {

			if (!Me) return;

			Me.label.text = text;
			Me.duration = dur;
			Me.timer = 0;
			Me.onClose = onDone;

			Me.fd.FadeIn ();
		}

		public static bool IsOpened () => Me && Me.gameObject.activeSelf;
	}
}