using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
	[SerializeField]
	private Enemy thisEnemy;

	private Slider sliderUI;

	private void Awake()
	{
		sliderUI = GetComponentInChildren<Slider>();
		if (thisEnemy != null)
		{
			thisEnemy.HealthChangeEvent += OnThisEnemyHealthChange;
		}
	}

	private void Start()
	{
		if (thisEnemy != null)
		{
			OnThisEnemyHealthChange(thisEnemy, thisEnemy.Health, thisEnemy.MaxHealth);
		}
	}

	private void OnDestroy()
	{
		if (thisEnemy != null)
		{
			thisEnemy.HealthChangeEvent -= OnThisEnemyHealthChange;
		}
	}

	private void OnThisEnemyHealthChange(Enemy enemy, int health, int maxHealth)
	{
		float value = Mathf.InverseLerp(0f, maxHealth, health);
		sliderUI.value = value;
	}
}
