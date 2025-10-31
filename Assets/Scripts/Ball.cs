using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [Header("Stats")]
    public float startSpeed = 5f;

    [Header("Lifecycle")]
    public float lifetime = 10f; // この時間（秒）が経過すると消滅対象になる

    // --- 内部ステータス ---
    private Rigidbody2D rb;
    private bool isPerishable = false; // 消滅対象（寿命を迎えた）か？

    // --- 静的（グローバル）変数 ---
    public static int currentBallCount = 0; // 現在のボールの総数

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // ボールが「生成」されたので、カウンターを増やす
        currentBallCount++;
    }

    void Start()
    {
        // 消滅タイマー（コルーチン）を開始
        StartCoroutine(LifetimeRoutine());
    }

    void OnDestroy()
    {
        // ボールが「消滅」するので、カウンターを減らす
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

        // 方向ベクトルを正規化（長さを1に）して、速度をかける
        rb.linearVelocity = direction.normalized * startSpeed;
    }

    // 物理演算の更新タイミングで呼ばれる
    void FixedUpdate()
    {
        // 速度が0（停止）でなければ、速度を一定に保つ
        if (rb.linearVelocity.sqrMagnitude > 0.01f) 
        {
            // 現在の速度がstartSpeedより遅い場合、速度をstartSpeedに戻す
            if (rb.linearVelocity.sqrMagnitude < startSpeed * startSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * startSpeed;
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

    // --- ★ここが変更点です ---
    /// <summary>
    /// トリガー判定（ここで消滅をチェック）
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 自分が消滅対象(isPerishable)であるか？
        // 2. 接触した相手のタグが "Player" (パッド) か？
        if (isPerishable && other.CompareTag("Player"))
        {
            Destroy(gameObject); // 両方trueなら消滅する
        }
    }
}