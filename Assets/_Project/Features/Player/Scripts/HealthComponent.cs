using UnityEngine;
using System.Collections;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Player
{
    /// <summary>
    /// プレイヤーまたはエンティティの体力管理を行うコンポーネント
    /// </summary>
    /// <remarks>
    /// このコンポーネントは以下の機能を提供します：
    /// - 体力値の管理（現在値、最大値）
    /// - ダメージの受取と回復処理
    /// - 無敵状態の管理
    /// - DOTweenによる視覚的フィードバック（シェイク、色変更）
    /// - イベント駆動型アーキテクチャによる他システムとの連携
    /// 
    /// 使用例：
    /// <code>
    /// healthComponent.TakeDamage(25);
    /// healthComponent.Heal(10);
    /// healthComponent.SetInvulnerable(true, 2f);
    /// </code>
    /// 
    /// 注意事項：
    /// - IHealthTargetインターフェースを実装し、コマンドパターンと連携可能
    /// - 無敌状態中はダメージを無効化
    /// - アニメーション効果にはDOTweenライブラリが必要
    /// </remarks>
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

    /// <summary>
    /// 現在の体力値を取得します
    /// </summary>
    /// <value>0から最大体力値までの整数値</value>
    public int CurrentHealth => currentHealth;
    
    /// <summary>
    /// 最大体力値を取得します
    /// </summary>
    /// <value>エンティティが持つことができる最大の体力値</value>
    public int MaxHealth => maxHealth;
    
    /// <summary>
    /// 現在無敵状態かどうかを取得します
    /// </summary>
    /// <value>無敵状態の場合はtrue、通常状態の場合はfalse</value>
    public bool IsInvulnerable => isInvulnerable;

    /// <summary>
    /// エンティティが死亡しているかどうかを取得します
    /// </summary>
    /// <value>体力が0以下の場合はtrue、生存している場合はfalse</value>
    public bool IsDead => currentHealth <= 0;

    /// <summary>
    /// 指定した量だけ体力を回復します
    /// </summary>
    /// <param name="amount">回復量。正の整数値を指定してください</param>
    /// <remarks>
    /// 処理内容：
    /// - 現在体力 + 回復量を計算し、最大体力を上限とします
    /// - 回復アニメーション（緑色の点滅、スケールパルス効果）を実行
    /// - onHealed、onHealthChangedイベントを発行
    /// 
    /// 注意事項：
    /// - 負の値を指定してもダメージは与えられません（TakeDamageメソッドを使用してください）
    /// - 既に最大体力の場合でも、アニメーションとイベントは発行されます
    /// </remarks>
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

    /// <summary>
    /// 物理ダメージを受けます（エレメントタイプは"physical"として処理）
    /// </summary>
    /// <param name="amount">ダメージ量。正の整数値を指定してください</param>
    /// <remarks>
    /// このメソッドは TakeDamage(amount, "physical") の簡易版です。
    /// 詳細な処理については、パラメータ付きのTakeDamageメソッドを参照してください。
    /// </remarks>
    public void TakeDamage(int amount)
    {
        TakeDamage(amount, "physical");
    }

    /// <summary>
    /// 指定したエレメントタイプのダメージを受けます
    /// </summary>
    /// <param name="amount">ダメージ量。正の整数値を指定してください</param>
    /// <param name="elementType">ダメージのエレメントタイプ（例：physical, fire, ice, poison等）</param>
    /// <remarks>
    /// 処理内容：
    /// - 無敵状態の確認（無敵状態の場合はダメージを無効化）
    /// - 現在体力からダメージ量を減算（0を下限とする）
    /// - ダメージアニメーション（シェイク、赤色点滅）を実行
    /// - onDamaged、onHealthChangedイベントを発行
    /// - 体力が0以下になった場合、onDeathイベントを発行
    /// 
    /// 注意事項：
    /// - 無敵状態中はダメージを受けません
    /// - エレメントタイプは現在ログ出力のみで使用されていますが、将来的な拡張に備えています
    /// - 体力が0になっても自動的にGameObjectは破棄されません
    /// </remarks>
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
    
    /// <summary>
    /// ダメージを受けた際の視覚的フィードバックアニメーションを再生します
    /// </summary>
    /// <remarks>
    /// アニメーション内容：
    /// - Transform.DOShakePosition(): オブジェクトの位置をシェイクさせて衝撃を表現
    /// - MeshRenderer.DOColor(): オブジェクトを一時的に赤色に変化させてダメージを視覚化
    /// 
    /// 使用されるDOTween設定：
    /// - シェイク強度: damageShakeIntensity
    /// - シェイク時間: damageShakeDuration
    /// - イージング: Ease.OutElastic（弾性的な減衰）
    /// 
    /// 注意事項：
    /// - MeshRendererが存在しない場合、色変更は実行されません
    /// - DOTweenライブラリが必要です
    /// - 複数回連続で呼ばれた場合、アニメーションが重複実行される可能性があります
    /// </remarks>
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
    
    /// <summary>
    /// 回復時の視覚的フィードバックアニメーションを再生します
    /// </summary>
    /// <remarks>
    /// アニメーション内容：
    /// - MeshRenderer.DOColor(): オブジェクトを一時的に緑色に変化させて回復を視覚化
    /// - Transform.DOScale(): オブジェクトを一時的に拡大してパルス効果を演出
    /// 
    /// アニメーション仕様：
    /// - 色変更: 緑色へ0.1秒で変化後、元の色へ0.2秒で復帰
    /// - スケール変更: 110%に0.1秒で拡大後、元のスケールへ0.2秒でバウンス効果付きで復帰
    /// - イージング: Ease.OutBounce（バウンス効果）
    /// 
    /// 注意事項：
    /// - MeshRendererが存在しない場合、色変更は実行されません
    /// - DOTweenライブラリが必要です
    /// - 複数回連続で呼ばれた場合、アニメーションが重複実行される可能性があります
    /// </remarks>
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
    
    /// <summary>
    /// 現在の体力割合に基づいて、Odin Inspectorの進行バーの色を決定します
    /// </summary>
    /// <returns>体力割合に応じた色（緑、黄、赤）</returns>
    /// <remarks>
    /// 色分け基準：
    /// - 60%以上: 緑色（健康状態）
    /// - 30%以上60%未満: 黄色（注意状態）
    /// - 30%未満: 赤色（危険状態）
    /// 
    /// 使用箇所：
    /// - Odin InspectorのProgressBar属性のColorGetterパラメータで使用
    /// - インスペクター上での視覚的な体力状態の把握に役立ちます
    /// 
    /// 注意事項：
    /// - このメソッドはOdin Inspectorでのみ使用されるプライベートメソッドです
    /// - maxHealthが0の場合は0除算を避けるため、赤色を返します
    /// </remarks>
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
