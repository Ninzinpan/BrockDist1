using System.Collections.Generic;
using UnityEngine;

// Unityエディタの「Create」メニューに項目を追加する
[CreateAssetMenu(fileName = "NewStageData", menuName = "Blockfall Roguelike/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Scene Management")]
    // このステージをクリアした後にロードするシーンの名前
    public string nextSceneName;

    [Header("Display")]
    // StageStartUI に表示するステージ名の「翻訳キー」
    // (LocalizationManager で "stage_name_1" -> "ステージ 1" のように使います)
    public string stageDisplayNameKey;

    [Header("Gameplay Data")]
    // このステージのアップグレードUIで出現するバフのプール
    // （将来、ここに "EnemySpawnList" などを追加して拡張します）
    public List<BuffData> availableBuffs;
    
    // （例: 将来の拡張用）
    // [Header("Enemy Data")]
    // public List<GameObject> enemyPrefabs;
    // public int enemyCount;
}