using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PadController : MonoBehaviour
{
    [Header("Movement")]
    // public float speed = 10f; // ← ★ 1. 削除（PlayerStatsへ移管）
    public float boundary = 8f;
    private Rigidbody2D rb;

    [Header("Ball Launching")]
    public GameObject ballPrefab;
    public Transform spawnPoint;

    [Header("Ball Limit")]
    // (PlayerStatsが管理)

    [Header("Player Stats")]
    public float invincibilityDuration = 1f;

    [Header("UI")]
    public Slider hpBarSlider; 

    // --- 内部変数 ---
    private float currentHp;
    private bool isInvincible = false;
    private bool isAlive = true;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float spawnTimer;

    // --- コンポーネント参照 ---
    private PlayerStats playerStats; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTimer = 0f;
        isAlive = true;
        isInvincible = false;
        
        playerStats = GetComponent<PlayerStats>();
        currentHp = playerStats.CurrentMaxHp; 
        
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        if(spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        UpdateHpBar();
        playerStats.OnStatsRecalculated.AddListener(UpdateHpBarMax);
    }

    void FixedUpdate() 
    {
        if (!isAlive) return; 
        HandleMovement();
    }

    void Update()
    {
        if (!isAlive) return; 
        HandleSpawning();
    }

    private void HandleMovement()
    {
        // ★ 2. PlayerStats の CurrentSpeed を参照するように変更
        float moveInput = Input.GetAxis("Horizontal");
        Vector2 newPosition = rb.position + Vector2.right * moveInput * playerStats.CurrentSpeed * Time.fixedDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary);
        rb.MovePosition(newPosition);
    }

    // (HandleSpawning, LaunchBallTowardsMouse, TakeDamage, Heal, 
    //  UpdateHpBar, UpdateHpBarMax, InvincibilityRoutine, Die は変更なし)

    private void HandleSpawning()
    {
        if (spawnTimer > 0f)
        {
            spawnTimer -= Time.deltaTime;
        }
        
        if (Input.GetMouseButton(0) && spawnTimer <= 0f)
        {
            if (Ball.currentBallCount >= playerStats.CurrentMaxBalls)
            {
                return; 
            }
            
            LaunchBallTowardsMouse();
            spawnTimer = playerStats.CurrentSpawnInterval; 
        }
    }

    private void LaunchBallTowardsMouse()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 launchDirection = mouseWorldPosition - (Vector2)spawnPoint.position;
        GameObject newBallObj = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        Ball ballScript = newBallObj.GetComponent<Ball>();

        if (ballScript != null)
        {
            ballScript.Launch(launchDirection);
        }
        else
        {
            Debug.LogError("Ball prefabに 'Ball' スクリプトがありません！");
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isInvincible || !isAlive) return;
        currentHp -= amount;
        UpdateHpBar(); 
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }
    
    public void Heal(float amount)
    {
        if (!isAlive) return;
        currentHp = Mathf.Min(currentHp + amount, playerStats.CurrentMaxHp);
        UpdateHpBar();
    }
    
    private void UpdateHpBar()
    {
        if (hpBarSlider != null)
        {
            hpBarSlider.value = currentHp;
        }
    }
    
    private void UpdateHpBarMax()
    {
        if (hpBarSlider != null)
        {
            hpBarSlider.maxValue = playerStats.CurrentMaxHp;
            if(currentHp > playerStats.CurrentMaxHp)
            {
                currentHp = playerStats.CurrentMaxHp;
                UpdateHpBar();
            }
        }
    }
    
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                bool isVisible = (Mathf.FloorToInt(timer * 10) % 2 == 0);
                spriteRenderer.color = isVisible ? Color.red : originalColor;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        isInvincible = false;
    }
    
    private void Die()
    {
        isAlive = false;
        if(spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
    }
}