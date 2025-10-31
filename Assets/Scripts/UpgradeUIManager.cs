using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("Next Scene")]
    public string nextSceneName = "Stage2";

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
    private LocalizationManager l10n; // 翻訳マネージャー

    // Start() はもう l10n の取得に不要（空にしてもよい）
    void Start()
    {
        // 以前はここで l10n を取得していたが、DisplayChoices に移動
    }

    /// <summary>
    /// GameManager から呼ばれ、UIを表示・設定する
    /// </summary>
    public void DisplayChoices(PlayerStats stats, BuffData choice1, BuffData choice2, BuffData choice3)
    {
        // --- ★ 1. 修正点：ここで l10n を取得する ---
        // (Start() ではなく、UIが表示される直前に取得)
        l10n = LocalizationManager.Instance;
        if(l10n == null)
        {
            Debug.LogError("LocalizationManager がシーンにありません！ UIの翻訳ができません。");
        }
        // --- 修正ここまで ---

        this.playerStats = stats; 
        
        // 2. UIを（念のため）先にアクティブにする
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        
        // 3. 各ボタンを設定する
        SetupButton(choiceButton1, choiceIcon1, choiceNameText1, choiceDescText1, choice1);
        SetupButton(choiceButton2, choiceIcon2, choiceNameText2, choiceDescText2, choice2);
        SetupButton(choiceButton3, choiceIcon3, choiceNameText3, choiceDescText3, choice3);
    }

    private void SetupButton(Button button, Image icon, TextMeshProUGUI nameText, TextMeshProUGUI descText, BuffData buff)
    {
        if (button == null || icon == null || nameText == null || descText == null || buff == null || playerStats == null)
        {
            Debug.LogError("UpgradeUIManager の Inspector 設定が不足しているか、BuffData が null です。");
            return;
        }
        
        // ★ 4. 修正点：l10n が null でもクラッシュせず、英語名を表示する
        if (l10n == null)
        {
            nameText.text = buff.buffName; 
            descText.text = "Error: L10N missing";
            return; 
        }

        // --- 1. テキストとアイコンの設定 ---
        string translatedName = l10n.GetTranslatedName(buff.buffName);
        string translatedDescTemplate = l10n.GetTranslatedDescription(buff.buffName);
        
        icon.sprite = buff.buffIcon;

        // --- 2. レベルと効果値の計算 ---
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

        // --- 3. クリックイベントの設定 ---
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

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("次のステージ名(Next Scene Name)が設定されていません！");
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}