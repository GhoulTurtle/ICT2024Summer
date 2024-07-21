using UnityEngine;
using UnityEngine.Events;

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

	public UnityEvent WorldAEvent;
    public UnityEvent WorldBEvent;

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
                        case WorldSwitchReceiverType.Platform:
							if (isTied && tiedWorldType != WorldType.TypeA){
								recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeAFadeMat;
							}
							else { 
								recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeAMat;
							}
                            break;
                        case WorldSwitchReceiverType.Sky:
                            RenderSettings.skybox = worldSwitchDataSO.WorldTypeASkyBoxMat;
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

                    WorldAEvent?.Invoke();
                    break;
                case WorldType.TypeB: 
					switch (worldSwitchReceiverType){
                        case WorldSwitchReceiverType.Platform:
							if (isTied && tiedWorldType != WorldType.TypeB)
							{
								recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeBFadeMat;
							}
							else { 
								recieverMeshRenderer.material = worldSwitchDataSO.WorldTypeBMat;
							}
                            break;
                        case WorldSwitchReceiverType.Sky: 
							RenderSettings.skybox = worldSwitchDataSO.WorldTypeBSkyBoxMat;
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

					WorldBEvent?.Invoke();
                    break;
            }
        }
    }

    private void WorldSwitchTriggered(WorldType worldType){
        switch (worldSwitchReceiverType){
            case WorldSwitchReceiverType.Platform:
						if(worldType == tiedWorldType){
							recieverCollider.enabled = true;
						}
						else{
							recieverCollider.enabled = false;
						}
                break;
        }

        UpdateMaterialOnWorldType(worldType);
    }
}