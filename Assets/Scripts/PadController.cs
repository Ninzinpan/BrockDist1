using System.Collections;
using UnityEngine;
using UnityEngine.UI; // ★1. UIコンポーネント（Slider）を使うために必要

[RequireComponent(typeof(Rigidbody2D))]
public class PadController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float boundary = 8f;
    private Rigidbody2D rb;

    [Header("Ball Launching")]
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 0.5f;

    [Header("Ball Limit")]
    public int maxBalls = 10;

    [Header("Player Stats")]
    public float maxHp = 3f; // 最大HP
    public float invincibilityDuration = 1f; // 無敵時間
    
    // --- ここから追加 ---
    [Header("UI")]
    public Slider hpBarSlider; // ★2. InspectorからHPバー（Slider）を受け取る変数
    // --- 追加ここまで ---

    private float currentHp;
    private bool isInvincible = false;
    private bool isAlive = true; // 生きているか
    
    private SpriteRenderer spriteRenderer; // 色を変えるため
    private Color originalColor;

    private float spawnTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTimer = 0f;
        
        currentHp = maxHp;
        isAlive = true;
        isInvincible = false;
        
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        if(spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // --- ここから追加 ---
        // ★3. HPバーの初期設定
        if (hpBarSlider != null)
        {
            hpBarSlider.maxValue = maxHp; // Sliderの最大値をmaxHpと同期
            hpBarSlider.value = currentHp;  // Sliderの現在値をcurrentHpと同期
        }
        else
        {
            Debug.LogWarning("HPBar SliderがPadControllerに設定されていません！");
        }
        // --- 追加ここまで ---
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
        float moveInput = Input.GetAxis("Horizontal");
        Vector2 newPosition = rb.position + Vector2.right * moveInput * speed * Time.fixedDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary);
        rb.MovePosition(newPosition);
    }

    private void HandleSpawning()
    {
        if (spawnTimer > 0f)
        {
            spawnTimer -= Time.deltaTime;
        }
        
        if (Input.GetMouseButton(0) && spawnTimer <= 0f)
        {
            if (Ball.currentBallCount >= maxBalls)
            {
                return; 
            }
            
            LaunchBallTowardsMouse();
            spawnTimer = spawnInterval;
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
        if (isInvincible || !isAlive)
        {
            return;
        }

        currentHp -= amount;
        Debug.Log("Player HP: " + currentHp);

        // --- ここから追加 ---
        // ★4. ダメージを受けた時にHPバーの現在値を更新
        if (hpBarSlider != null)
        {
            hpBarSlider.value = currentHp;
        }
        // --- 追加ここまで ---

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
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
        Debug.Log("プレイヤーは動けなくなりました");
    }
}