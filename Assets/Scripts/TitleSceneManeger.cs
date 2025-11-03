using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random; // System..Random との衝突を避ける

public class TitleScreenManager : MonoBehaviour
{
    [Header("Game Scene Settings")]
    public string gameSceneName = "Stage1"; // ゲームを開始するシーン名

    [Header("Menu Balls Settings")]
    public GameObject menuBallPrefab; // ★ 1. 動くボールのプレハブ
    public int numberOfBalls = 15;    // ★ 2. 生成するボールの数
    public float minSpawnX = -8f;     // ★ 3. ボールの生成範囲X
    public float maxSpawnX = 8f;
    public float minSpawnY = -4f;     // ★ 4. ボールの生成範囲Y
    public float maxSpawnY = 4f;
    public float minInitialSpeed = 2f; // ★ 5. ボールの初期速度
    public float maxInitialSpeed = 5f;

    void Start()
    {
        // ★ 6. メニューボールを生成・配置
        if (menuBallPrefab != null)
        {
            GenerateMenuBalls();
        }
    }

    private void GenerateMenuBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            // ランダムな位置を生成
            Vector3 randomPos = new Vector3(
                Random.Range(minSpawnX, maxSpawnX),
                Random.Range(minSpawnY, maxSpawnY),
                0f
            );

            // ボールを生成
            GameObject ball = Instantiate(menuBallPrefab, randomPos, Quaternion.identity);

            // Rigidbody2D を取得
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // ランダムな方向と速度で飛ばす
                Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                float randomSpeed = Random.Range(minInitialSpeed, maxInitialSpeed);
                rb.linearVelocity = randomDirection * randomSpeed;
            }
            else
            {
                Debug.LogWarning("MenuBallPrefab に Rigidbody2D が見つかりません！");
            }
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}