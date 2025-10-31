using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ThornProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 5f; // 棘の移動速度
    public float damage = 1f; // プレイヤーに与えるダメージ（PlayerHitboxが読み取る）

    private Rigidbody2D rb;

    void Start()
    {
        // 1. 自分の Rigidbody 2D を取得
        rb = GetComponent<Rigidbody2D>();
        
        // 2. まっすぐ下（Vector2.down）に、指定された速度で移動を開始
        rb.linearVelocity = Vector2.down * speed;
    }

    // 3. 他の Trigger に接触した時に呼ばれる
    void OnTriggerEnter2D(Collider2D other)
    {
        // 4. もし接触した相手が...
        if (other.CompareTag("Ball") ||// 「Ball」タグか、
            other.CompareTag("Top") ||             
            other.CompareTag("Ground") ||              // 「Ground」タグか、
            other.CompareTag("PlayerForOpponent"))     // 「PlayerForOpponent」タグなら
        {
            // 自分自身を消滅させる
            Destroy(gameObject);
        }
    }
}