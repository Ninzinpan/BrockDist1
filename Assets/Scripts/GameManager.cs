using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random; 

public class GameManager : MonoBehaviour
{
    [Header("UI References (Persistent)")]
    private UpgradeUIManager upgradeUI; 
    private GameOverUI gameOverUI; 
    // private GameClearUI gameClearUI; // ★ 1. クリアUIへの参照を削除

    [Header("XP Scaling")]
    public int baseKillsNeeded = 10;
    public int additionalKillsPerLevel = 5;

    // --- 内部変数 ---
    private PlayerStats playerStats;
    private StageData currentStageData; 
    private bool isGameFinished = false; // ゲーム終了フラグ

    // --- カウンター ---
    private int currentKillCount;  
    private int currentEnemyCount; // ★ 2. ステージクリア用のカウンター
    private int upgradeCount; 
    private int killsNeededForLevelUp;
    private Slider xpBarSlider;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameFinished = false; 
        
        if (upgradeUI != null) upgradeUI.gameObject.SetActive(false);
        if (gameOverUI != null) gameOverUI.Hide(); 
        // if (gameClearUI != null) gameClearUI.Hide(); // ★ 3. 削除

        // (StageDataLink を探す処理...)
        SceneDataLink dataLink = FindFirstObjectByType<SceneDataLink>();
        if (dataLink != null) currentStageData = dataLink.currentStageData;
        else Debug.LogError("このシーン(" + scene.name + ")に SceneDataLink がありません！");

        // (StageStartDisplay を探す処理...)
        Time.timeScale = 0f;
        StageStartDisplay stageStartUI = FindFirstObjectByType<StageStartDisplay>();
        if (stageStartUI != null)
        {
            stageStartUI.gameObject.SetActive(true);
            string displayName = (currentStageData != null) ? currentStageData.stageDisplayNameKey : scene.name;
            stageStartUI.StartSequence(displayName);
        }
        else Time.timeScale = 1f; 

        // XPシステムを初期化
        currentKillCount = 0;
        upgradeCount = 0; 
        killsNeededForLevelUp = baseKillsNeeded;

        // 「残り敵数」のカウンターも初期化
        currentEnemyCount = FindObjectsByType<Opponent>(FindObjectsSortMode.None).Length;

        // (XPバーを探す処理...)
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
        else Debug.LogWarning("このシーンに 'XPBar' タグのオブジェクトがありません");
    }

    /// <summary>
    /// 敵が倒された時に Opponent.cs から呼ばれる
    /// </summary>
    public void OnEnemyDefeated()
    {
        if (isGameFinished) return; 

        // 1. XP処理
        currentKillCount++;
        if (xpBarSlider != null) xpBarSlider.value = currentKillCount;

        if (currentKillCount >= killsNeededForLevelUp)
        {
            ShowUpgradeChoices(); 
            currentKillCount = 0; 
            upgradeCount++; 
            killsNeededForLevelUp = baseKillsNeeded + (additionalKillsPerLevel * upgradeCount);
            if (xpBarSlider != null)
            {
                xpBarSlider.maxValue = killsNeededForLevelUp;
                xpBarSlider.value = currentKillCount;
            }
        }

        // --- ★ 4. 全滅処理を修正 ---
        currentEnemyCount--;
        if (currentEnemyCount <= 0)
        {
            // 敵が全滅した
            isGameFinished = true; // ゲーム終了
            Time.timeScale = 0f; // 時間を止める
            ReturnToHome(); // 即座にホームに戻る
        }
    }
    
    // (ShowUpgradeChoices は変更なし)
    private void ShowUpgradeChoices()
    {
        // (中身は同じ)
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
            upgradeUI.gameObject.SetActive(true); 
            upgradeUI.DisplayChoices(playerStats, choice1, choice2, choice3);
        }
    }

    // (GameOver関連のメソッドは変更なし)
    public void StartGameOverSequence()
    {
        if (isGameFinished) return; 
        isGameFinished = true; 
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Show();
        }
        else Debug.LogError("GameOverUI が見つかりません！");
    }

    public void RestartStage()
    {
        Time.timeScale = 1f;
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetPlayerState();
        }
        if (BGMPlayer.Instance != null)
        {
            BGMPlayer.Instance.PlayMusic();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- ★ 5. ReturnToHome メソッドを修正 ---
    /// <summary>
    /// ゲームクリア時（またはUIから）タイトルに戻る
    /// </summary>
    public void ReturnToHome()
    {
        Time.timeScale = 1f; // 時間を戻す

        // BGMを止める
        if (BGMPlayer.Instance != null)
        {
            BGMPlayer.Instance.StopMusic();
        }

        // すべての永続化オブジェクトを破棄
        if (PlayerStats.Instance != null) Destroy(PlayerStats.Instance.gameObject);
        if (PersistentBuffUI.Instance != null) Destroy(PersistentBuffUI.Instance.gameObject);
        if (UpgradeUIManager.Instance != null) Destroy(UpgradeUIManager.Instance.gameObject);
        if (GameOverUI.Instance != null) Destroy(GameOverUI.Instance.gameObject);
        // if (GameClearUI.Instance != null) Destroy(GameClearUI.Instance.gameObject); // 削除
        if (LocalizationManager.Instance != null) Destroy(LocalizationManager.Instance.gameObject);
        if (BGMPlayer.Instance != null) Destroy(BGMPlayer.Instance.gameObject);
        
        // 最後に自分自身を破棄
        Destroy(gameObject);

        // タイトルシーン（SampleScene）をロード
        SceneManager.LoadScene("SampleScene");
    }
    
    public void OnEnemySpawned()
    {
        // (OnSceneLoaded でカウント)
    }
}