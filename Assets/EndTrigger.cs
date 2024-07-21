using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTrigger : MonoBehaviour{
	private const string PLAYER = "Player";
	private const string CREDITS = "Credits";

	private void OnTriggerEnter(Collider other) {
		if(!other.CompareTag(PLAYER)) return;

		SceneManager.LoadScene(CREDITS);
	}
}
