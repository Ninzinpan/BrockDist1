using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    // ★ 1. シングルトン化
    public static GameOverUI Instance { get; private set; }

    [Header("References")]
    public Button restartButton;

    void Awake()
    {
        // ★ 2. シングルトン ＋ 永続化ロジック
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 既に存在したら自分を破棄
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいで永続化
    }

    void Start()
    {
        // 3. ボタンにリスナーを設定
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartPressed);
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

    /// <summary>
    /// GameManagerから呼ばれ、UIを表示する
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        // (ここでフェードインなどの演出を開始してもよい)
    }

    /// <summary>
    /// GameManagerから呼ばれ、UIを非表示にする
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}