using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour{
	[Header("Pressure Plate Events")]
	public UnityEvent OnPress;
	public UnityEvent OnUnpress;
	
	private bool isPressed = false;
	
	private const string PLAYER = "Player";

	private void OnTriggerEnter(Collider other) {
		if(!other.CompareTag(PLAYER) || isPressed) return;

		isPressed = true;
		OnPress?.Invoke();
	}

	private void OnTriggerExit(Collider other){
		if(!other.CompareTag(PLAYER) || !isPressed) return;

		isPressed = false;
		OnUnpress?.Invoke();
	}
}