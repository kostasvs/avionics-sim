using Assets.Scripts.InteractHandlers;
using UnityEngine;

namespace Assets.Scripts {
	public class UseCaseToggle : MonoBehaviour {

		public static int useCase = -1;

		[SerializeField]
		private Transform commNavView;
		[SerializeField]
		private Transform atcView;
		[SerializeField]
		private Transform finTopView;

		[SerializeField]
		private RectTransform headsetButton;

		[SerializeField]
		private CommNavTestSet commNavTestSet;
		[SerializeField]
		private Transform commNavTestSetModel;
		[SerializeField]
		private CommNavSystem[] commNavSystems;
		[SerializeField]
		private AtcTestSet atcTestSet;
		[SerializeField]
		private Transform atcTestSetModel;
		[SerializeField]
		private AtcTransponder atcTransponder;

		[SerializeField]
		private GameObject postitVOR;

		void Start () {

			if (useCase < 0) return;

			Interactable i;
			if (useCase != 0 && useCase != 1) {

				commNavView.gameObject.SetActive (false);
				commNavTestSet.gameObject.SetActive (false);
				commNavTestSetModel.gameObject.SetActive (false);
				commNavTestSet.coaxial.SetActive (false);

				for (int j = 0; j < commNavSystems.Length; j++) {
					i = commNavSystems[j].GetComponent<Interactable> ();
					i.enabled = false;
				}
			}

			headsetButton.gameObject.SetActive (useCase == 0);
			finTopView.gameObject.SetActive (useCase == 1);
			postitVOR.SetActive (useCase == 1);
			
			i = commNavTestSet.aircraftAntenna.GetComponent<Interactable> ();
			i.enabled = useCase == 0;
			i = commNavTestSet.aircraftVorAntenna.GetComponent<Interactable> ();
			i.enabled = useCase == 1;

			if (useCase != 2) {

				atcView.gameObject.SetActive (false);
				atcTestSet.gameObject.SetActive (false);
				atcTestSetModel.gameObject.SetActive (false);
				atcTestSet.coaxial.SetActive (false);
				atcTestSet.antenna.gameObject.SetActive (false);

				i = atcTestSet.aircraftAntenna.GetComponent<Interactable> ();
				i.enabled = false;

				i = atcTransponder.GetComponent<Interactable> ();
				i.enabled = false;
			}
		}
	}
}