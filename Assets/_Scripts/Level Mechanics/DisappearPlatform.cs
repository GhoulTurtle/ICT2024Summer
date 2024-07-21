using System.Collections;
using UnityEngine;

public class DisappearPlatform : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private Collider platformCollider;
	[SerializeField] private MeshRenderer platformMeshRenderer;

	[Header("Timing Variables")]
	[SerializeField] private float disppearTime;
	[SerializeField] private float appearTime;

	private const string PLAYER = "Player";

	private bool isActive;

	private IEnumerator currentPlatformTimer;

	private void OnDestroy() {
		StopAllCoroutines();
	}

	private void OnTriggerEnter(Collider other) {
		if(!other.CompareTag(PLAYER) || currentPlatformTimer != null) return;

		StartPlatformTimer(true);
	}

	private void StartPlatformTimer(bool isDisappearing){
		currentPlatformTimer = null;
		currentPlatformTimer = PlatformTimerCoroutine(isDisappearing);
		StartCoroutine(currentPlatformTimer);
	}

	private IEnumerator PlatformTimerCoroutine(bool isDisappearing){
		float waitTime = isDisappearing ? disppearTime : appearTime;

		yield return new WaitForSeconds(waitTime);

		isActive = !isDisappearing;

		if(isActive){
			platformCollider.enabled = true;
			platformMeshRenderer.enabled = true;
		}
		else{
			platformCollider.enabled = false;
			platformMeshRenderer.enabled = false;
		}

		if(isActive){
			currentPlatformTimer = null;
			yield break;
		} 
		
		StartPlatformTimer(false);
	}
}
