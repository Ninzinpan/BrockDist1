using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))] // ★ 1. AudioSource が必須であることを明記
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

    // --- ★ 2. ここから追加 ---
    [Header("Audio")]
    public AudioClip hitSound; // Inspectorで設定する音ファイル

    public AudioClip deathSound; // 敵が倒されたときの音
    
    private AudioSource audioSource; // スピーカーコンポーネント
    // --- 追加ここまで ---


    // --- 内部変数 ---
    private float currentHp;
    private bool isMoving = true;
    private bool isInvincible = false;

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

        // ★ 3. 自分の AudioSource を取得
        audioSource = GetComponent<AudioSource>(); 
    }

    void Start()
    {
        // (Start() の中身は変更なし)
        currentHp = maxHp;
        isMoving = true;
        isInvincible = false;
        
        player = FindFirstObjectByType<PadController>();
        playerStats = FindFirstObjectByType<PlayerStats>();
        gameManager = FindFirstObjectByType<GameManager>(); 

        if (player == null) Debug.LogError("シーンに PadController が見つかりません！", this);
        if (playerStats == null) Debug.LogError("シーンに PlayerStats が見つかりません！", this);
        if (gameManager == null) Debug.LogError("シーンに GameManager が見つかりません！", this);
    }

    // (FixedUpdate は変更なし)
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
            // --- ★ 4. 音を再生 ---
            if (hitSound != null)
            {
                // PlayOneShot を使うと、音が重なっても正しく再生される
                audioSource.PlayOneShot(hitSound);
            }
            // --- 再生ここまで ---

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
    
    // (HandleDamage, InvincibleFlashRoutine, Die は変更なし)
    // ...
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
AudioSource.PlayClipAtPoint(deathSound, transform.position);        }
        if (gameManager != null)
        {
            gameManager.OnEnemyDefeated();
        }
        Destroy(gameObject);
    }
}