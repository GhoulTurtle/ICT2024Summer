using UnityEngine;
using UnityEngine.Events;

public class PlayerPickup : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private float pickupDestroyTimeInSeconds;

	[Header("Pickup Events")]
	[SerializeField] private UnityEvent OnPickup;

	private const string PLAYER = "Player";
	private void OnTriggerEnter(Collider other) {
		if(!other.transform.CompareTag(PLAYER)) return;

		OnPickup?.Invoke();
		if(other.TryGetComponent(out PlayerMovement playerMovement)){
			playerMovement.UnlockSlideAbility();
		}
		Destroy(gameObject, pickupDestroyTimeInSeconds);
	}
}
