using UnityEngine;
using UnityEngine.UI; // ★ 1. Image を使うために必要

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))] // ★ 2. Image が必須
public class MouseReticle : MonoBehaviour
{
    private RectTransform reticleRectTransform;
    private Image reticleImage; // ★ 3. Image コンポーネントへの参照

    void Start()
    {
        reticleRectTransform = GetComponent<RectTransform>();
        reticleImage = GetComponent<Image>(); // ★ 4. Image コンポーネントを取得

        Cursor.visible = false;
        reticleImage.enabled = true; // ★ 5. Image を表示
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            // --- ポーズ中の処理 ---
            if (reticleImage.enabled) // ★ 6. Image が表示されているなら
            {
                Cursor.visible = true; // OSカーソルを表示
                reticleImage.enabled = false; // ★ 7. Image コンポーネントを無効（非表示）
            }
        }
        else
        {
            // --- ゲームプレイ中の処理 ---
            if (!reticleImage.enabled) // ★ 8. Image が非表示なら
            {
                Cursor.visible = false; // OSカーソルを非表示
                reticleImage.enabled = true; // ★ 9. Image コンポーネントを有効（表示）
            }
            
            // マウスに追従
            reticleRectTransform.position = Input.mousePosition;
        }
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}