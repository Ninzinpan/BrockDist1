using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    // 辞書を「名前用」と「説明文用」の2つに分ける
    private Dictionary<string, string> nameTranslations = new Dictionary<string, string>();
    private Dictionary<string, string> descriptionTemplates = new Dictionary<string, string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase(); // 辞書を初期化
        }
    }

    /// <summary>
    /// 翻訳データベースを初期化
    /// </summary>
    private void InitializeDatabase()
    {
        // --- 名前用の辞書 ---
        // キーは BuffData.buffName と一致させる
        nameTranslations.Add("Health Recovery", "体力回復");
        nameTranslations.Add("Fire Rate Up", "連射速度アップ");
        nameTranslations.Add("Max Balls Up", "最大ボール数アップ");
        nameTranslations.Add("Ball Damage Up", "ボール攻撃力アップ");
        nameTranslations.Add("Speed Up", "移動速度アップ"); // (Speed用に追加)

        // --- 説明文用の辞書 ---
        // キーは BuffData.buffName と一致させる
        descriptionTemplates.Add("Health Recovery", "体力を 1 ポイント回復する");
        descriptionTemplates.Add("Fire Rate Up", "連射間隔が {value} 秒短縮される");
        descriptionTemplates.Add("Max Balls Up", "ボールの最大数が {value} 個増加する");
        descriptionTemplates.Add("Ball Damage Up", "ボールの攻撃力が {value} ポイント強化");
        descriptionTemplates.Add("Speed Up", "移動速度が {value} 増加する");
    }

    /// <summary>
    /// BuffName (ID) を渡すと、和訳された「名前」を返す
    /// </summary>
    public string GetTranslatedName(string buffNameKey)
    {
        if (nameTranslations.TryGetValue(buffNameKey, out string value))
        {
            return value;
        }
        return buffNameKey; // 見つからなければキー（英語名）をそのまま返す
    }

    /// <summary>
    /// BuffName (ID) を渡すと、和訳された「説明文テンプレート」を返す
    /// </summary>
    public string GetTranslatedDescription(string buffNameKey)
    {
        if (descriptionTemplates.TryGetValue(buffNameKey, out string value))
        {
            return value;
        }
        return "説明文が見つかりません";
    }
}