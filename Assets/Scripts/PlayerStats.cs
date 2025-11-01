using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseMaxHp = 3f;
    public float baseSpawnInterval = 0.5f;
    public int baseMaxBalls = 10;
    public float baseBallDamage = 1f;
    public float baseSpeed = 10f;

    public float ball_speed = 5f;

    // --- バフ管理台帳 ---
    public Dictionary<BuffData, int> buffLevels = new Dictionary<BuffData, int>();

    // --- 最終的な計算値 ---
    public float CurrentMaxHp { get; private set; }
    public float CurrentSpawnInterval { get; private set; }
    public int CurrentMaxBalls { get; private set; }
    public float CurrentBallDamage { get; private set; }
    public float CurrentSpeed { get; private set; }

    public float CurrentBallSpeed { get; private set; }
    public static PlayerStats Instance { get; private set; }
    // --- イベント ---
    public UnityEvent OnStatsRecalculated;

    // --- 参照 ---
    private PadController padController; // ★ 1. PadController への参照を追加

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
        // 既にPad/PlayerStatsが存在する場合、
        // この（新しくロードされた）Padを破棄
        Destroy(gameObject);
        return; // Awakeの残り処理を中断
        }
    Instance = this;

    // 2. このオブジェクト（Pad）をシーン移動で破棄しない
    DontDestroyOnLoad(gameObject);
        // ★ 2. PadController への参照を取得
        padController = GetComponent<PadController>(); 
        if(padController == null)
        {
            Debug.LogError("PadController が PlayerStats と同じオブジェクトにありません！");
        }
        
        RecalculateStats();
    }

    /// <summary>
    /// バフをプレイヤーに追加する（UpgradeUIなどから呼ばれる）
    /// </summary>
    public void AddBuff(BuffData buffToAdd)
    {
        // ★ 3. 処理を分岐
        
        // A. もし「使い捨て効果」なら
        if (buffToAdd.isOneTimeEffect)
        {
            // ステータスの種類に応じて、即時効果を発動
            switch (buffToAdd.statToBuff)
            {
                case StatType.HealthRecovery:
                    if (padController != null)
                    {
                        padController.Heal(buffToAdd.enhancementValue);
                    }
                    break;
                // (将来的に「所持金ゲット」などもここに追加できる)
            }
        }
        // B. もし「永続強化」なら
        else
        {
            // 台帳(buffLevels)に、追加するバフが既にあるかチェック
            if (buffLevels.ContainsKey(buffToAdd))
            {
                // あればレベルを+1
                buffLevels[buffToAdd]++;
            }
            else
            {
                // なければレベル1として新規追加
                buffLevels.Add(buffToAdd, 1);
            }

            // ステータスを再計算
            RecalculateStats();
        }
    }

    /// <summary>
    /// 全ステータスを基本値から再計算する
    /// </summary>
    private void RecalculateStats()
    {
        // (中身は変更なし)
        CurrentMaxHp = baseMaxHp;
        CurrentSpawnInterval = baseSpawnInterval;
        CurrentMaxBalls = baseMaxBalls;
        CurrentBallDamage = baseBallDamage;
        CurrentSpeed = baseSpeed;
        CurrentBallSpeed = ball_speed;

        foreach (var buffEntry in buffLevels)
        {
            BuffData buff = buffEntry.Key;
            int level = buffEntry.Value;

            switch (buff.statToBuff)
            {
                case StatType.MaxHP:
                    CurrentMaxHp += buff.enhancementValue * level;

                    break;
                case StatType.FireRate:
                    CurrentSpawnInterval += buff.enhancementValue * level;
                    break;
                case StatType.MaxBalls:
                    CurrentMaxBalls += (int)(buff.enhancementValue * level);
                    break;
                case StatType.BallDamage:
                    CurrentBallDamage += buff.enhancementValue * level;
                    break;
                case StatType.Speed:
                    CurrentBallSpeed += buff.enhancementValue * level;
                    break;
            }
        }

        if (CurrentSpawnInterval < 0.05f)
        {
            CurrentSpawnInterval = 0.05f;
        }

        OnStatsRecalculated.Invoke();
    }
    /// <summary>
/// GameManagerから呼ばれ、プレイヤーの状態をリセットする
/// </summary>
public void ResetPlayerState()
{
    // 自分の相棒である PadController にリセットを命令
    padController.ResetState();
}
}