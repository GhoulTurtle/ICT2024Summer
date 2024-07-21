using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private PlayerDeathDataSO playerDeathData;
	[SerializeField] private List<GameObject> spawnedInLevelParents; 
	[SerializeField] private Transform levelSpawnPoint;
	[SerializeField] private PlayerHealth playerHealth;

	private PlayerInput playerInput;

    private const string INGAME = "Player";
    private const string UI = "UI"; 
	
	public static GameManager Instance;

	private int currentPlayerDeathCounter = 0;

	public Action OnGameOver;
	public EventHandler<ValidDeathPlayerDeathEntryEventArgs> OnValidPlayerDeathEntry;
	public class ValidDeathPlayerDeathEntryEventArgs : EventArgs{
		public PlayerDeathEntry deathEntry;
		public int livesRemaining;

		public ValidDeathPlayerDeathEntryEventArgs(PlayerDeathEntry _deathEntry, int _livesRemaining){
			deathEntry = _deathEntry;
			livesRemaining = _livesRemaining;
		}
	}

	private void Awake() {
		if(Instance != null){
			Destroy(gameObject);
			return;
		}
		else{
			Instance = this;
		}

		if(playerHealth != null){
			playerHealth.OnDeathAction += PlayerDeathTriggered;
		}
	}

	private void Start() {
		for (int i = 0; i < spawnedInLevelParents.Count; i++){
			spawnedInLevelParents[i].SetActive(false);
		}
	}

    private void OnDestroy() {
		if(Instance == this){
			Instance = null;
		}

		playerHealth.OnDeathAction -= PlayerDeathTriggered;
	}

	private void PlayerDeathTriggered(){
		Time.timeScale = 0;

		if(!playerDeathData.IsDeathIndexValid(currentPlayerDeathCounter)){
			TriggerGameOver();
			return;
		}
		
		PlayerDeathEntry currentDeathEntry = playerDeathData.playerDeathEntries[currentPlayerDeathCounter];

		if(currentDeathEntry != null){
            switch (currentDeathEntry.deathBuff){
                case DeathBuff.Health_Increase: playerHealth.SetMaxHealth(currentDeathEntry.maxHealthAmount);
                    break;
                case DeathBuff.Platforms_Added: 
					if(spawnedInLevelParents.Count > currentDeathEntry.spawnedInLevelParentIndex){
						spawnedInLevelParents[currentDeathEntry.spawnedInLevelParentIndex].SetActive(true);
					}
				    break;
            }
        }

		int playerDeathEntryCount = playerDeathData.GetPlayerDeathEntryCount();
		int adjustedPlayerDeathCount = playerDeathEntryCount + 1;

		//Update game UI event
		OnValidPlayerDeathEntry?.Invoke(this, new ValidDeathPlayerDeathEntryEventArgs(currentDeathEntry, playerDeathEntryCount - adjustedPlayerDeathCount));

		//Reset the player
		playerHealth.ResetHealth();
		
		if(playerHealth.TryGetComponent(out CharacterController playerMovement)){
			playerMovement.enabled = false;
		}

		playerMovement.transform.position = levelSpawnPoint.position;

		if(playerMovement != null){
			playerMovement.enabled = true;
		}

        currentPlayerDeathCounter++;
    }

    private void TriggerGameOver(){
        OnGameOver?.Invoke();
		Time.timeScale = 1;
    }
}