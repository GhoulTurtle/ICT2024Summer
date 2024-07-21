using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour{
	[Header("UI References")]
	[SerializeField] private PlayerFirstCamLook playerFirstCamLook;
	[SerializeField] private Slider mouseSensSlider;

	private const string MOUSE_SENS = "MouseSens";

	private void Awake() {
		if(mouseSensSlider != null){
			if(PlayerPrefs.HasKey(MOUSE_SENS)){
				mouseSensSlider.value = PlayerPrefs.GetFloat(MOUSE_SENS);
			}
			else{
				mouseSensSlider.value = 15f;
			}
		}
	}

	public void SetMouseSens(float sens){
		PlayerPrefs.SetFloat(MOUSE_SENS, sens);
		PlayerPrefs.Save();

		if(playerFirstCamLook != null){
			playerFirstCamLook.UpdateCameraSens();
		}
	}
}