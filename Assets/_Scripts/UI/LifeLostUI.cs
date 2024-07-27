using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LifeLostUI : MonoBehaviour{
	[Header("UI References")]
	[SerializeField] private GameObject lostLifeUI;
	[SerializeField] private GameObject gameOverUI;

	[SerializeField] private TextMeshProUGUI livesRemainingText;
	[SerializeField] private TextMeshProUGUI deathLoreText;
	[SerializeField] private TextMeshProUGUI deathBuffGainedText;
	
	private const string MAIN_MENU = "Main Menu";

	private void Start() {
		if(GameManager.Instance != null){
			GameManager.Instance.OnValidPlayerDeathEntry += ShowLostLifeUI;
			GameManager.Instance.OnGameOver += ShowGameOverUI;
		}
	}

    private void OnDestroy() {
		if(GameManager.Instance != null){
			GameManager.Instance.OnValidPlayerDeathEntry -= ShowLostLifeUI;
			GameManager.Instance.OnGameOver -= ShowGameOverUI;
		}
	}

    private void ShowLostLifeUI(object sender, GameManager.ValidDeathPlayerDeathEntryEventArgs e){
		lostLifeUI.SetActive(true);

		deathLoreText.text = e.currentDeathLore;

		livesRemainingText.text = "Chances left: " + e.livesRemaining.ToString();
        switch (e.deathEntry.deathBuff){
            case DeathBuff.Health_Increase: deathBuffGainedText.text = "Max HP Increased.";
                break;
            case DeathBuff.Platforms_Added: deathBuffGainedText.text = "Added Platforms.";
                break;
        }
    }

	public void OnContinueInput(InputAction.CallbackContext context){
		if(context.phase != InputActionPhase.Performed) return;
		if(!lostLifeUI.activeInHierarchy && !gameOverUI.activeInHierarchy) return;

		Time.timeScale = 1f;

		if(lostLifeUI.activeInHierarchy){
			lostLifeUI.SetActive(false);
		}
		else if(gameOverUI){
			SceneManager.LoadScene(MAIN_MENU);
			Cursor.lockState = CursorLockMode.None;		
		}
	}

	public bool IsDeathUIActive(){
		return lostLifeUI.activeInHierarchy || gameOverUI.activeInHierarchy;
	}

    private void ShowGameOverUI(){
		gameOverUI.SetActive(true);
    }


}
