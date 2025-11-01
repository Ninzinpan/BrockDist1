using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ★ 1. これを追加

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PadController : MonoBehaviour
{
    [Header("Movement")]
    public float boundary = 8f;
    private Rigidbody2D rb;

    [Header("Ball Launching")]
    public GameObject ballPrefab;
    public Transform spawnPoint;

    [Header("Player Stats")]
    public float invincibilityDuration = 1f;

    [Header("UI")]
    // ★ 2. Inspectorからの設定は不要になる（自動で探すため）
    // public Slider hpBarSlider; 
    private Slider hpBarSlider; // private に変更

    // (内部変数)
    private float currentHp;
    private bool isInvincible = false;
    private bool isAlive = true;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float spawnTimer;
    private PlayerStats playerStats; 

    public bool IsDebugMode = false; // デバッグ用フラグ

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
        
        // ★ 3. シーンロード時のイベントを購読
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // ★ 4. PlayerStats のイベントを購読
        playerStats.OnStatsRecalculated.AddListener(UpdateHpBarMax);
        
        // ★ 5. 最初のシーン（Stage1）のHPバーを探す
        FindAndAssignHPBar();
    }
    
    // ★ 6. 以下の3つのメソッドをまるごと追加
    
    /// <summary>
    /// オブジェクト破棄時にイベント購読を解除
    /// </summary>
    void OnDestroy()
    {
        // イベントの購読を解除（メモリリーク防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (playerStats != null)
        {
            playerStats.OnStatsRecalculated.RemoveListener(UpdateHpBarMax);
        }
    }

    /// <summary>
    /// シーンがロードされるたびに呼ばれる
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新しいシーンのHPバーを探して再接続する
        FindAndAssignHPBar();
    }
    
    /// <summary>
    /// シーン内から "HPBar" タグを探してアタッチし、表示を更新する
    /// </summary>
    private void FindAndAssignHPBar()
    {
        // "HPBar" タグを持つゲームオブジェクトを探す
        GameObject hpBarGO = GameObject.FindGameObjectWithTag("HPBar");
        
        if (hpBarGO != null)
        {
            // 見つけたオブジェクトから Slider コンポーネントを取得
            hpBarSlider = hpBarGO.GetComponent<Slider>();
            if (hpBarSlider != null)
            {
                // HPバーが見つかったら、即座に最大値と現在値を更新
                UpdateHpBarMax();
                UpdateHpBar();
            }
            else
            {
                Debug.LogError("HPBarタグのオブジェクトに Slider コンポーネントがありません！");
            }
        }
        else
        {
            Debug.LogWarning("このシーンには 'HPBar' タグのオブジェクトが見つかりません。");
            hpBarSlider = null; // 見つからなかった場合は null にしておく
        }
    }

    // (FixedUpdate, Update, HandleMovement, HandleSpawning, LaunchBallTowardsMouse, 
    //  TakeDamage, Heal, InvincibilityRoutine, Die は変更なし)
    
    // (UpdateHpBar, UpdateHpBarMax は null チェックがあるので変更なし)

    // (↓ 変更なしのメソッド群)
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
        Vector2 newPosition = rb.position + Vector2.right * moveInput * playerStats.CurrentSpeed * Time.fixedDeltaTime;
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
            Time.timeScale = 0f;
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
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
        GameManager.Instance.StartGameOverSequence();
    }
    /// <summary>
/// PlayerStatsから呼ばれ、HPや生存状態をリセットする
/// </summary>
public void ResetState()
    {
    // PlayerStatsから最新の最大HPを取得して全回復
    currentHp = playerStats.CurrentMaxHp;
    isAlive = true;

    // 色を元に戻す
    if(spriteRenderer != null)
    {
        spriteRenderer.color = originalColor;
    }

    // HPバーを（見つけていれば）更新
    UpdateHpBar();
    }
}