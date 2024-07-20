using System.Collections;
using UnityEngine;

public class TimerPlatform : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private Material activeMaterial;
	[SerializeField] private Material inactiveMaterial;
	[SerializeField] private MeshRenderer platformMeshRenderer;
	[SerializeField] private Collider platformCollider;

	[Header("Platform Variables")]
	[SerializeField] private float platformSwitchTimer;
	[SerializeField] private bool isActive = false;
	[SerializeField] private bool isManualTimer;

	private IEnumerator currentPlatformTimer;

	private void Awake() {
		UpdatePlatformState();
		
		if(isManualTimer) return;

		StartTimer();
	}

	private void OnDestroy() {
		StopAllCoroutines();
	}

	public void StartTimer(){
		currentPlatformTimer = PlatformTimerCoroutine();
		StartCoroutine(currentPlatformTimer);
	}

	private IEnumerator PlatformTimerCoroutine(){
		yield return new WaitForSeconds(platformSwitchTimer);
		
		isActive = !isActive;
		
		UpdatePlatformState();

		if(isManualTimer) yield break;

		StartTimer();
	}

	private void UpdatePlatformState(){
		if(isActive){
			platformMeshRenderer.material = activeMaterial;
			platformCollider.enabled = true;
		}
		else{
			platformMeshRenderer.material = inactiveMaterial;
			platformCollider.enabled = false;
		}
	}
}
