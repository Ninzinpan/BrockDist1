using UnityEngine;

// Collider 2Dがアタッチされていることを保証
[RequireComponent(typeof(Collider2D))]
public class PlayerHitbox : MonoBehaviour
{
    // 親にあるPadController（本体）への参照
    private PadController padController;

    void Start()
    {
        // 1. 親オブジェクト（Pad）にある PadController スクリプトを探して保存
        padController = GetComponentInParent<PadController>();

        if (padController == null)
        {
            Debug.LogError("親オブジェクトに PadController が見つかりません！", this);
        }

        // 2. このコライダーが必ずTriggerであることを確認
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("PlayerHitboxのCollider 2Dが 'Is Trigger' になっていません！", this);
        }
    }

    // 3. 他のTriggerに接触した時に呼ばれる
    void OnTriggerEnter2D(Collider2D other)
    {
        // 4. 接触した相手のタグが "Thorn" (ステップ2で作成) かどうかチェック
        if (other.CompareTag("Thorn"))
        {
            // 5. "Thorn" から ThornProjectile スクリプト（ステップ2で作成）を取得
            ThornProjectile thorn = other.GetComponent<ThornProjectile>();

            // 6. 参照が両方とも正しく取れたら、PadController にダメージを伝える
            if (padController != null && thorn != null)
            {
                // PadControllerのTakeDamageメソッドを、Thornが持つダメージ量で呼び出す
                padController.TakeDamage(thorn.damage);
            }
            
            // 棘（Thorn）自身の消滅処理は、
            // ステップ2で作成するThornProjectile.cs側で担当させます。
        }
    }
}