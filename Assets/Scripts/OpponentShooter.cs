using UnityEngine;

public class OpponentShooter : MonoBehaviour
{
    [Header("Shooting Setup")]
    public GameObject thornPrefab;    // ★ Inspectorで Thorn プレハブを設定
    public Transform firePoint;     // ★ Inspectorで FirePoint オブジェクトを設定
    public float fireInterval = 2f; // 発射間隔（秒）

    // 内部タイマー
    private float fireTimer;

    void Start()
    {
        // 最初にタイマーを発射間隔にセット
        fireTimer = fireInterval;
    }

    void Update()
    {
        // 1. 毎フレーム、タイマーを減らす
        fireTimer -= Time.deltaTime;

        // 2. タイマーが0以下になったら
        if (fireTimer <= 0f)
        {
            // 3. 発射処理
            Shoot();
            
            // 4. タイマーをリセット
            fireTimer = fireInterval;
        }
    }

    /// <summary>
    /// 棘を発射する
    /// </summary>
    private void Shoot()
    {
        // プレハブや発射位置が正しく設定されているか確認
        if (thornPrefab != null && firePoint != null)
        {
            // 棘（Thorn）を FirePoint の位置に生成（Instantiate）する
            Instantiate(thornPrefab, firePoint.position, Quaternion.Euler(0, 0, 180));        }
        else
        {
            Debug.LogWarning("Thorn Prefab または FirePoint が設定されていません", this);
        }
    }
}