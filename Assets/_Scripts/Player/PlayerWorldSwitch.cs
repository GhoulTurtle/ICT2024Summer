using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerWorldSwitch : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private bool isWorldSwitchUnlocked = false;
	[SerializeField] private float worldSwitchCooldown = 0.25f;

	[Header("Unity Events")]
	public UnityAction OnWorldSwitchUnlocked;

	private IEnumerator currentWorldSwitchCooldown;

	private void OnDestroy() {
		StopAllCoroutines();
	}

	public void UnlockWorldSwitchAbility(){
		if(isWorldSwitchUnlocked) return;

		isWorldSwitchUnlocked = true;
		OnWorldSwitchUnlocked?.Invoke();
	}

	public void WorldSwitch(InputAction.CallbackContext context){
		if(!isWorldSwitchUnlocked) return;
		if(context.phase != InputActionPhase.Performed) return;

		if(WorldSwitchManager.Instance != null){
			WorldSwitchManager.Instance.SwitchWorldType();
		}

		currentWorldSwitchCooldown = WorldSwitchCooldownCoroutine();
		StartCoroutine(currentWorldSwitchCooldown);
	}

	private IEnumerator WorldSwitchCooldownCoroutine(){
		yield return new WaitForSeconds(worldSwitchCooldown);
		currentWorldSwitchCooldown = null;
	}
}