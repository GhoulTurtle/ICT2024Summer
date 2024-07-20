using System;
using UnityEngine;

public class WorldSwitchManager : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private WorldType startingWorldType = WorldType.TypeA;
	
	public static WorldSwitchManager Instance;

	public Action<WorldType> OnWorldTypeSwitch;

	private WorldType currentWorldType;

	private void Awake() {
		if(Instance != null){
			Destroy(gameObject);
			return;
		}
		else{
			Instance = this;
		}

		currentWorldType = startingWorldType;
	}

	private void OnDestroy() {
		if(Instance == this){
			Instance = null;
		}
	}

	public void SwitchWorldType(){
        switch (currentWorldType){
            case WorldType.TypeA:
				currentWorldType = WorldType.TypeB;
				break;
            case WorldType.TypeB:
				currentWorldType = WorldType.TypeA;
				break;
        }

		OnWorldTypeSwitch?.Invoke(currentWorldType);
    }

    public WorldType GetCurrentWorldType(){
		return currentWorldType;
	}
}