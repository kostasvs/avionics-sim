using Assets.Scripts;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

public class ViewControl : MonoBehaviour {

	public static ViewControl Me { get; private set; }

	public static bool langEn = true;

	private Camera cam;
	private Transform camTr;

	private Vector2 rotAngles;
	private Vector2 rotAlignAngles;
	private Quaternion rotAlign;
	
	[SerializeField]
	private float rotSens = 2f;
	[SerializeField]
	private float rotLimitX = 85f;

	private float fovInit;
	[SerializeField]
	private float fovStep = 10f;
	[SerializeField]
	private float fovMin = 30f;

	[SerializeField]
	private Button resetButton;
	private RectTransform resetButtonTr;
	private Vector2 resetButtonOrigin;
	private Vector2 resetButtonFlippedOrigin;
	[SerializeField]
	private float resetButtonSlideSpeed = 150f;
	[SerializeField]
	private float resetButtonShowMinAngle = 10f;
	[SerializeField]
	private float resetTurnSpeed = 180f;

	private bool showReset;
	private bool resetting;

	private Vector3 lastPos;
	private Quaternion lastRot;
	private Transform[] pathPoints;
	private float movePhase;
	private int movePointIndex;
	[SerializeField]
	private float moveDur = .7f;

	private bool inInternal = false;
	private int zoneIndex = -1;
	[SerializeField]
	private Transform gatewayPoint;

	[SerializeField]
	private CanvasGroup hotspotsGroup;
	[SerializeField]
	private float hotspotsFadeDur = .25f;

	public HotSpot[] hotSpotTemplates;

	void Awake () {

		Me = this;
		cam = Camera.main;
		fovInit = cam.fieldOfView;
		camTr = cam.transform;
		InitializeAlign ();

		resetButtonTr = resetButton.GetComponent<RectTransform> ();
		resetButtonOrigin = resetButtonTr.anchoredPosition;
		resetButtonFlippedOrigin = resetButtonOrigin;
		resetButtonFlippedOrigin.y = -resetButtonFlippedOrigin.y;
		resetButtonTr.anchoredPosition = resetButtonFlippedOrigin;
		resetButton.gameObject.SetActive (false);
		resetButton.onClick.AddListener (() => {
			if (pathPoints != null) return;
			resetting = true; 
			showReset = false;
			ResetZoom ();
		});
	}

	void Update () {

		if (Input.GetKeyDown (KeyCode.Escape)) {
			MainMenu.ReturnToMenu ();
			return;
		}

		float hotspotsAlpha = pathPoints == null ? 1f : 0f;
		if (hotspotsGroup.alpha != hotspotsAlpha) {
			hotspotsGroup.alpha = Mathf.MoveTowards (hotspotsGroup.alpha, hotspotsAlpha,
				Time.deltaTime / hotspotsFadeDur);
		}

		bool allowTrigger = true, allowZoom = true;
		if (pathPoints != null) {
			var ppt = pathPoints[movePointIndex];
			movePhase += Time.deltaTime / moveDur;
			if (movePhase > 1f) {
				transform.SetPositionAndRotation (ppt.position, ppt.rotation);
				lastPos = ppt.position;
				lastRot = ppt.rotation;
				InitializeAlign ();
				movePointIndex++;
				if (movePointIndex >= pathPoints.Length) {
					pathPoints = null;
					Interactable.UpdateHotSpotsVisibility ();
				}
				movePhase = 0f;
			}
			else {
				transform.SetPositionAndRotation (
					Vector3.Lerp (lastPos, ppt.position, movePhase),
					Quaternion.Slerp (lastRot, ppt.rotation, movePhase));
			}
			
			allowTrigger = false;
			allowZoom = false;
		}
		else if (resetting) {
			var dt = resetTurnSpeed * Time.deltaTime;
			if (Quaternion.Angle (camTr.rotation, rotAlign) <= dt) {
				camTr.rotation = rotAlign;
				rotAngles = rotAlignAngles;
				resetting = false;
			}
			else camTr.rotation = Quaternion.RotateTowards (camTr.rotation,
				rotAlign, dt);
			
			allowTrigger = false;
		}
		else if (Input.GetMouseButton (1)) {
			rotAngles.x -= rotSens * Input.GetAxis ("Mouse Y");
			rotAngles.y += rotSens * Input.GetAxis ("Mouse X");

			rotAngles.x = Mathf.Clamp (rotAngles.x, -rotLimitX, rotLimitX);
			rotAngles.y %= 360f;

			camTr.eulerAngles = rotAngles;
			
			showReset = Quaternion.Angle (camTr.rotation, rotAlign) >
				resetButtonShowMinAngle;
			
			allowTrigger = false;
		}

		if (allowTrigger) allowTrigger = !ChoicesDialog.IsOpened () &&
				!WaitIndicator.IsOpened ();

		if (allowZoom) {
			var scroll = Input.GetAxis ("Mouse ScrollWheel");
			if (scroll != 0) {
				cam.fieldOfView = Mathf.Clamp (
					cam.fieldOfView - Mathf.Sign (scroll) * fovStep, fovMin, fovInit);
			}
		}
		var btnPos = showReset ? resetButtonOrigin : resetButtonFlippedOrigin;
		
		if (resetButton.gameObject.activeSelf != showReset && 
			(showReset || resetButtonTr.anchoredPosition == btnPos)) {
			resetButton.gameObject.SetActive (showReset);
		}

		if (resetButtonTr.anchoredPosition != btnPos) {
			resetButtonTr.anchoredPosition = Vector2.MoveTowards (
				resetButtonTr.anchoredPosition, btnPos, 
				resetButtonSlideSpeed * Time.deltaTime);
		}

		HotSpot.CheckHover (Input.mousePosition,
			allowTrigger && Input.GetMouseButtonDown (0));
	}

	public static void InitializeAlign () {

		if (!Me) return;
		Me.rotAngles = Me.camTr.eulerAngles;
		Me.rotAlignAngles = Me.camTr.eulerAngles;
		Me.rotAlign = Me.camTr.rotation;
		Me.showReset = false;
	}

	public static void ResetZoom () {

		if (!Me) return;
		Me.cam.fieldOfView = Me.fovInit;
	}

	public static void BeginMoveTo (Transform dest, bool isInternal, int zone) {

		if (!Me) return;
		if (isInternal == Me.inInternal) {
			Me.pathPoints = new Transform[] { dest };
		}
		else {
			Me.inInternal = isInternal;
			Me.pathPoints = new Transform[] { Me.gatewayPoint, dest };
		}
		Me.movePointIndex = 0;
		Me.movePhase = 0f;
		Me.lastPos = Me.transform.position;
		Me.lastRot = Me.transform.rotation;
		Me.zoneIndex = zone;
		Me.showReset = false;
		ResetZoom ();
	}

	public static Camera GetCamera () {
		return Me ? Me.cam : null;
	}

	public static int GetCurrentZone () {
		return Me ? Me.zoneIndex : -1;
	}
}
