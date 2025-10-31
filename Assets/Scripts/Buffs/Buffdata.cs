using UnityEngine;

// Unityエディタの「Create」メニューに項目を追加する
[CreateAssetMenu(fileName = "NewBuff", menuName = "Blockfall Roguelike/Buff Data")]
public class BuffData : ScriptableObject // ★ MonoBehaviour ではない
{
    [Header("UI Display")]
    public string buffName; // UIに表示する名前（例: "連射速度アップ"）
    public Sprite buffIcon; // UIに表示するアイコン

    [Header("Buff Logic")]
    // これが true なら「体力回復」のような使い捨て効果
    // false なら「連射Lv.2」のようにスタック（蓄積）可能な永続強化
    public bool isOneTimeEffect;

    // isOneTimeEffect が false の場合に、どのステータスを強化するか
    public StatType statToBuff;

    // 1レベルごとに、どれだけステータスを強化するか
    public float enhancementValue; 
    // 例:
    //  MaxBalls なら 1 (1個増える)
    //  FireRate なら -0.05 (0.05秒短縮)
    //  BallDamage なら 1 (ダメージ+1)
}

/// <summary>
/// 強化するステータスの種類を定義する「列挙型」
/// </summary>
public enum StatType
{
    // 永続強化
    MaxBalls,       // ボールの最大数
    FireRate,       // 連射速度（発射間隔）
    BallDamage,     // ボールの攻撃力（今後実装用）
    MaxHP,          // 最大HP（今後実装用）
    Speed,

    // 使い捨て効果（isOneTimeEffect = true の時に使う）
    HealthRecovery  // 体力回復
}