using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Buff Database")]
    public List<BuffData> allBuffDatabase; 

    [Header("Required Components")]
    public UpgradeUIManager upgradeUI; 

    // --- 内部変数 ---
    private int currentEnemyCount;
    private PlayerStats playerStats;

    void Start()
    {
        // 1. PlayerStatsへの参照を取得
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("シーンに PlayerStats が見つかりません！");
        }

        // 2. （暫定対応）スポナーがないため、シーン開始時の敵の数を数える
        
        // --- ★ここが修正点です ---
        // 'FindObjectsOfType<Opponent>().Length' を
        // 'FindObjectsByType<Opponent>(FindObjectsSortMode.None).Length' に変更
        currentEnemyCount = FindObjectsByType<Opponent>(FindObjectsSortMode.None).Length;
        // --- 修正ここまで ---

        Debug.Log("現在のステージの敵の数: " + currentEnemyCount);

        // 3. アップグレードUIが設定されていなければ非表示（エラー防止）
        if (upgradeUI == null)
        {
            Debug.LogError("UpgradeUIManager が GameManager に設定されていません！");
        }
        else
        {
            upgradeUI.gameObject.SetActive(false); // UIを隠しておく
        }
    }

    /// <summary>
    /// 敵が倒された時に Opponent.cs から呼ばれる
    /// </summary>
    public void OnEnemyDefeated()
    {
        currentEnemyCount--;

        // 敵が0になったかチェック
        if (currentEnemyCount <= 0)
        {
            // 敵が全滅したので、アップグレード選択を表示
            ShowUpgradeChoices();
        }
    }

    /// <summary>
    /// アップグレードの3択UIを表示する
    /// </summary>
    private void ShowUpgradeChoices()
    {
        if (allBuffDatabase == null || allBuffDatabase.Count == 0)
        {
            Debug.LogError("GameManager の All Buff Database が空です！");
            return;
        }

        // 2. ランダムに3つのバフを選ぶ（重複OK）
        BuffData choice1 = allBuffDatabase[Random.Range(0, allBuffDatabase.Count)];
        BuffData choice2 = allBuffDatabase[Random.Range(0, allBuffDatabase.Count)];
        BuffData choice3 = allBuffDatabase[Random.Range(0, allBuffDatabase.Count)];

        if (upgradeUI != null && playerStats != null)
        {
            upgradeUI.DisplayChoices(playerStats, choice1, choice2, choice3);
        }
    }

    // （将来的に敵スポナーができた場合、スポナーがこのメソッドを呼ぶ）
    public void OnEnemySpawned()
    {
        currentEnemyCount++;
    }
}