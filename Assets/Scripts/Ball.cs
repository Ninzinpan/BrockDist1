using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    // ★ 1. この startSpeed 変数はもう「基本速度」としては使わない
    //    (Launch() での一時的な使用、または FixedUpdate での参照のみ)
    // public float startSpeed; // ← 削除してもOK

    [Header("Lifecycle")]
    public float lifetime = 10f; 

    // ★ 2. PlayerStats への参照は Start() では不要
    // private PlayerStats playerStats;

    // --- 内部ステータス ---
    private Rigidbody2D rb;
    private bool isPerishable = false;

    // --- 静的（グローバル）変数 ---
    public static int currentBallCount = 0; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentBallCount++;
    }

    void Start()
    {
        // 消滅タイマー（コルーチン）を開始
        StartCoroutine(LifetimeRoutine());
    }

    void OnDestroy()
    {
        currentBallCount--;
    }

    /// <summary>
    /// ボールを指定された方向に発射します。
    /// </summary>
    public void Launch(Vector2 direction)
    {
        if (Mathf.Abs(direction.y) < 0.1f)
        {
            direction.y = direction.y >= 0 ? 0.1f : -0.1f;
        }

        // ★ 3. 発射の瞬間に、PlayerStats から「現在の速度」を取得
        float currentSpeed = 5f; // デフォルト値
        if (PlayerStats.Instance != null)
        {
            currentSpeed = PlayerStats.Instance.CurrentBallSpeed;
        }
        
        rb.linearVelocity = direction.normalized * currentSpeed;
    }

    // 物理演算の更新タイミングで呼ばれる
    void FixedUpdate()
    {
        // ★ 4. 常に PlayerStats から「現在の正しい速度」を取得
        float targetSpeed = 5f; // デフォルト値
        if (PlayerStats.Instance != null)
        {
            targetSpeed = PlayerStats.Instance.CurrentBallSpeed;
        }

        // 速度が0（停止）でなければ、速度を一定に保つ
        if (rb.linearVelocity.sqrMagnitude > 0.01f) 
        {
            // ★ 5. 古い startSpeed ではなく、targetSpeed（現在の正しい速度）と比較・維持
            if (rb.linearVelocity.sqrMagnitude < targetSpeed * targetSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * targetSpeed;
            }
        }
    }

    /// <summary>
    /// ボールの寿命を管理するコルーチン
    /// </summary>
    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        isPerishable = true;
    }

    /// <summary>
    /// トリガー判定（ここで消滅をチェック）
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPerishable && other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}