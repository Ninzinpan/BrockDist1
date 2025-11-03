using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("References")]
    public Button restartButton; // リスタートボタン
    
    // --- ★ 1. ホームボタン用の変数を追加 ---
    public Button homeButton;    // ホームに戻るボタン

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
        // 3. ボタンにリスナーを設定
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartPressed);
        }
        
        // --- ★ 2. ホームボタンにもリスナーを設定 ---
        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomePressed);
        }
        
        // 4. 起動時は必ず非表示にしておく
        gameObject.SetActive(false);
    }
        
    void OnRestartPressed()
    {
        // 5. GameManagerにリスタートを依頼
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartStage();
        }
    }

    // --- ★ 3. ホームボタン用のメソッドを新設 ---
    /// <summary>
    /// ホームボタンが押された時に呼ばれる
    /// </summary>
    void OnHomePressed()
    {
        // 6. GameManagerに「ホームへ戻る」よう依頼
        if (GameManager.Instance != null)
        {
            // (GameManager側で永続オブジェクトの破棄とシーン遷移を行う)
            GameManager.Instance.ReturnToHome();
        }
    }

    /// <summary>
    /// GameManagerから呼ばれ、UIを表示する
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// GameManagerから呼ばれ、UIを非表示にする
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}