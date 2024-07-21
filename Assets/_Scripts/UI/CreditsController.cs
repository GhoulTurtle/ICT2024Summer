using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour{
	private const string MAIN_MENU = "Main Menu";

	public void LoadMainMenu(){
		SceneManager.LoadScene(MAIN_MENU);
	}
}