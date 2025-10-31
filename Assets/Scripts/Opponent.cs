using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Opponent : MonoBehaviour
{
    [Header("Stats")]
    public float maxHp = 3f;
    public float moveSpeed = 1f;

    [Header("On Ground Contact")]
    public float damageToPlayer = 1f; // プレイヤーに与えるダメージ

    [Header("Damage Effect")]
    public Color flashColor = Color.red;
    public float invincibleDuration = 0.2f;

    // --- 内部変数 ---
    private float currentHp;
    private bool isMoving = true;
    private bool isInvincible = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    private PadController player; // プレイヤーへの参照を保持

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    void Start()
    {
        currentHp = maxHp;
        isMoving = true;
        isInvincible = false;
        
        // --- ★ここが修正点です ---
        // 'FindObjectOfType' を 'FindFirstObjectByType' に変更
        player = FindFirstObjectByType<PadController>();
        // --- 修正ここまで ---

        if (player == null)
        {
            Debug.LogError("シーンに PadController が見つかりません！");
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            rb.linearVelocity = Vector2.down * moveSpeed;
        }
    }

    // (OnCollisionEnter2D, HandleDamage, InvincibleFlashRoutine, Die メソッドは変更なし)
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            HandleDamage();
        }
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;

            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
            }
            
            Die(); 
        }
    }
    
    private void HandleDamage()
    {
        if (isInvincible || currentHp <= 0)
        {
            return;
        }
        currentHp--;
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleFlashRoutine());
        }
    }

    private IEnumerator InvincibleFlashRoutine()
    {
        isInvincible = true;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(invincibleDuration);
        spriteRenderer.color = originalColor;
        isInvincible = false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}