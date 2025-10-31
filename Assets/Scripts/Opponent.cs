using System.Collections; // コルーチンを使うために必要
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Opponent : MonoBehaviour
{
    [Header("Stats")]
    public float maxHp = 3f;           // 敵の最大HP
    public float moveSpeed = 1f;       // 敵の移動速度（ゆっくり）

    [Header("Damage Effect")]
    public Color flashColor = Color.red; // ダメージを受けた時の色
    public float invincibleDuration = 0.2f; // 無敵時間（秒）

    // --- 内部で管理する変数 ---
    private float currentHp;
    private bool isMoving = true;
    private bool isInvincible = false;

    // --- 必要なコンポーネント ---
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor; // 元の色（白）を保存

    void Awake()
    {
        // 必要なコンポーネントを自分自身から取得
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 元の色を記憶
        originalColor = spriteRenderer.color;
    }

    void Start()
    {
        // ゲーム開始時の設定
        currentHp = maxHp;
        isMoving = true;
        isInvincible = false;
    }

    void FixedUpdate()
    {
        // "isMoving" が true の間だけ、下に移動し続ける
        if (isMoving)
        {
            // Rigidbody 2D の速度を直接制御
            rb.linearVelocity = Vector2.down * moveSpeed;
        }
    }

    // 物理的な衝突が発生した時に呼ばれる
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. もし「Ball」タグのオブジェクトに衝突したら
        if (collision.gameObject.CompareTag("Ball"))
        {
            HandleDamage();
        }
        
        // 2. もし「Ground」タグのオブジェクト（下の壁）に衝突したら
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Opponent reached the ground.");

            StopAndLock();
            
            // TODO: ここで将来的にGameManagerにゲームオーバーを通知する
            // Debug.Log("ゲームオーバー！");
        }
    }

    /// <summary>
    /// 移動を停止し、その場に固定される
    /// </summary>
    private void StopAndLock()
    {
        isMoving = false;
        rb.linearVelocity = Vector2.zero; // 速度を0に
        rb.bodyType = RigidbodyType2D.Static; // 物理的に動かない「静的」な物体に変化
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    private void HandleDamage()
    {
        // 無敵時間中、またはHPが0なら、何もしない
        if (isInvincible || currentHp <= 0)
        {
            return;
        }

        // HPを減らす
        currentHp--;

        // HPが0以下になったかチェック
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // HPが残っているなら、無敵と点滅処理を開始
            StartCoroutine(InvincibleFlashRoutine());
        }
    }

    /// <summary>
    /// 無敵時間と点滅（色変更）を管理するコルーチン
    /// </summary>
    private IEnumerator InvincibleFlashRoutine()
    {
        isInvincible = true; // 無敵開始
        spriteRenderer.color = flashColor; // 色を赤に変更

        // invincibleDuration で指定した秒数だけ待つ
        yield return new WaitForSeconds(invincibleDuration);

        spriteRenderer.color = originalColor; // 色を元に戻す
        isInvincible = false; // 無敵終了
    }

    /// <summary>
    /// 死亡時の処理（仕様通りDestroyのみ）
    /// </summary>
    private void Die()
    {
        // TODO: 将来的にここで爆発エフェクトなどを再生する
        
        Destroy(gameObject); // オブジェクトを消滅させる
    }
}