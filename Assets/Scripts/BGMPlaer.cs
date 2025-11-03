using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMPlayer : MonoBehaviour
{
    // どこからでも BGMPlayer.Instance でアクセスできるようにする
    public static BGMPlayer Instance { get; private set; }

    private AudioSource audioSource;

    void Awake()
    {
        // 1. シングルトン ＋ 永続化ロジック
        if (Instance != null && Instance != this)
        {
            // 既に BGMPlayer が存在する場合（例: Stage1から持ち越された）
            // この（新しくロードされた）BGMPlayer を破棄
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいで永続化

        // 2. 自分の AudioSource を取得
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// BGMを（最初から）再生する
    /// </summary>
    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// BGMを停止する
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}