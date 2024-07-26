using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour{
	[Header("Health Variables")]
	[SerializeField] private int maxHealth;

	[Header("Unity Events")]
	public UnityEvent OnDamaged;
	public UnityEvent OnHealed;
	public UnityEvent OnMaxHealthIncreased;
	public UnityEvent OnDeath;

	public Action OnDeathAction;
	public Action<int> OnDamagedAction;
	public Action<int> OnResetAction;
	public Action<int> OnMaxHealthIncreaseAction;

	private int currentHealth;

	private void Awake(){
		currentHealth = maxHealth;
	}

	public void TakeDamage(int damage, Vector3 damagePoint){
		currentHealth -= damage;
		if(currentHealth <= 0){
			OnDeathAction?.Invoke();
			OnDeath?.Invoke();
			return;
		}

		OnDamagedAction?.Invoke(damage);
		OnDamaged?.Invoke();
	}

	public void HealHealth(int amount){
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
		OnResetAction?.Invoke(currentHealth);
	}

	public void SetMaxHealth(int amount){
		maxHealth = amount;
		OnMaxHealthIncreaseAction?.Invoke(amount);
		OnMaxHealthIncreased?.Invoke();
	}

	public int GetCurrentHealth(){
		return currentHealth;
	}

	public int GetMaxHealth(){
		return maxHealth;
	}

	public bool IsHealthFull(){
		return currentHealth == maxHealth;
	}

	public bool IsOneHit(){
		return currentHealth == 1;
	}
}