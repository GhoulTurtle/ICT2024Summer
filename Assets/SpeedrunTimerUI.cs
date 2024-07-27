using System.Collections;
using TMPro;
using UnityEngine;

public class SpeedrunTimerUI : MonoBehaviour{
	[Header("UI References")]
	[SerializeField] private TextMeshProUGUI speedrunTimerText;

	[Header("Speedrun Timer Variables")]
	[SerializeField] private bool startTimerOnStart;

	private float timePassed = 0f;

	private IEnumerator currentTimerCoroutine;

	private void Start(){
		if(startTimerOnStart){
			StartTimer();
		}
	}

	private void OnDestroy() {
		StopAllCoroutines();
	}

	public void StartTimer(){
		currentTimerCoroutine = SpeedrunTimerCoroutine();
		StartCoroutine(currentTimerCoroutine);
	}

	public void StopTimer(){
		if(currentTimerCoroutine != null){
			StopCoroutine(currentTimerCoroutine);
			currentTimerCoroutine = null;
		}
	}

	public float GetCurrentTime(){
		return timePassed;
	}

	public void ResetTimer(){
		StopTimer();
		timePassed = 0f;
		StartTimer();
	}

	private IEnumerator SpeedrunTimerCoroutine(){
		while(true){
			timePassed += Time.deltaTime;

			int milliseconds = (int)(timePassed * 1000 % 1000);
			int seconds = (int)timePassed % 60;
			int minutes = (int)timePassed / 60;
			int hours = minutes / 60;

			speedrunTimerText.text = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", hours, minutes, seconds, milliseconds / 10);
			yield return null;
		}
	}
}