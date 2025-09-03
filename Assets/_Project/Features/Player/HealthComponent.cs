using UnityEngine;
using asterivo.Unity60.Core.Components;

public class HealthComponent : MonoBehaviour, IHealthTarget
{
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int maxHealth = 100;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Healed {amount}. Current health: {currentHealth}");
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        Debug.Log($"Took {amount} damage. Current health: {currentHealth}");
    }
}
