using UnityEngine;

[CreateAssetMenu(menuName = "Death Lore Data", fileName = "NewDeathLoreData")]
public class DeathLoreDataSO : ScriptableObject{
	public string[] deathLoreSentences = new string[5];
}
