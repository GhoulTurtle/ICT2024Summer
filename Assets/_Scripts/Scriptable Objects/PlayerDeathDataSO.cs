using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Death Data", fileName = "NewPlayerDeathData")]
public class PlayerDeathDataSO : ScriptableObject{
	[Header("Required References")]
	public List<PlayerDeathEntry> playerDeathEntries;

	public bool IsDeathIndexValid(int index){
		if(playerDeathEntries.Count == index) return false;
		return true;
	}

	public int GetPlayerDeathEntryCount(){
		return playerDeathEntries.Count;
	}
}
