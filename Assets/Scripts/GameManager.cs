using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // ★ 1. using は必要
using Random = UnityEngine.Random; 

public class GameManager : MonoBehaviour
{
    [Header("Buff Database")]
    public List<BuffData> allBuffDatabase; 

    [Header("Required Components")]
    public UpgradeUIManager upgradeUI; 

    // --- 内部変数 ---
    private int currentEnemyCount;
    private PlayerStats playerStats;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // 1. シングルトン設定
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; 
        }
        Instance = this;

        // 2. このオブジェクトをシーン移動で破棄しない
        DontDestroyOnLoad(gameObject);
    }

void Start()
    {
        // 1. PlayerStatsへの参照を取得
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("シーンに PlayerStats が見つかりません！");
        }

        // 2. シーンがロードされた時に実行するイベントを購読
        SceneManager.sceneLoaded += OnSceneLoaded; 
        
        // 3. 最初のシーン（Stage1）の情報をロード
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // --- ★ 3. おそらく、このメソッドがまるごと抜けています ---
    /// <summary>
    /// シーンがロードされるたびに自動的に呼び出されるメソッド
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ★ 1. まずゲームを一時停止！
        Time.timeScale = 0f;

        // 2. このシーンの「UpgradeUIManager」を探す
        upgradeUI = FindFirstObjectByType<UpgradeUIManager>(); 
        if (upgradeUI == null)
        {
            Debug.LogWarning("このシーン(" + scene.name + ")には UpgradeUIManager がありません。");
        }
        else
        {
            upgradeUI.gameObject.SetActive(false); // UIを隠しておく
        }

        // 3. このシーンの「StageStartDisplay」を探す
        StageStartDisplay stageStartUI = FindFirstObjectByType<StageStartDisplay>();
        if (stageStartUI != null)
        {
            // ★ 4. ステージ開始UIにシーケンス開始を命令
            stageStartUI.gameObject.SetActive(true);
            stageStartUI.StartSequence(scene.name); // (シーン名をそのまま "Stage1" のように表示)
        }
        else
        {
            // ★ 5. もし開始UIがなければ、即座にゲームを開始
            Debug.LogWarning("このシーン(" + scene.name + ")には StageStartDisplay がありません。");
            Time.timeScale = 1f; 
        }

        // 6. 新しいシーンの敵の数を数える
        currentEnemyCount = FindObjectsByType<Opponent>(FindObjectsSortMode.None).Length;
        Debug.Log(scene.name + " の敵の数: " + currentEnemyCount);
    }

    // --- ★ 4. このメソッドも必要です ---
    /// <summary>
    /// オブジェクトが破棄される時（ゲーム終了時など）に呼ばれる
    /// </summary>
    void OnDestroy()
    {
        // メモリリーク防止のため、イベントの予約を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    /// <summary>
    /// 敵が倒された時に Opponent.cs から呼ばれる
    /// </summary>
    public void OnEnemyDefeated()
    {
        currentEnemyCount--;

        if (currentEnemyCount <= 0)
        {
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

        // (重複なしのロジック...)
        BuffData choice1, choice2, choice3;

        if (allBuffDatabase.Count >= 3)
        {
            List<BuffData> availableBuffs = new List<BuffData>(allBuffDatabase);

            int index1 = Random.Range(0, availableBuffs.Count);
            choice1 = availableBuffs[index1];
            availableBuffs.RemoveAt(index1); 

            int index2 = Random.Range(0, availableBuffs.Count);
            choice2 = availableBuffs[index2];
            availableBuffs.RemoveAt(index2);

            int index3 = Random.Range(0, availableBuffs.Count);
            choice3 = availableBuffs[index3];
        }
        else
        {
            choice1 = allBuffDatabase[Random.Range(0, allBuffDatabase.Count)];
            choice2 = allBuffDatabase[Random.Range(0, allBuffDatabase.Count)];
            choice3 = allBuffDatabase[Random.Range(0, allBuffDatabase.Count)];
        }
        
        if (upgradeUI != null && playerStats != null)
        {
            upgradeUI.DisplayChoices(playerStats, choice1, choice2, choice3);
        }
        else if (upgradeUI == null)
        {
            Debug.LogError("UpgradeUI が null のため、選択肢を表示できません。");
        }
    }

    public void OnEnemySpawned()
    {
        currentEnemyCount++;
    }
}