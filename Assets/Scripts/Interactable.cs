using Assets.Scripts.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts {
	public class Interactable : MonoBehaviour {

		private static readonly HashSet<Interactable> list = new HashSet<Interactable> ();

		[SerializeField]
		private int hotSpotType;
		[SerializeField]
		private string displayName;
		[SerializeField]
		private float multiplyScale = 1f;
		[SerializeField]
		private int requireZone = -1;
		
		private HotSpot hotSpot;

		[SerializeField]
		private UnityEvent onTrigger;
		private InteractHandler interactHandler;

		private const float minDistFromCamSqr = .04f;

		void Start () {

			var templateGo = ViewControl.Me.hotSpotTemplates[hotSpotType].gameObject;
			var hsgo = Instantiate (templateGo, templateGo.transform.parent);
			hotSpot = hsgo.GetComponent<HotSpot> ();
			hotSpot.SetAnchor (transform);
			hotSpot.SetLabelText (displayName);
			hotSpot.SetRelativeScale (multiplyScale, true);
			
			if (onTrigger != null) {
				hotSpot.AddTriggerListener (() => onTrigger.Invoke ());
			}

			interactHandler = GetComponent<InteractHandler> ();
			if (interactHandler != null) {
				hotSpot.AddTriggerListener (() => interactHandler.OnInteract ());
			}

			hotSpot.Init ();
			UpdateHotSpotVisibility ();
		}

		private void OnEnable () {

			list.Add (this);
			if (hotSpot && !hotSpot.gameObject.activeSelf) {
				hotSpot.gameObject.SetActive (true);
			}
		}

		private void OnDisable () {

			list.Remove (this);
			if (hotSpot && hotSpot.gameObject.activeSelf) {
				hotSpot.gameObject.SetActive (false);
			}
		}

		public static void UpdateHotSpotsVisibility () {

			foreach (var ii in list) {
				ii.UpdateHotSpotVisibility ();
			}
		}

		private void UpdateHotSpotVisibility () {

			var camPos = ViewControl.GetCamera ().transform.position;
			var curZone = ViewControl.GetCurrentZone ();
			
			bool show = (requireZone < 0 || requireZone == curZone) &&
				(transform.position - camPos).sqrMagnitude > minDistFromCamSqr;
			
			if (hotSpot.gameObject.activeSelf != show) {
				hotSpot.gameObject.SetActive (show);
			}
		}

		public void AcquireViewAsInternal (int zone) {
			ViewControl.BeginMoveTo (transform, true, zone);
		}

		public void AcquireViewAsExternal (int zone) {
			ViewControl.BeginMoveTo (transform, false, zone);
		}
	}

	public class InteractHandler : MonoBehaviour {
		public virtual void OnInteract () { }
	}
}