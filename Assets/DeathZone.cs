using UnityEngine;

public class DeathZone : MonoBehaviour{
	private const string PLAYER = "Player";
	
	private void OnTriggerEnter(Collider other) {
		if(!other.CompareTag(PLAYER)) return;

		if(other.TryGetComponent(out PlayerHealth playerHealth)){
			playerHealth.TriggerInstantDeath();
		}
	}
}