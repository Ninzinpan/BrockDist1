using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UpgradeUIManager : MonoBehaviour
{
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

    private PlayerStats playerStats;
    private LocalizationManager l10n;
    private GameManager gameManager;
    public static UpgradeUIManager Instance { get; private set; }

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
        l10n = LocalizationManager.Instance;
        gameManager = GameManager.Instance; 

        if(l10n == null) Debug.LogError("LocalizationManager がシーンにありません！");
        if (gameManager == null) Debug.LogError("GameManager がシーンにありません！");
    }

    /// <summary>
    /// GameManager から呼ばれ、UIを表示・設定する
    /// </summary>
    public void DisplayChoices(PlayerStats stats, BuffData choice1, BuffData choice2, BuffData choice3)
    {
        this.playerStats = stats; 

        // 1. まず時間を止める（UIは既にGameManagerによって表示されている）
        Time.timeScale = 0f;
        
        // 2. l10n が null なら Start() を呼ぶ（保険）
        if (l10n == null) Start();

        // 3. ★まずボタンを非表示にする
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        choiceButton3.gameObject.SetActive(false);

        // 4. ボタンの中身をセットアップ（非表示のまま）
        SetupButton(choiceButton1, choiceIcon1, choiceNameText1, choiceDescText1, choice1);
        SetupButton(choiceButton2, choiceIcon2, choiceNameText2, choiceDescText2, choice2);
        SetupButton(choiceButton3, choiceIcon3, choiceNameText3, choiceDescText3, choice3);

        // 5. ボタンを遅れて表示するコルーチンを開始
        StartCoroutine(ShowButtonsAfterDelay(0.5f));
    }

    /// <summary>
    /// ボタンを遅れて表示するコルーチン
    /// </summary>
    private IEnumerator ShowButtonsAfterDelay(float delay)
    {
        // リアルタイムで待機
        yield return new WaitForSecondsRealtime(delay);

        // 準備が完了したボタンを表示
        choiceButton1.gameObject.SetActive(true);
        choiceButton2.gameObject.SetActive(true);
        choiceButton3.gameObject.SetActive(true);
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
            
            // ★ 6. ユーザーの仕様（変更）に戻す
            //    (「+1」や「-0.05」など、1レベルあたりの上昇値のみを表示)
            float totalEffectValue = buff.enhancementValue; 
            
            nameText.text = $"{translatedName} Lv.{nextLevel}";
            descText.text = translatedDescTemplate.Replace("{value}", Mathf.Abs(totalEffectValue).ToString("F2"));
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnChoiceMade(buff) );
    }

    private void OnChoiceMade(BuffData chosenBuff)
    {
        if (playerStats != null)
        {
            playerStats.AddBuff(chosenBuff);
        }

        gameObject.SetActive(false);
        Time.timeScale = 1f;
        
        // (シーン遷移のロジックは削除済み)
    }
}