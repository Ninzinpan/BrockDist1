using UnityEngine;

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

    // --- ここから追加 ---
    [Header("Ball Limit")]
    public int maxBalls = 10; // フィールドに出せるボールの最大数
    // --- 追加ここまで ---

    private float spawnTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTimer = 0f;
    }

    void FixedUpdate() 
    {
        HandleMovement();
    }

    void Update()
    {
        HandleSpawning();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        Vector2 newPosition = rb.position + Vector2.right * moveInput * speed * Time.fixedDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary);
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// ボール発射の入力とタイマーを管理する
    /// </summary>
    private void HandleSpawning()
    {
        if (spawnTimer > 0f)
        {
            spawnTimer -= Time.deltaTime;
        }

        // マウスホールド AND タイマーOK かチェック
        if (Input.GetMouseButton(0) && spawnTimer <= 0f)
        {
            // --- ここから変更 ---
            // ★追加：ボールの最大数チェック
            if (Ball.currentBallCount >= maxBalls)
            {
                return; // 上限に達しているので、ここで処理を中断（発射しない）
            }
            // --- 変更ここまで ---

            // 3. 発射処理を呼び出す
            LaunchBallTowardsMouse();
            
            // 4. タイマーをリセット
            spawnTimer = spawnInterval;
        }
    }

    private void LaunchBallTowardsMouse()
    {
        // (LaunchBallTowardsMouse の中身は変更なし)
        
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
}