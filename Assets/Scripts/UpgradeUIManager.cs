using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeUIManager : MonoBehaviour
{
    // ★ 1. nextSceneName 変数を削除
    // [Header("Next Scene")]
    // public string nextSceneName = "Stage2"; 

    // --- 3つの選択肢ボタンの参照 ---
    [Header("Choice Button 1")]
    public Button choiceButton1;
    public Image choiceIcon1;
    public TextMeshProUGUI choiceNameText1;
    public TextMeshProUGUI choiceDescText1;

    [Header("Choice Button 2")]
    public Button choiceButton2;
    public Image choiceIcon2;
    public TextMeshProUGUI choiceNameText2;
    public TextMeshProUGUI choiceDescText2;

    [Header("Choice Button 3")]
    public Button choiceButton3;
    public Image choiceIcon3;
    public TextMeshProUGUI choiceNameText3;
    public TextMeshProUGUI choiceDescText3; 

    // --- 内部で保持するデータ ---
    private PlayerStats playerStats;
    private LocalizationManager l10n;
    private GameManager gameManager; // ★ 2. GameManager への参照を追加
    public static UpgradeUIManager Instance { get; private set; }
    // Awake() または Start() で参照を取得
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
        // 永続化オブジェクトへの参照を取得
        l10n = LocalizationManager.Instance;
        gameManager = GameManager.Instance; 

        if(l10n == null)
        {
            Debug.LogError("LocalizationManager がシーンにありません！");
        }
        if (gameManager == null)
        {
            Debug.LogError("GameManager がシーンにありません！");
        }
    }

    /// <summary>
    /// GameManager から呼ばれ、UIを表示・設定する
    /// </summary>
    public void DisplayChoices(PlayerStats stats, BuffData choice1, BuffData choice2, BuffData choice3)
    {
        this.playerStats = stats; 

        // UIを先にアクティブにする（Start()が呼ばれていない場合のため）
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        
        // l10n と gameManager が null なら、Start() を強制的に呼ぶ（保険）
        if (l10n == null) Start(); 

        SetupButton(choiceButton1, choiceIcon1, choiceNameText1, choiceDescText1, choice1);
        SetupButton(choiceButton2, choiceIcon2, choiceNameText2, choiceDescText2, choice2);
        SetupButton(choiceButton3, choiceIcon3, choiceNameText3, choiceDescText3, choice3);
    }

    private void SetupButton(Button button, Image icon, TextMeshProUGUI nameText, TextMeshProUGUI descText, BuffData buff)
    {
        if (button == null || icon == null || nameText == null || descText == null || buff == null || playerStats == null)
        {
            Debug.LogError("UpgradeUIManager の Inspector 設定が不足しています。");
            return;
        }
        if (l10n == null)
        {
            nameText.text = buff.buffName;
            descText.text = "Error: L10N missing";
            return; 
        }

        string translatedName = l10n.GetTranslatedName(buff.buffName);
        string translatedDescTemplate = l10n.GetTranslatedDescription(buff.buffName);
        
        icon.sprite = buff.buffIcon;

        int currentLevel = 0;
        playerStats.buffLevels.TryGetValue(buff, out currentLevel);

        if (buff.isOneTimeEffect)
        {
            nameText.text = translatedName;
            descText.text = translatedDescTemplate.Replace("{value}", buff.enhancementValue.ToString("F0"));
        }
        else
        {
            int nextLevel = currentLevel + 1; 
            float totalEffectValue = buff.enhancementValue * nextLevel;
            
            nameText.text = $"{translatedName} Lv.{nextLevel}";
            descText.text = translatedDescTemplate.Replace("{value}", Mathf.Abs(totalEffectValue).ToString("F2"));
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnChoiceMade(buff) );
    }

    /// <summary>
    /// いずれかのボタンが押された時に呼ばれる
    /// </summary>
    /// <summary>
    /// いずれかのボタンが押された時に呼ばれる
    /// </summary>
    private void OnChoiceMade(BuffData chosenBuff)
    {
        // 1. PlayerStats に選んだバフを渡す
        if (playerStats != null)
        {
            playerStats.AddBuff(chosenBuff);
        }

        // 2. UIを非表示にする
        gameObject.SetActive(false);
        
        // 3. ゲーム時間を戻す（★これは残す）
        Time.timeScale = 1f;

        // ★ 4. シーン遷移のロジックを削除（またはコメントアウト）
        /*
        if (gameManager != null)
        {
            gameManager.LoadNextStage();
        }
        else
        {
            Debug.LogError("GameManager が null のため、次のステージに進めません！");
        }
        */
    }
}