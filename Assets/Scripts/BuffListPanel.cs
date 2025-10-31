using UnityEngine;
using TMPro; // TextMeshPro を使うために必要

public class PersistentBuffUI : MonoBehaviour
{
    [Header("UI References")]
    // ★ Inspectorで設定: バフ1行分のプレハブ
    public GameObject buffEntryPrefab; 
    
    // ★ Inspectorで設定: VerticalLayoutGroup が付いたパネル
    public Transform buffListContainer; 

    [Header("Singleton & Persistence")]
    public static PersistentBuffUI Instance { get; private set; }

    // --- 参照 ---
    private PlayerStats playerStats;
    private LocalizationManager l10n;

    void Awake()
    {
        // 1. このUI Canvasを永続化するシングルトン設定
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
        // 2. 他のシングルトン（永続化オブジェクト）への参照を取得
        //    (Start() で呼ぶのは、他の Awake() が完了するのを待つため)
        playerStats = PlayerStats.Instance;
        l10n = LocalizationManager.Instance;

        if (playerStats == null || l10n == null)
        {
            Debug.LogError("PlayerStats または L10n が見つかりません！");
            return;
        }

        // 3. ★最重要★
        // PlayerStats の「ステータスが更新された」イベントを "購読" する
        // OnStatsRecalculated が呼ばれたら、UpdateUI メソッドも呼ぶように設定
        playerStats.OnStatsRecalculated.AddListener(UpdateUI);
        
        // 4. ゲーム開始時に一度UIを更新
        UpdateUI();
    }

    // PlayerStats.OnStatsRecalculated が呼び出されると、
    // このメソッドが自動的に実行される
    public void UpdateUI()
    {
        if (playerStats == null || l10n == null) return; // 安全装置

        // 1. 古いUIエントリをすべて削除（全消去）
        foreach (Transform child in buffListContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. PlayerStats の「台帳」を元に、UIを再構築
        //    (buffLevels = Dictionary<BuffData, int>)
        foreach (var buffEntry in playerStats.buffLevels)
        {
            BuffData buff = buffEntry.Key;
            int level = buffEntry.Value;

            // 3. プレハブ（BuffEntry_Template）を
            //    buffListContainer の子として生成
            GameObject entryGO = Instantiate(buffEntryPrefab, buffListContainer);
            
            // 4. 生成したオブジェクトの TextMeshPro コンポーネントを取得
            TextMeshProUGUI textComponent = entryGO.GetComponent<TextMeshProUGUI>();

            if (textComponent != null)
            {
                // 5. テキストを設定 (例: "連射速度アップ Lv.2")
                string translatedName = l10n.GetTranslatedName(buff.buffName);
                textComponent.text = $"{translatedName} Lv.{level}";
            }
        }
    }

    // オブジェクトが破棄される時（ゲーム終了時など）に呼ばれる
    void OnDestroy()
    {
        // 6. メモリリーク防止のため、イベントの購読を解除
        if (playerStats != null)
        {
            playerStats.OnStatsRecalculated.RemoveListener(UpdateUI);
        }
    }
}