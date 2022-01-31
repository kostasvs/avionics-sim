using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	public class Notifications : MonoBehaviour {

		private static Notifications me;

		[SerializeField]
		private GameObject template;

		[SerializeField]
		private float defDuration = 5f;

		[SerializeField]
		private Color[] typeColors;

		void Awake () {

			me = this;
		}

		public static void ShowNotif (string text, int ofType = -1, float duration = 0f) {

			if (!me) return;

			var go = Instantiate (me.template, me.template.transform.parent);
			var fd = go.GetComponent<FadeableDialog> ();
			fd.FadeIn ();

			if (duration <= 0f) duration = me.defDuration;
			fd.ScheduleFadeOut (duration);
			
			if (ofType >= 0 && ofType < me.typeColors.Length) {
				var img = go.GetComponent<Image> ();
				img.color = me.typeColors[ofType];
			}

			var txt = go.GetComponentInChildren<Text> ();
			txt.text = text;
		}
	}
}