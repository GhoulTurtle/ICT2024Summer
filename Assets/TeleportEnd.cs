using System;
using UnityEngine;

public class TeleportEnd : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private Transform teleportToTransform;

	public Action<Transform> OnTeleportEntered;
	
	private const string PLAYER = "Player";

	private void OnTriggerEnter(Collider other) {
		if(!other.CompareTag(PLAYER)) return;

		OnTeleportEntered?.Invoke(other.transform);
	}

	public Transform GetTeleportPoint(){
		return teleportToTransform;
	}
}
