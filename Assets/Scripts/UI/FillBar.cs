using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI {
	class FillBar : MonoBehaviour {
		
		[SerializeField]
		private Transform targetTr;
		private Image img;
		public Vector3 useScale = Vector3.one;
		private Vector3 scaleInit;

		private void Awake () {

			if (!targetTr) targetTr = transform;
			if (!img) img = targetTr.GetComponent<Image> ();
			scaleInit = targetTr.localScale;
		}

		public void SetFill (float fill) {

			if (useScale != Vector3.one) {
				var s = scaleInit;
				var ss = Vector3.Lerp (useScale, Vector3.one, fill);
				s.x *= ss.x;
				s.y *= ss.y;
				s.z *= ss.z;
				targetTr.localScale = s;
			}
			else img.fillAmount = Mathf.Clamp01 (fill);
		}
	}
}
