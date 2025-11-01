using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random; 

public class GameManager : MonoBehaviour
{
    [Header("UI References (Must be persistent)")]
    // (Inspectorから設定するのではなく、Start() で探すように変更)
    private UpgradeUIManager upgradeUI; 
    private GameOverUI gameOverUI; // ★ 1. 変数として保持

    // --- 内部変数 ---
    private int currentEnemyCount;
    private PlayerStats playerStats;
    private StageData currentStageData; 
    private bool isGameOver = false;

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
        // --- ★ 2. 永続化オブジェクトへの参照を「最初」に1回だけ取得 ---
        playerStats = PlayerStats.Instance;
        upgradeUI = UpgradeUIManager.Instance; // (UpgradeUIManagerもシングルトン前提)
        gameOverUI = GameOverUI.Instance; // (GameOverUIもシングルトン前提)
        
        if (playerStats == null) Debug.LogError("PlayerStats が見つかりません！");
        if (upgradeUI == null) Debug.LogError("UpgradeUIManager が見つかりません！");
        if (gameOverUI == null) Debug.LogError("GameOverUI が見つかりません！");
        // ---

        SceneManager.sceneLoaded += OnSceneLoaded; 
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーンがロードされるたびに呼び出されるメソッド
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameOver = false; // ★ 3. ゲームオーバー状態をリセット
        
        // 4. 永続化UIを隠す
        if (upgradeUI != null) upgradeUI.gameObject.SetActive(false);
        if (gameOverUI != null) gameOverUI.Hide(); // ★ 4. GameOverUIを隠す

        // 5. 現在のステージの設計図を取得
        SceneDataLink dataLink = FindFirstObjectByType<SceneDataLink>();
        if (dataLink != null)
        {
            currentStageData = dataLink.currentStageData;
        }
        else
        {
            Debug.LogError("このシーン(" + scene.name + ")に SceneDataLink がありません！");
            currentStageData = null;
        }

        // 6. ステージ開始UIを探して起動
        Time.timeScale = 0f;
        StageStartDisplay stageStartUI = FindFirstObjectByType<StageStartDisplay>();
        if (stageStartUI != null)
        {
            stageStartUI.gameObject.SetActive(true);
            string displayName = (currentStageData != null) ? currentStageData.stageDisplayNameKey : scene.name;
            stageStartUI.StartSequence(displayName);
        }
        else
        {
            Time.timeScale = 1f; 
        }

        // 7. 新しいシーンの敵の数を数える
        currentEnemyCount = FindObjectsByType<Opponent>(FindObjectsSortMode.None).Length;
    }

    /// <summary>
    /// PadController から呼ばれ、ゲームオーバー処理を開始する
    /// </summary>
    public void StartGameOverSequence()
    {
        if (isGameOver) return;
        isGameOver = true;



        // 永続化されている GameOverUI を表示
        if (gameOverUI != null)
        {
            gameOverUI.Show();
        }
        else
        {
            Debug.LogError("GameOverUI が見つかりません！");
        }
    }

    /// <summary>
    /// GameOverUI から呼ばれ、ステージをリスタートする
    /// </summary>
    public void RestartStage()
    {
        // 1. PlayerStats (と Pad) の状態をリセット
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetPlayerState();
        }

        // 2. 現在のシーン名を再度読み込む
        // (OnSceneLoaded が自動で呼ばれ、UIが隠されたり敵がカウントされる)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // (OnEnemyDefeated, ShowUpgradeChoices, LoadNextStage, OnEnemySpawned は変更なし)
    
    public void OnEnemyDefeated()
    {
        if (isGameOver) return; // ゲームオーバー後はカウントしない
        currentEnemyCount--;
        if (currentEnemyCount <= 0) ShowUpgradeChoices();
    }
    
    private void ShowUpgradeChoices()
    {
        if (currentStageData == null || currentStageData.availableBuffs == null || currentStageData.availableBuffs.Count == 0)
        {
            Debug.LogError("現在の StageData にバフが設定されていません！");
            return;
        }
        List<BuffData> buffPool = currentStageData.availableBuffs;
        BuffData choice1, choice2, choice3;
        if (buffPool.Count >= 3)
        {
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
            choice1 = buffPool[Random.Range(0, buffPool.Count)];
            choice2 = buffPool[Random.Range(0, buffPool.Count)];
            choice3 = buffPool[Random.Range(0, buffPool.Count)];
        }
        if (upgradeUI != null && playerStats != null)
        {
            upgradeUI.DisplayChoices(playerStats, choice1, choice2, choice3);
        }
    }
    
    public void LoadNextStage()
    {
        if (currentStageData != null && !string.IsNullOrEmpty(currentStageData.nextSceneName))
        {
            SceneManager.LoadScene(currentStageData.nextSceneName);
        }
        else
        {
            Debug.LogError("次のステージ名(Next Scene Name)が StageData に設定されていません！");
        }
    }
    
    public void OnEnemySpawned()
    {
        currentEnemyCount++;
    }
}