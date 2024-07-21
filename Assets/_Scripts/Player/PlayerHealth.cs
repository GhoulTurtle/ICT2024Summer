using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour{
	[Header("Health Variables")]
	[SerializeField] private float maxHealth;

	[Header("Unity Events")]
	public UnityEvent OnDamaged;
	public UnityEvent OnHealed;
	public UnityEvent OnMaxHealthIncreased;
	public UnityEvent OnDeath;

	public Action OnDeathAction;
	public Action<float> OnDamagedAction;

	private float currentHealth;

	private void Start(){
		currentHealth = maxHealth;
	}

	public void TakeDamage(float damage, Vector3 damagePoint){
		currentHealth -= damage;
		if(currentHealth <= 0){
			OnDeathAction?.Invoke();
			OnDeath?.Invoke();
			return;
		}

		OnDamagedAction?.Invoke(damage);
		OnDamaged?.Invoke();
	}

	public void HealHealth(float amount){
		currentHealth += amount;
		if(currentHealth > maxHealth) currentHealth = maxHealth;
		OnHealed?.Invoke();
	}

	public void TriggerInstantDeath(){
		OnDeathAction?.Invoke();
		OnDeath?.Invoke();
	}

	public void ResetHealth(){
		currentHealth = maxHealth;
	}

	public void SetMaxHealth(float amount){
		maxHealth = amount;
		OnMaxHealthIncreased?.Invoke();
	}

	public bool IsHealthFull(){
		return currentHealth == maxHealth;
	}
}
