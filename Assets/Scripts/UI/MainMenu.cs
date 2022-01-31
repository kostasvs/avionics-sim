using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI {
	public class MainMenu : MonoBehaviour {

		public void MakeChoice (int choice) {
			UseCaseToggle.useCase = choice;
			SceneManager.LoadScene (1);
		}

		public static void ReturnToMenu () {
			SceneManager.LoadScene (0);
		}

		public void ExitGame () {
			Application.Quit ();
		}
	}
}
