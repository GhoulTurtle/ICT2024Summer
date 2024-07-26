using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private PlayerHealth playerHealth;
	[SerializeField] private Sprite normalSkullSprite;
	[SerializeField] private Sprite lowSkullSprite;

	[Header("UI References")]
	[SerializeField] private Image skullImage;
	[SerializeField] private Slider healthSlider;
	[SerializeField] private Image healthSliderFill;
	[SerializeField] private HorizontalLayoutGroup sliderHotchParent;
	[SerializeField] private Transform fillHotchPrefab; 

	[Header("Health UI Variables")]
	[SerializeField] private Color sliderFillRegularColor;
	[SerializeField] private Color sliderFillLowColor;

	private int previousMaxHealth;

	private void Start() {
		if(playerHealth == null) return;

		playerHealth.OnDamagedAction += UpdateHealthUI;
		playerHealth.OnResetAction += UpdateHealthUI;
		playerHealth.OnMaxHealthIncreaseAction += HealthIncreased;

		SetupUI();
	}

    private void OnDestroy() {
		if(playerHealth == null) return;

		playerHealth.OnDamagedAction -= UpdateHealthUI;
		playerHealth.OnMaxHealthIncreaseAction -= HealthIncreased;
	}

    private void UpdateHealthUI(int damageAmount){
		healthSlider.value = playerHealth.GetCurrentHealth();

		healthSliderFill.color = playerHealth.IsOneHit() ? sliderFillLowColor : sliderFillRegularColor;
		skullImage.sprite = playerHealth.IsOneHit() ? lowSkullSprite : normalSkullSprite;
    }

	private void HealthIncreased(int totalAdded){
		AddHealthKnotchs(totalAdded - previousMaxHealth);
		previousMaxHealth = totalAdded;
		healthSlider.maxValue = previousMaxHealth;
	}

    private void SetupUI(){
		previousMaxHealth = playerHealth.GetMaxHealth();

		healthSlider.maxValue = previousMaxHealth;

		UpdateHealthUI(0);

		sliderHotchParent.spacing = -160f;

		//If we have 2 health then we want 1 knotch
		AddHealthKnotchs(previousMaxHealth - 1);
    }

	private void AddHealthKnotchs(int amountToAdd){
		for (int i = 0; i < amountToAdd; i++){
			Instantiate(fillHotchPrefab, sliderHotchParent.transform);
			if(sliderHotchParent.transform.childCount >= 3){
				sliderHotchParent.spacing += 30f;
			}
		}
	}
}