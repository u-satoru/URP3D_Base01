using UnityEngine;
using System.Collections;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Player
{
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
    
    [TabGroup("Health", "Settings")]
    [LabelText("Invulnerable")]
    [ShowInInspector, ReadOnly]
    private bool isInvulnerable = false;
    
    private Coroutine invulnerabilityCoroutine;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => isInvulnerable;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // DOTweenで回復アニメーション
        PlayHealAnimation();
        
        onHealed?.Raise();
        onHealthChanged?.Raise();
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"Healed {amount}. Current health: {currentHealth}");
#endif
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, "physical");
    }

    public void TakeDamage(int amount, string elementType)
    {
        // 無敵状態の場合はダメージを受けない
        if (isInvulnerable)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Damage blocked due to invulnerability: {amount} {elementType}");
#endif
            return;
        }
        
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        
        // DOTweenでダメージアニメーション
        PlayDamageAnimation();
        
        onDamaged?.Raise();
        onHealthChanged?.Raise();
        
        if (currentHealth <= 0)
        {
            onDeath?.Raise();
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"Took {amount} {elementType} damage. Current health: {currentHealth}");
#endif
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
    
    /// <summary>
    /// 無敵状態を設定します
    /// </summary>
    /// <param name="invulnerable">無敵状態にするかどうか</param>
    /// <param name="duration">無敵時間（秒）。0以下の場合は手動で解除するまで継続</param>
    public void SetInvulnerable(bool invulnerable, float duration = 0f)
    {
        isInvulnerable = invulnerable;
        
        // 既存の無敵時間コルーチンがあればストップ
        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
            invulnerabilityCoroutine = null;
        }
        
        // 無敵状態で継続時間が指定されている場合
        if (invulnerable && duration > 0f)
        {
            invulnerabilityCoroutine = StartCoroutine(InvulnerabilityTimer(duration));
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"Invulnerability set to: {invulnerable}" + (duration > 0f ? $" for {duration} seconds" : ""));
#endif
    }
    
    /// <summary>
    /// 無敵時間の管理コルーチン
    /// </summary>
    private System.Collections.IEnumerator InvulnerabilityTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
        invulnerabilityCoroutine = null;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Invulnerability expired");
#endif
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
}
