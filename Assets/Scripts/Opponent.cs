using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))] 
public class Opponent : MonoBehaviour
{
    [Header("Stats")]
    public float maxHp = 3f;
    public float moveSpeed = 1f; // 画面外（初期）の移動速度
    
    [Tooltip("画面内に入った（Topタグに触れた）後の移動速度。0のままなら moveSpeed を使い続ける")]
    public float inScreenMoveSpeed = 0f; 

    // --- ★ 1. ここから追加 ---
    [Header("Screen Boundary")]
    [Tooltip("このY座標（ワールド座標）より下に移動したら、画面内とみなす")]
    public float inScreenYBoundary = 4.5f;
    // --- 追加ここまで ---

    [Header("On Ground Contact")]
    public float damageToPlayer = 1f; 

    [Header("Damage Effect")]
    public Color flashColor = Color.red;
    public float invincibleDuration = 0.2f;

    [Header("Audio")]
    public AudioClip hitSound; 
    public AudioClip deathSound; 
    
    private AudioSource audioSource; 

    // --- 内部変数 ---
    private float currentHp;
    private bool isMoving = true;
    private bool isInvincible = false;
    private bool hasEnteredScreen = false; // ★ 2. 画面内に既に入ったか

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    private PadController player;
    private PlayerStats playerStats;
    private GameManager gameManager; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        audioSource = GetComponent<AudioSource>(); 
    }

    void Start()
    {
        currentHp = maxHp;
        isMoving = true;
        isInvincible = false;
        hasEnteredScreen = false; // ★ 3. 初期化
        
        player = FindFirstObjectByType<PadController>();
        playerStats = FindFirstObjectByType<PlayerStats>();
        gameManager = FindFirstObjectByType<GameManager>(); 

        if (player == null) Debug.LogError("シーンに PadController が見つかりません！", this);
        if (playerStats == null) Debug.LogError("シーンに PlayerStats が見つかりません！", this);
        if (gameManager == null) Debug.LogError("シーンに GameManager が見つかりません！", this);
    }

    // --- ★ 4. FixedUpdate を修正 ---
    void FixedUpdate()
    {
        if (isMoving)
        {
            // 1. まだ画面内に入っていないかチェック
            if (!hasEnteredScreen)
            {
                // 2. 自分のY座標が、設定したY座標より下に来たかチェック
                if (transform.position.y < inScreenYBoundary)
                {
                    // 3. 画面内に入った
                    hasEnteredScreen = true;

                    // 4. Inspectorで inScreenMoveSpeed が設定されていれば速度を更新
                    if (inScreenMoveSpeed > 0f)
                    {
                        moveSpeed = inScreenMoveSpeed;
                    }
                }
            }

            // 5. （更新された可能性のある）moveSpeed で移動
            rb.linearVelocity = Vector2.down * moveSpeed;
        }
    }
    
    // ★ 5. OnTriggerEnter2D は（Topタグに関しては）不要
    // void OnTriggerEnter2D(Collider2D other) { ... }

    // ★ 6. OnCollisionEnter2D は（Topタグの処理がないまま）
    void OnCollisionEnter2D(Collision2D collision)
    {
        // "Ball" タグに触れたか
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            if (playerStats != null)
            {
                HandleDamage(playerStats.CurrentBallDamage);
            }
            else
            {
                HandleDamage(1f); 
            }
        }
        
        // "Ground" タグに触れたか
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
    
    // (HandleDamage, InvincibleFlashRoutine, Die は変更なし)
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
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        if (gameManager != null)
        {
            gameManager.OnEnemyDefeated();
        }
        Destroy(gameObject);
    }
}