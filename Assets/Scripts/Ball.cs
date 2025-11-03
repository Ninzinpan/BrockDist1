using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [Header("Lifecycle")]
    public float lifetime = 10f; // この時間（秒）が経過すると消滅対象になる

    // --- ★ 1. ここから追加 ---
    [Header("Bounce Limit")]
    [Tooltip("Opponent（敵）に当たらずに反射できる最大回数")]
    public int maxBounces = 5; // 反射の最大回数
    
    private int currentBounceCount = 0; // 現在の反射回数
    // --- 追加ここまで ---

    // --- 内部ステータス ---
    private Rigidbody2D rb;
    private bool isPerishable = false; // 消滅対象（寿命を迎えた）か？

    // --- 静的（グローバル）変数 ---
    public static int currentBallCount = 0; // 現在のボールの総数

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
        // 速度が0にならないように、最低限のY方向の速度を保証する
        if (Mathf.Abs(direction.y) < 0.1f)
        {
            direction.y = direction.y >= 0 ? 0.1f : -0.1f;
        }

        // PlayerStats（シングルトン）から現在の正しいボール速度を取得
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
        // 常に PlayerStats から「現在の正しい速度」を取得
        float targetSpeed = 5f; // デフォルト値
        if (PlayerStats.Instance != null)
        {
            targetSpeed = PlayerStats.Instance.CurrentBallSpeed;
        }

        // 速度が0（停止）でなければ、速度を一定に保つ
        if (rb.linearVelocity.sqrMagnitude > 0.01f) 
        {
            // startSpeed ではなく、targetSpeed（現在の正しい速度）と比較・維持
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
        // 'lifetime'秒だけ待つ
        yield return new WaitForSeconds(lifetime);

        // 'lifetime'秒経過したら、消滅対象(Perishable)にする
        isPerishable = true;
    }

    /// <summary>
    /// トリガー判定（Playerパッドとの接触）
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. プレイヤー（パッド）に触れたか
        if (other.CompareTag("Player"))
        {
            // 1a. 寿命による消滅判定
            if (isPerishable)
            {
                Destroy(gameObject);
                return; // 消滅するので反射カウントしない
            }

            // 1b. 反射としてカウント
            HandleBounce();
        }
    }

    // --- ★ 2. 以下のメソッドをまるごと追加 ---
    /// <summary>
    /// 物理的に「衝突」した時に呼ばれる
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 敵 (Opponent) に当たったか？
        if (collision.gameObject.CompareTag("Opponent"))
        {
            // 反射カウントをリセット
            currentBounceCount = 0;
        }
        // 2. それ以外（壁 "Wall_Top", "Ground" など）に当たったか？
        else
        {
            // 反射としてカウント
            HandleBounce();
        }
    }

    // --- ★ 3. 以下のメソッドをまるごと追加 ---
    /// <summary>
    /// 反射をカウントし、上限に達したら消滅する
    /// </summary>
    private void HandleBounce()
    {
        currentBounceCount++;

        if (currentBounceCount >= maxBounces)
        {
            // （オプション: ここで消滅エフェクトを再生してもよい）
            Destroy(gameObject);
        }
    }
}