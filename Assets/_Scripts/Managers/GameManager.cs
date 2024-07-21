using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private PlayerDeathDataSO playerDeathData;
	[SerializeField] private List<GameObject> spawnedInLevelParents; 
	[SerializeField] private Transform levelSpawnPoint;
	[SerializeField] private PlayerHealth playerHealth;
	
	public static GameManager Instance;

	private void Awake() {
		if(Instance != null){
			Destroy(gameObject);
		}
		else{
			Instance = this;
		}
	}

	private void OnDestroy() {
		if(Instance == this){
			Instance = null;
		}
	}



}
