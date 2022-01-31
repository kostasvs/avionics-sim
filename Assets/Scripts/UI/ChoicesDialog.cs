using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	public class ChoicesDialog : MonoBehaviour {

		private static ChoicesDialog me;

		private FadeableDialog fd;
		
		[SerializeField]
		private Text defText;
		
		[SerializeField]
		private Button buttonTemplate;
		private List<Button> buttons = new List<Button> ();
		private int activeButtons = 0;

		[SerializeField]
		private Color[] typeColors;

		private bool pendingPos;

		private void Start () {

			me = this;
			fd = GetComponent<FadeableDialog> ();
			gameObject.SetActive (false);
		}

		private void Update () {
			if (pendingPos) {
				pendingPos = false;
				MoveToCursor ();
			}
		}
		public static void ClearChoices () {

			if (!me) return;
			me.activeButtons = 0;
			foreach (var b in me.buttons) {
				if (b.gameObject.activeSelf) b.gameObject.SetActive (false);
			}
		}

		public static void AddChoice (string label, UnityAction action, 
			bool addClose = true, int color = 0) {

			if (!me) return;
			Button btn;
			if (me.activeButtons >= me.buttons.Count) {
				var go = Instantiate (me.buttonTemplate, 
					me.buttonTemplate.transform.parent);
				btn = go.GetComponent<Button> ();
				me.buttons.Add (btn);
			}
			else btn = me.buttons[me.activeButtons];

			btn.image.color = me.typeColors[color];
			btn.GetComponentInChildren<Text> ().text = label;

			btn.onClick.RemoveAllListeners ();
			if (action != null) btn.onClick.AddListener (action);
			if (addClose) btn.onClick.AddListener (() => me.fd.FadeOut ());

			btn.gameObject.SetActive (true);
			me.activeButtons++;
		}

		public static void Display () {

			if (!me) return;
			me.defText.gameObject.SetActive (me.activeButtons == 0);
			if (!me.fd) me.fd = me.GetComponent<FadeableDialog> ();
			me.fd.FadeIn ();
			me.pendingPos = true;
			//me.Invoke (nameof (MoveToCursor), .1f);
		}

		private void MoveToCursor () {

			Vector2 pos = Input.mousePosition;
			var s = transform.localScale.x;
			if (s > 0) {
				pos /= s;
				var rect = fd.targetRectTransform.rect;
				pos.x = Mathf.Clamp (pos.x, rect.width / 2, 
					Screen.width / s - rect.width / 2);
				pos.y = Mathf.Clamp (pos.y, rect.height / 2,
					Screen.height / s - rect.height / 2);
			}
			fd.targetRectTransform.anchoredPosition = pos;
		}

		public static bool IsOpened () => me && me.fd.IsOpened ();
	}
}