using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour{
    [Header("Required References")]
    [SerializeField] private GameObject pauseMenuUI;

    private bool isActive = false;

    private const string MAIN_MENU = "Main Menu";

    public void PauseInput(InputAction.CallbackContext context){
        if(context.phase != InputActionPhase.Performed) return;

        SetPauseMenuState(!isActive);
    }

    public void SetPauseMenuState(bool state){
        isActive = state;

        pauseMenuUI.SetActive(isActive);

        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;

        Time.timeScale = isActive ? 0f : 1f;
    }

	public void LoadMainMenu(){
        Time.timeScale = 1f;

		SceneManager.LoadScene(MAIN_MENU);
	}
}
