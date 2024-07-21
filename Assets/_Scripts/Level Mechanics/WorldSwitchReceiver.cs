using UnityEngine;

public class WorldSwitchReceiver : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private WorldSwitchDataSO worldSwitchDataSO;
	[SerializeField] private Collider recieverCollider;
	[SerializeField] private MeshRenderer recieverMeshRenderer;
	[SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
	[SerializeField] private bool isTied = false;
	[SerializeField] private WorldType tiedWorldType;
	[SerializeField] private WorldSwitchReceiverType worldSwitchReceiverType;

	private WorldSwitchManager worldSwitchManager;
	private WorldType currentWorldType;

	private void Start() {
		if(WorldSwitchManager.Instance != null){
			worldSwitchManager = WorldSwitchManager.Instance;
			SetupWorldSwitchReciever();
		}
	}

	private void OnDestroy() {
		if(worldSwitchManager != null){
			worldSwitchManager.OnWorldTypeSwitch -= WorldSwitchTriggered;
		}
	}

    private void SetupWorldSwitchReciever(){
		currentWorldType = worldSwitchManager.GetCurrentWorldType();
		worldSwitchManager.OnWorldTypeSwitch += WorldSwitchTriggered;


		if(isTied){
			UpdateMaterialOnWorldType(tiedWorldType);
		}
		else{
			UpdateMaterialOnWorldType(currentWorldType);
		}
		WorldSwitchTriggered(currentWorldType);
    }

	private void UpdateMaterialOnWorldType(WorldType worldType){
		if(worldSwitchDataSO != null){
            switch (worldType){
                case WorldType.TypeA:
                    switch (worldSwitchReceiverType){
                        case WorldSwitchReceiverType.Platform: recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeAMat;
                            break;
                        case WorldSwitchReceiverType.Sky: recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeASkyBoxMat;
                            break;
                        case WorldSwitchReceiverType.Statue:
							if (recieverMeshRenderer != null) {
                                recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeAStatueMat;
                            }
							
							if (skinnedMeshRenderer != null) {
								skinnedMeshRenderer.material = worldSwitchDataSO.WorldTypeAStatueMat;
							}
							break;
                        case WorldSwitchReceiverType.Background: recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeABackgroundMat;
                            break;
                    }
                    break;
                case WorldType.TypeB: 
					switch (worldSwitchReceiverType){
                        case WorldSwitchReceiverType.Platform: recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeBMat;
                            break;
                        case WorldSwitchReceiverType.Sky: recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeBSkyBoxMat;
                            break;
                        case WorldSwitchReceiverType.Statue:
							if (recieverMeshRenderer != null) { 
								recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeBStatueMat;
							}
                            if (skinnedMeshRenderer != null)
                            {
                                skinnedMeshRenderer.material = worldSwitchDataSO.WorldTypeBStatueMat;
                            }
                            break;
                        case WorldSwitchReceiverType.Background: recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeBBackgroundMat;
                            break;
                    }
                    break;
            }
        }
    }

    private void WorldSwitchTriggered(WorldType worldType){
        switch (worldSwitchReceiverType){
            case WorldSwitchReceiverType.Platform:
						if(worldType == tiedWorldType){
							recieverCollider.enabled = true;
							recieverMeshRenderer.enabled = true;
						}
						else{
							recieverCollider.enabled = false;
							recieverMeshRenderer.enabled = false;
						}
                break;
            case WorldSwitchReceiverType.Sky or WorldSwitchReceiverType.Statue or WorldSwitchReceiverType.Background:
						UpdateMaterialOnWorldType(worldType);
                break;
        }
    }
}