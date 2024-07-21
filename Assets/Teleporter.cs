using UnityEngine;

public class Teleporter : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private TeleportEnd teleportEndA;
	[SerializeField] private TeleportEnd teleportEndB;

	private void Awake() {
		teleportEndA.OnTeleportEntered += TeleporterAEntered;
		teleportEndB.OnTeleportEntered += TeleporterBEntered;
	}

	private void OnDestroy() {
		teleportEndA.OnTeleportEntered += TeleporterAEntered;
		teleportEndB.OnTeleportEntered += TeleporterBEntered;
	}

    private void TeleporterAEntered(Transform playerTransform){
        if(playerTransform.TryGetComponent(out CharacterController playerMovement)){
			playerMovement.enabled = false;
		}

		playerMovement.transform.position = teleportEndB.GetTeleportPoint().position;

		if(playerMovement != null){
			playerMovement.enabled = true;
		}
    }

	private void TeleporterBEntered(Transform playerTransform){
		if(playerTransform.TryGetComponent(out CharacterController playerMovement)){
			playerMovement.enabled = false;
		}

		playerMovement.transform.position = teleportEndA.GetTeleportPoint().position;

		if(playerMovement != null){
			playerMovement.enabled = true;
		}
    }
}
