using System.Collections;
using UnityEngine;

public class DamagePlatform : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private Material safeMaterial;
	[SerializeField] private Material notSafeMaterial;
	[SerializeField] private MeshRenderer platformMeshRenderer;

	[Header("Platform Variables")]
	[SerializeField] private float platformSwitchTimer;
	[SerializeField] private bool isDangerous = false;
	[SerializeField] private bool isManualTimer;

	[Header("Damage Variables")]
	[SerializeField] private int damagePerTick = 1;
	[SerializeField] private float damageTickCooldownInSeconds = 1.5f;

	private IEnumerator currentPlatformTimer;
	private IEnumerator currentDamageCooldown;

	private const string PLAYER = "Player";

	private void Awake() {
		UpdatePlatformState();
		
		if(isManualTimer) return;

		StartTimer();
	}

	private void OnDestroy() {
		StopAllCoroutines();
	}

	public void SwitchPlatformState(){
		isDangerous = !isDangerous;
		UpdatePlatformState();
	}

	public void StartTimer(){
		currentPlatformTimer = PlatformTimerCoroutine();
		StartCoroutine(currentPlatformTimer);
	}

	private void UpdatePlatformState(){
		if(isDangerous){
			platformMeshRenderer.material = notSafeMaterial;
		}
		else{
			platformMeshRenderer.material = safeMaterial;
		}
	}

	private void OnTriggerStay(Collider other) {
		if(!other.CompareTag(PLAYER) || currentDamageCooldown != null || !isDangerous) return;

		if(other.TryGetComponent(out PlayerHealth playerHealth)){
			playerHealth.TakeDamage(damagePerTick, transform.position);
			currentDamageCooldown = DamageTickTimerCoroutine();
			StartCoroutine(currentDamageCooldown);
		}
	}

	private IEnumerator PlatformTimerCoroutine(){
		yield return new WaitForSeconds(platformSwitchTimer);
		
		isDangerous = !isDangerous;
		
		UpdatePlatformState();

		if(isManualTimer) yield break;

		StartTimer();
	}

	private IEnumerator DamageTickTimerCoroutine(){
		yield return new WaitForSeconds(damageTickCooldownInSeconds);
		currentDamageCooldown = null;
	}

}
