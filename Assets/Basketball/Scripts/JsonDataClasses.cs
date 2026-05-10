using System.Collections.Generic;
using UnityEngine;

/*
 * pro1.json の構造に対応したデータクラス定義
 * JSON例:
 * [
 * {
 * "frame_index": 0,
 * "players": [
 * { "tracker_id": 1, "team_id": 1, "x": 37.3, "y": 41.3 },
 * ...
 * ]
 * },
 * ...
 * ]
 */

/// <summary>
/// JSON内の "players" リストの各要素
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int tracker_id; // "player_id" から int型の "tracker_id" に変更
    public int team_id;
    public float x;        // "position" リストから単体の float x, y に変更
    public float y;
}

/// <summary>
/// JSON内の各フレーム要素
/// </summary>
[System.Serializable]
public class FrameData
{
    public int frame_index; // "frame_number" から変更
    public List<PlayerData> players;
}

// JSONのルートが配列 [...] なので、RootDataクラスは削除しました。
// 読み込み時に JsonHelper を使用して FrameData[] として扱います。