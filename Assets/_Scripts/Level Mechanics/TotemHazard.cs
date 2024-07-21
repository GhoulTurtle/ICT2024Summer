using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TotemHazard : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private List<TotemHitter> totemHitterList = new();

	[Header("Totem Hazard Variables")]
	[SerializeField] private float rotationSpeed;
	[SerializeField] private float damageAmountPerHit;

	private void Awake() {
		if(totemHitterList.Count == 0){
			totemHitterList = transform.GetComponentsInChildren<TotemHitter>().ToList();
		}

		if(totemHitterList.Count > 0){
			for (int i = 0; i < totemHitterList.Count; i++){
				totemHitterList[i].SetupHitter(damageAmountPerHit);
			}
		}	
	}

	private void Update() {
		transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
	}
}