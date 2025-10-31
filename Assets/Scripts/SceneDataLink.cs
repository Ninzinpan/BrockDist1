using UnityEngine;

public class SceneDataLink : MonoBehaviour
{
    [Header("Stage Configuration")]
    // ★ Inspectorで、このシーンに対応する StageData アセットを設定
    public StageData currentStageData;

    void Awake()
    {
        // 設定漏れがないかチェック
        if (currentStageData == null)
        {
            Debug.LogError("SceneDataLink に StageData が設定されていません！", this);
        }
    }
}