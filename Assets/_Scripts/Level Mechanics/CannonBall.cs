using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour{
	[Header("Projectile Variables")]
	[SerializeField] private float collisonDetectionRadius = 3f;
	[SerializeField] private float maxProjectileLiveTime = 30f;
	[SerializeField] private bool hitMultipleTimes = false;
	
	[Header("Required References")]
	[SerializeField] private LayerMask dealDamageLayers; 
	[SerializeField] private LayerMask destoryProjectileLayers;
	[SerializeField] private int maxTargetColliders;

	private int damageAmount;
	private float projectileSpeed = 5f;

	private Collider[] targetColliders;

	private List<Collider> hitColliders;

	private Vector3 moveDir;

	private void Awake() {
		if(!hitMultipleTimes){
			hitColliders = new List<Collider>();
		}

		targetColliders = new Collider[maxTargetColliders];
	}

	public void SetupProjectile(int _damageAmount, float _projectileSpeed, Vector3 _moveDir){
		damageAmount = _damageAmount;
		projectileSpeed = _projectileSpeed;
		
		moveDir = _moveDir;

		transform.forward = moveDir;

		StartCoroutine(ProjectileLiveTimerCoroutine());
	}

	public float GetProjectileSize() {
		return collisonDetectionRadius;
	}

	private void Update() {
		MoveForward();
		DetectCollison();
	}

	private void OnDestroy() {
		StopAllCoroutines();
	}

	private void DetectCollison(){
		if(Physics.OverlapSphereNonAlloc(transform.position, collisonDetectionRadius, targetColliders, dealDamageLayers) == 0) return;
		
		foreach (Collider target in targetColliders){
			if(target == null || !target.TryGetComponent(out PlayerHealth healthSystem)) continue;
			if(!hitMultipleTimes && hitColliders.Contains(target)) continue;

			healthSystem.TakeDamage(damageAmount, transform.position);
		}

		if(Physics.CheckSphere(transform.position, collisonDetectionRadius, destoryProjectileLayers)){
			Destroy(gameObject);
		}
	}

	private void MoveForward(){
		transform.position += projectileSpeed * Time.deltaTime * moveDir;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, collisonDetectionRadius);	
	}

	private IEnumerator ProjectileLiveTimerCoroutine(){
		yield return new WaitForSecondsRealtime(maxProjectileLiveTime);
		Destroy(gameObject);
	}
}
