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

	private void Awake(){
		currentHealth = maxHealth;
	}

	public void TakeDamage(float damage, Vector3 damagePoint){
		currentHealth -= damage;
		if(currentHealth <= 0){
			OnDeathAction?.Invoke();
			OnDeath?.Invoke();
			Debug.Log("IM DEAD!");
			return;
		}

		Debug.Log("I TOOK DAMAGE");

		OnDamagedAction?.Invoke(damage);
		OnDamaged?.Invoke();
	}

	public void HealHealth(float amount){
		currentHealth += amount;
		if(currentHealth > maxHealth) currentHealth = maxHealth;
		OnHealed?.Invoke();
	}

	public void IncreaseMaxHealth(float amount){
		if(IsHealthFull()){
			currentHealth += amount;
		}
		maxHealth += amount;
		OnMaxHealthIncreased?.Invoke();
	}

	public bool IsHealthFull(){
		return currentHealth == maxHealth;
	}
}
