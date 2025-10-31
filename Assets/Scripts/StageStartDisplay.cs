using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro を使うために必要

[RequireComponent(typeof(CanvasGroup))]
public class StageStartDisplay : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI stageNameText; // ★ Inspectorで設定

    [Header("Timings")]
    public float displayDuration = 1.5f; // テキストが表示され続ける時間
    public float fadeDuration = 1.0f;  // テキストが消えるのにかかる時間

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// GameManager から呼ばれる
    /// </summary>
    public void StartSequence(string stageName)
    {
        stageNameText.text = stageName;
        StartCoroutine(FadeOutRoutine());
    }

    // --- ★ このメソッド（コルーチン）を修正 ---
    private IEnumerator FadeOutRoutine()
    {
        // 1. まず、完全に不透明にする
        canvasGroup.alpha = 1f;

        // 2. 表示時間（displayDuration）だけ「リアルタイム」で待つ
        //    (この間 Time.timeScale は 0)
        yield return new WaitForSecondsRealtime(displayDuration);

        // 3. ★最重要★ ゲームの時間を動かし始める！
        //    (プレイヤーや敵が動き出す)
        Time.timeScale = 1f;

        // 4. フェードアウト処理
        float timer = 0f;
        while (timer < fadeDuration)
        {
            // ★修正★
            // Time.unscaledDeltaTime ではなく、通常の Time.deltaTime を使う
            // (ゲームの進行と同時にフェードアウトする)
            timer += Time.deltaTime; 
            
            // 1から0へ、徐々にアルファ（透明度）を下げる
            canvasGroup.alpha = 1f - (timer / fadeDuration);
            
            yield return null; // 1フレーム待つ
        }

        // 5. 完全に透明にする
        canvasGroup.alpha = 0f;

        // 6. UIを非表示にする
        gameObject.SetActive(false);
    }
}