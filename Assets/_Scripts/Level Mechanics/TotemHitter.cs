using UnityEngine;

public class TotemHitter : MonoBehaviour{
	private const string PLAYER = "Player";

	private float damageAmount;

	public void SetupHitter(float _damageAmount){
		damageAmount = _damageAmount;
	}

	private void OnTriggerEnter(Collider other) {
		if(!other.gameObject.CompareTag(PLAYER)) return;

		if(other.gameObject.TryGetComponent(out PlayerHealth playerHealth)){
			playerHealth.TakeDamage(damageAmount, transform.position);
		}
	}
}
