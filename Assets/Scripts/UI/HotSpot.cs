using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	public class HotSpot : MonoBehaviour {

		private static readonly HashSet<HotSpot> list = new HashSet<HotSpot> ();

		private float phase;
		private const float phaseChangeSpeed = 4f;
		private RectTransform rtr;

		[SerializeField]
		private bool autoHide = true;

		[SerializeField]
		private Image icon;
		private Color colorInit;

		[SerializeField]
		private Transform anchor;
		private float curDepth;

		[SerializeField]
		private Color colorHover;

		[SerializeField]
		private Text labelText;
		private CanvasGroup labelCG;
		private RectTransform labelParent;
		private Vector2 labelParentPosInit;

		private const float iconScaleOnFocus = 1.25f;
		[SerializeField]
		private float relativeScale = 1f;
		private Vector3 iconScaleInit;
		private const float wobbleScale = .06f;
		private const float wobbleFrq = .4f;

		public bool focused;
		private UnityEvent onTrigger;

		private bool hasInit;

		private void Start () {
			Init ();
		}

		public void Init () {

			if (hasInit) return;
			hasInit = true;

			rtr = GetComponent<RectTransform> ();
			labelParent = labelText.transform.parent.GetComponent<RectTransform> ();
			labelCG = labelParent.GetComponent<CanvasGroup> ();

			colorInit = icon.color;
			icon.transform.localScale *= relativeScale;
			iconScaleInit = icon.transform.localScale;
			labelParent.anchoredPosition *= relativeScale;
			labelParentPosInit = labelParent.anchoredPosition;
		}

		private void OnEnable () {
			list.Add (this);
		}
		private void OnDisable () {
			list.Remove (this);
		}

		void Update () {

			if (anchor) {
				var pos = ViewControl.GetCamera ().WorldToScreenPoint (anchor.position);
				
				bool toShow = true;
				if (autoHide) {
					toShow = pos.x > 0 && pos.y > 0 &&
					pos.x < Screen.width && pos.y < Screen.height && pos.z >= 0;
				}
				else {
					if (pos.z < 0) {
						pos = -pos;
						pos.x = pos.x > Screen.width / 2 ? Screen.width : 0;
						pos.z = 0;
					}
					else pos.x = Mathf.Clamp (pos.x, 0, Screen.width);
					pos.y = Mathf.Clamp (pos.y, 0, Screen.height);
				}

				if (toShow) {
					if (!icon.gameObject.activeSelf) icon.gameObject.SetActive (true);
					
					var s = rtr.parent.localScale.x;
					if (s > 0) pos /= s;
					rtr.anchoredPosition = pos;
					curDepth = pos.z;

					if (phase > 0 && s > 0) {
						var lpos = labelParentPosInit;
						var sd = labelParent.sizeDelta;
						if (pos.y + lpos.y + sd.y / 2 > Screen.height / s) {
							lpos.y = -lpos.y;
						}
						lpos.x = Mathf.Clamp (lpos.x,
							sd.x / 2 - pos.x,
							Screen.width / s - sd.x / 2 - pos.x);
						labelParent.anchoredPosition = lpos;
					}
				}
				else {
					if (icon.gameObject.activeSelf) icon.gameObject.SetActive (false);
					curDepth = -1f;
				}
			}

			var prevPhase = phase;
			phase = Mathf.MoveTowards (phase, focused ? 1 : 0, 
				phaseChangeSpeed * Time.deltaTime);
			if (prevPhase != phase) {
				labelCG.alpha = phase;
				icon.color = Color.Lerp (colorInit, colorHover, phase);
			}
			var w = wobbleScale * Mathf.Sin (6.28f * wobbleFrq * Time.time);
			icon.transform.localScale = (1f + w) *
				Mathf.Lerp (1, iconScaleOnFocus, phase) * iconScaleInit;
		}

		public static void CheckHover (Vector2 mousePos, bool press) {

			HotSpot bestSpot = null;
			float bestDepth = 0f;
			foreach (var hs in list) {

				hs.focused = false;
				if (hs.curDepth < 0) continue;

				var s = hs.rtr.parent.localScale.x;
				var pos = hs.rtr.anchoredPosition * s;
				var delta = pos - mousePos;
				if (s > 0) delta /= s;
				if (hs.relativeScale > 0) delta /= hs.relativeScale;
				if (!hs.rtr.rect.Contains (delta)) continue;

				if (bestSpot == null || hs.curDepth < bestDepth) {
					bestSpot = hs;
					bestDepth = hs.curDepth;
				}
			}
			if (bestSpot) {
				bestSpot.focused = true;
				if (press) bestSpot.onTrigger.Invoke ();
			}
		}

		public void SetLabelText (string text) {
			labelText.text = text;
		}

		public void SetRelativeScale (float rs, bool multiply = false) {
			if (hasInit) Debug.LogWarning ("SetRelativeScale called after init");
			else if (multiply) relativeScale *= rs;
			else relativeScale = rs;
		}

		public void SetAnchor (Transform anchor) {
			this.anchor = anchor;
		}

		public void AddTriggerListener (UnityAction action) {
			if (onTrigger == null) onTrigger = new UnityEvent ();
			onTrigger.AddListener (action);
		}
	}
}