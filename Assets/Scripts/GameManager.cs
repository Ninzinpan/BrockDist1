using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random; 

public class GameManager : MonoBehaviour
{
    // ★ 1. 全バフリストは削除
    // public List<BuffData> allBuffDatabase; 

    [Header("Required Components")]
    public UpgradeUIManager upgradeUI; // 永続UIへの参照

    // --- 内部変数 ---
    private int currentEnemyCount;
    private PlayerStats playerStats;
    private StageData currentStageData; // ★ 2. 現在のステージ設計図を保持

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("シーンに PlayerStats が見つかりません！");
        }

        // シーンがロードされた時に実行するイベントを購読
        SceneManager.sceneLoaded += OnSceneLoaded; 
        
        // 最初のシーン（Stage1）の情報をロード
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        // イベント購読を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーンがロードされるたびに呼び出されるメソッド
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ★ 3. 新しいシーンの SceneDataLink を探す
        SceneDataLink dataLink = FindFirstObjectByType<SceneDataLink>();
        if (dataLink != null)
        {
            // 発見したら、現在のステージ設計図として保持
            currentStageData = dataLink.currentStageData;
        }
        else
        {
            Debug.LogError("このシーン(" + scene.name + ")に SceneDataLink がありません！");
            currentStageData = null; // 安全のため null に
        }

        // 4. アップグレードUIへの参照を取得（永続化UIの場合）
        if (upgradeUI == null) // まだ参照がなければ探す
        {
            upgradeUI = FindFirstObjectByType<UpgradeUIManager>(); 
        }
        if (upgradeUI != null)
        {
            upgradeUI.gameObject.SetActive(false); // UIを隠しておく
        }

        // 5. ステージ開始UIを探して起動
        Time.timeScale = 0f;
        StageStartDisplay stageStartUI = FindFirstObjectByType<StageStartDisplay>();
        if (stageStartUI != null)
        {
            stageStartUI.gameObject.SetActive(true);
            
            // ★ 6. StageData の「表示名キー」をそのまま渡す
            // (L10n不採用のため、"Stage 1" などの文字列がそのまま表示される)
            string displayName = (currentStageData != null) ? currentStageData.stageDisplayNameKey : scene.name;
            stageStartUI.StartSequence(displayName);
        }
        else
        {
            Time.timeScale = 1f; 
        }

        // 7. 新しいシーンの敵の数を数える
        currentEnemyCount = FindObjectsByType<Opponent>(FindObjectsSortMode.None).Length;
        Debug.Log(scene.name + " の敵の数: " + currentEnemyCount);
    }

    /// <summary>
    /// 敵が倒された時に Opponent.cs から呼ばれる
    /// </summary>
    public void OnEnemyDefeated()
    {
        currentEnemyCount--;

        if (currentEnemyCount <= 0)
        {

            StartCoroutine(ShowUpgradeChoicesAfterDelay(0.5f));
        }
            
    }

    private IEnumerator ShowUpgradeChoicesAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ShowUpgradeChoices();
    }

    /// <summary>
    /// アップグレードの3択UIを表示する
    /// </summary>
    private void ShowUpgradeChoices()
    {
        // ★ 7. 「全バフリスト」の代わりに「現在のステージのバフリスト」を参照
        if (currentStageData == null || currentStageData.availableBuffs == null || currentStageData.availableBuffs.Count == 0)
        {
            Debug.LogError("現在の StageData にバフが設定されていません！");
            return;
        }

        List<BuffData> buffPool = currentStageData.availableBuffs;
        
        BuffData choice1, choice2, choice3;

        // 登録バフが3種類以上あるか？
        if (buffPool.Count >= 3)
        {
            // 重複なしロジック
            List<BuffData> availableBuffs = new List<BuffData>(buffPool);

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
            // 安全装置：重複を許可
            choice1 = buffPool[Random.Range(0, buffPool.Count)];
            choice2 = buffPool[Random.Range(0, buffPool.Count)];
            choice3 = buffPool[Random.Range(0, buffPool.Count)];
        }
        
        if (upgradeUI != null && playerStats != null)
        {
            upgradeUI.DisplayChoices(playerStats, choice1, choice2, choice3);
        }
    }

    // ★ 8. 新しいメソッドを追加
    /// <summary>
    /// UpgradeUIManager から呼ばれ、次のステージに進む
    /// </summary>
    public void LoadNextStage()
    {
        if (currentStageData != null && !string.IsNullOrEmpty(currentStageData.nextSceneName))
        {
            SceneManager.LoadScene(currentStageData.nextSceneName);
        }
        else
        {
            Debug.LogError("次のステージ名(Next Scene Name)が StageData に設定されていません！");
            // (オプション) 無限ループやタイトルに戻る処理
        }
    }

    public void OnEnemySpawned()
    {
        currentEnemyCount++;
    }
}