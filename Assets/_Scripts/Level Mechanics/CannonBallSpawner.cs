using System.Collections;
using UnityEngine;

public class CannonBallSpawner : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private CannonBall cannonBallPrefab;
	[SerializeField] private Transform cannonBallSpawnPos;
	
	[Header("Cannon Ball Spawner Variables")]
	[SerializeField] private bool startSpawningOnStart = false;
	[SerializeField] private float cannonBallSpawnCooldown;
	[SerializeField] private float cannonBallFlySpeed = 5f;
	[SerializeField] private int cannonBallDamageAmount = 1;

	private IEnumerator currentCannonBallSpawnCooldown;
	private IEnumerator currentCannonBallSpawnCoroutine;

	private void Awake() {
		if(startSpawningOnStart){
			StartSpawningCannonBalls();
		}
	}

	public void StartSpawningCannonBalls(){
		StopSpawningCannonBalls();

		currentCannonBallSpawnCoroutine = CannonBallSpawnCoroutine();
		StartCoroutine(currentCannonBallSpawnCoroutine);
	}

	public void StopSpawningCannonBalls(){
		if(currentCannonBallSpawnCoroutine != null){
			StopCoroutine(currentCannonBallSpawnCoroutine);
			currentCannonBallSpawnCoroutine = null;
		}
	}

	private IEnumerator CannonBallSpawnCoroutine(){
		while(true){
			if(currentCannonBallSpawnCooldown == null){
				CannonBall spawnedCannonBall = Instantiate(cannonBallPrefab, cannonBallSpawnPos.position, Quaternion.identity);
				spawnedCannonBall.SetupProjectile(cannonBallDamageAmount, cannonBallFlySpeed, transform.forward);

				currentCannonBallSpawnCooldown = CannonBallCooldownCoroutine();
				StartCoroutine(currentCannonBallSpawnCooldown);
			}

			yield return null;
		}
	}

	private IEnumerator CannonBallCooldownCoroutine(){
		yield return new WaitForSeconds(cannonBallSpawnCooldown);
		currentCannonBallSpawnCooldown = null;
	}
}