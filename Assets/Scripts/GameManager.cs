using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ★ 1. Slider を使うために必要
using Random = UnityEngine.Random; 

public class GameManager : MonoBehaviour
{
    [Header("UI References (Persistent)")]
    private UpgradeUIManager upgradeUI; 
    private GameOverUI gameOverUI; 

    // --- ★ 2. ここから XP の設定を追加 ---
    [Header("XP Scaling")]
    public int baseKillsNeeded = 10; // 最初のレベルアップに必要な討伐数
    public int additionalKillsPerLevel = 5; // レベルアップごとに追加で必要になる討伐数
    // --- 追加ここまで ---

    // --- 内部変数 ---
    private PlayerStats playerStats;
    private StageData currentStageData; 
    private bool isGameOver = false;

    // --- ★ 3. XP関連の内部変数を追加 ---
    private int currentKillCount;  // 現在の討伐数（XP）
    private int killsNeededForLevelUp; // 次のレベルアップに必要な討伐数
    private int upgradeCount; // アップグレード（レベルアップ）した回数
    
    private Slider xpBarSlider; // ★ 4. XPバーのSliderへの参照
    // ---

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
        // 永続化オブジェクトへの参照を「最初」に1回だけ取得
        playerStats = PlayerStats.Instance;
        upgradeUI = UpgradeUIManager.Instance; 
        gameOverUI = GameOverUI.Instance; 
        
        if (playerStats == null) Debug.LogError("PlayerStats が見つかりません！");
        if (upgradeUI == null) Debug.LogError("UpgradeUIManager が見つかりません！");
        if (gameOverUI == null) Debug.LogError("GameOverUI が見つかりません！");

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
        isGameOver = false; 
        
        // 永続化UIを隠す
        if (upgradeUI != null) upgradeUI.gameObject.SetActive(false);
        if (gameOverUI != null) gameOverUI.Hide(); 

        // ステージ設計図を取得
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

        // ステージ開始UIを起動
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

        // --- ★ 5. 敵の数ではなく、XPシステムを初期化 ---
        currentKillCount = 0;
        upgradeCount = 0; 
        killsNeededForLevelUp = baseKillsNeeded;

        // ★ 6. XPバーを探して初期化
        GameObject xpBarGO = GameObject.FindGameObjectWithTag("XPBar");
        if (xpBarGO != null)
        {
            xpBarSlider = xpBarGO.GetComponent<Slider>();
            if (xpBarSlider != null)
            {
                xpBarSlider.maxValue = killsNeededForLevelUp;
                xpBarSlider.value = 0;
            }
        }
        else
        {
            Debug.LogWarning("このシーンに 'XPBar' タグのオブジェクトがありません");
            xpBarSlider = null;
        }
        // ---
    }

    /// <summary>
    /// 敵が倒された時に Opponent.cs から呼ばれる
    /// </summary>
    public void OnEnemyDefeated()
    {
        if (isGameOver) return; // ゲームオーバー後はカウントしない

        // --- ★ 7. 敵カウントダウンから、XPカウントアップ処理に変更 ---

        // 1. 討伐数（XP）を加算
        currentKillCount++;

        // 2. XPバーのUIを更新
        if (xpBarSlider != null)
        {
            xpBarSlider.value = currentKillCount;
        }

        // 3. レベルアップ判定
        if (currentKillCount >= killsNeededForLevelUp)
        {
            // 4. アップグレードUIを表示 (UI側が Time.timeScale = 0 にする)
            ShowUpgradeChoices(); 

            // 5. XPとレベルをリセット・再計算
            currentKillCount = 0; // XPをリセット
            upgradeCount++; // アップグレード回数を増やす

            // 6. 次の目標値を計算（線形増加）
            killsNeededForLevelUp = baseKillsNeeded + (additionalKillsPerLevel * upgradeCount);
            
            // 7. UIの最大値と現在値を更新
            if (xpBarSlider != null)
            {
                xpBarSlider.maxValue = killsNeededForLevelUp;
                xpBarSlider.value = currentKillCount; // 0に戻す
            }
        }
        // --- 
    }
    
    // (StartGameOverSequence, GameOverRoutine, RestartStage は変更なし)
    
    public void StartGameOverSequence()
    {
        if (isGameOver) return; 
        isGameOver = true;
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        if (gameOverUI != null)
        {
            gameOverUI.Show();
        }
        else
        {
            Debug.LogError("GameOverUI が見つかりません！");
        }
    }

    public void RestartStage()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetPlayerState();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // (ShowUpgradeChoices, LoadNextStage, OnEnemySpawned は変更なし)
    
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
        // (このメソッドは現在使われていないが、将来のスポナーのために残しておく)
        // currentEnemyCount++; 
    }
}