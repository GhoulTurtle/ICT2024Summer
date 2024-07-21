using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Death Data", fileName = "NewPlayerDeathData")]
public class PlayerDeathDataSO : ScriptableObject{
	[Header("Required References")]
	public List<PlayerDeathEntry> playerDeathEntries;
	
}
