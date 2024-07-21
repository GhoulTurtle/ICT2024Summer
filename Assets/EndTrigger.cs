using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTrigger : MonoBehaviour{
	private const string PLAYER = "Player";
	private const string CREDITS = "Credits";

	private void OnTriggerEnter(Collider other) {
		if(!other.CompareTag(PLAYER)) return;

		Cursor.lockState = CursorLockMode.None;
		Time.timeScale = 1f;
		SceneManager.LoadScene(CREDITS);
	}
}
