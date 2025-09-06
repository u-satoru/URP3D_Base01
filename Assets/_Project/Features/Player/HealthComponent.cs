using UnityEngine;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

public class HealthComponent : MonoBehaviour, IHealthTarget
{
    [TabGroup("Health", "Settings")]
    [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthColor")]
    [LabelText("Current Health")]
    [SerializeField] private int currentHealth = 100;
    
    [TabGroup("Health", "Settings")]
    [PropertyRange(1, 1000)]
    [LabelText("Max Health")]
    [SuffixLabel("HP", overlay: true)]
    [SerializeField] private int maxHealth = 100;
    
    [TabGroup("Health", "Events")]
    [LabelText("Health Changed")]
    [SerializeField] private GameEvent onHealthChanged;
    
    [TabGroup("Health", "Events")]
    [LabelText("Damaged")]
    [SerializeField] private GameEvent onDamaged;
    
    [TabGroup("Health", "Events")]
    [LabelText("Healed")]
    [SerializeField] private GameEvent onHealed;
    
    [TabGroup("Health", "Events")]
    [LabelText("Death")]
    [SerializeField] private GameEvent onDeath;
    
    [TabGroup("Health", "Animation")]
    [LabelText("Damage Shake Intensity")]
    [PropertyRange(0.1f, 2f)]
    [SerializeField] private float damageShakeIntensity = 0.5f;
    
    [TabGroup("Health", "Animation")]
    [LabelText("Damage Shake Duration")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("s", overlay: true)]
    [SerializeField] private float damageShakeDuration = 0.3f;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // DOTweenで回復アニメーション
        PlayHealAnimation();
        
        onHealed?.Raise();
        onHealthChanged?.Raise();
        
        Debug.Log($"Healed {amount}. Current health: {currentHealth}");
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        
        // DOTweenでダメージアニメーション
        PlayDamageAnimation();
        
        onDamaged?.Raise();
        onHealthChanged?.Raise();
        
        if (currentHealth <= 0)
        {
            onDeath?.Raise();
        }
        
        Debug.Log($"Took {amount} damage. Current health: {currentHealth}");
    }
    
    private void PlayDamageAnimation()
    {
        // シェイクアニメーション
        transform.DOShakePosition(damageShakeDuration, damageShakeIntensity)
            .SetEase(Ease.OutElastic);
        
        // 赤く点滅させる（MeshRendererがある場合）
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var originalColor = renderer.material.color;
            renderer.material.DOColor(Color.red, 0.1f)
                .OnComplete(() => renderer.material.DOColor(originalColor, 0.2f));
        }
    }
    
    private void PlayHealAnimation()
    {
        // 緑に点滅させる（MeshRendererがある場合）
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var originalColor = renderer.material.color;
            renderer.material.DOColor(Color.green, 0.1f)
                .OnComplete(() => renderer.material.DOColor(originalColor, 0.2f));
        }
        
        // スケールでパルス効果
        var originalScale = transform.localScale;
        transform.DOScale(originalScale * 1.1f, 0.1f)
            .OnComplete(() => transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBounce));
    }
    
    // Odin Inspector用のカラーゲッター
    private Color GetHealthColor()
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        return healthPercentage switch
        {
            > 0.6f => Color.green,
            > 0.3f => Color.yellow,
            _ => Color.red
        };
    }
}
