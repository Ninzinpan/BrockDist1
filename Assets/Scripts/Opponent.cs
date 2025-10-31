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
    public float damageToPlayer = 1f; 

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
    
    // --- 参照 ---
    private PadController player;
    private PlayerStats playerStats;
    private GameManager gameManager; // ★ 1. GameManager への参照を追加

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
        
        // シーン全体から必要なコンポーネントを探す
        player = FindFirstObjectByType<PadController>();
        playerStats = FindFirstObjectByType<PlayerStats>();
        gameManager = FindFirstObjectByType<GameManager>(); // ★ 2. GameManager を探す

        if (player == null) Debug.LogError("シーンに PadController が見つかりません！", this);
        if (playerStats == null) Debug.LogError("シーンに PlayerStats が見つかりません！", this);
        if (gameManager == null) Debug.LogError("シーンに GameManager が見つかりません！", this);
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            rb.linearVelocity = Vector2.down * moveSpeed;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (playerStats != null)
            {
                HandleDamage(playerStats.CurrentBallDamage);
            }
            else
            {
                HandleDamage(1f); 
            }
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
    
    private void HandleDamage(float damageAmount)
    {
        if (isInvincible || currentHp <= 0) return;

        currentHp -= damageAmount; 
        
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
        // ★ 3. 死亡処理
        
        // 1. GameManager に死亡を報告
        if (gameManager != null)
        {
            gameManager.OnEnemyDefeated();
        }

        // 2. 自分自身を消滅させる
        Destroy(gameObject);
    }
}