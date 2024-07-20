using UnityEngine;
using UnityEngine.Events;

public class PlayerPickup : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private float pickupDestroyTimeInSeconds;
	[SerializeField] private PlayerPickupType pickupType;

	[Header("Pickup Events")]
	[SerializeField] private UnityEvent OnPickup;

	private const string PLAYER = "Player";
	private void OnTriggerEnter(Collider other) {
		if(!other.transform.CompareTag(PLAYER)) return;

		OnPickup?.Invoke();
        switch (pickupType){
            case PlayerPickupType.Health: //Heal the player
                break;
            case PlayerPickupType.Slide_Ability:         
				if (other.TryGetComponent(out PlayerMovement playerMovement)){
					playerMovement.UnlockSlideAbility();
				}
                break;
            case PlayerPickupType.World_Switch_Ability:
				if (other.TryGetComponent(out PlayerWorldSwitch playerWorldSwitch)){
					playerWorldSwitch.UnlockWorldSwitchAbility();
				}
                break;
        }
		Destroy(gameObject, pickupDestroyTimeInSeconds);
    }
}
