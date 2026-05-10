using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("JSONデータ設定")]
    // ★変更: 複数のファイル名を登録できるように変更
    public string[] jsonFileNames = new string[] { "pro1", "pro2" };
    private int currentFileIndex = 0; // 現在再生中のファイル番号

    private FrameData[] replayFrames;

    [Header("UI（デバッグ用）")]
    public TextMeshProUGUI frameCounterText;

    [Header("アバター")]
    public GameObject team1PlayerPrefab;
    public GameObject team2PlayerPrefab;

    [Header("座標・設定")]
    [SerializeField] private float positionScale = 0.1f;
    public float heightOffset = 0.0f;
    public bool flipHorizontal = false;
    public bool flipVertical = false;
    public float zOffsetCorrection = 150.0f;

    // 生成オブジェクト管理
    private Dictionary<int, GameObject> activePlayers = new Dictionary<int, GameObject>();

    [Header("再生コントロール")]
    [SerializeField] private float baseFrameRate = 30.0f;
    public float playbackSpeed = 1.0f;
    public bool IsPlaying { get; private set; } = false;

    public float Progress
    {
        get
        {
            if (replayFrames == null || replayFrames.Length == 0) return 0f;
            return (float)currentFrameIndex / (replayFrames.Length - 1);
        }
    }

    private float timer = 0f;
    private int currentFrameIndex = 0;

    void Start()
    {
        // 最初のデータをロード
        LoadJsonData(currentFileIndex);
        IsPlaying = false;
    }

    void Update()
    {
        if (replayFrames == null || replayFrames.Length == 0) return;

        if (IsPlaying)
        {
            timer += Time.deltaTime * playbackSpeed;
            float interval = 1.0f / baseFrameRate;

            if (timer >= interval)
            {
                timer -= interval;
                currentFrameIndex++;
                if (currentFrameIndex >= replayFrames.Length - 1)
                {
                    currentFrameIndex = 0;
                }
            }
        }

        // 補間処理
        float intervalCalc = 1.0f / baseFrameRate;
        float t = IsPlaying ? (timer / intervalCalc) : 0;
        UpdateInterpolatedFrame(currentFrameIndex, t);
    }

    // --- 操作メソッド ---

    public void TogglePlayPause()
    {
        IsPlaying = !IsPlaying;
    }

    public void SeekTo(float normalizedTime)
    {
        if (replayFrames == null || replayFrames.Length == 0) return;
        normalizedTime = Mathf.Clamp01(normalizedTime);
        currentFrameIndex = Mathf.FloorToInt((replayFrames.Length - 1) * normalizedTime);
        timer = 0;
        UpdateInterpolatedFrame(currentFrameIndex, 0);
    }

    public void SetSpeed(float speed)
    {
        playbackSpeed = Mathf.Max(0, speed);
    }

    // ★追加: 次のデータへ切り替えるメソッド
    public void SwitchToNextData()
    {
        currentFileIndex++;
        // 最後のファイルまでいったら最初に戻る
        if (currentFileIndex >= jsonFileNames.Length)
        {
            currentFileIndex = 0;
        }

        // 一旦停止してロードし直す
        IsPlaying = false;
        LoadJsonData(currentFileIndex);
    }

    // ---------------------------------------------

    // ★修正: インデックスを指定してロードする形に変更
    void LoadJsonData(int index)
    {
        // 1. 前のデータをクリア（重要）
        foreach (var kvp in activePlayers)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        activePlayers.Clear();

        // 2. 変数リセット
        currentFrameIndex = 0;
        timer = 0;

        // 3. ファイル読み込み
        if (index < 0 || index >= jsonFileNames.Length) return;
        string fileName = jsonFileNames[index];

        if (frameCounterText != null) frameCounterText.text = $"Loading: {fileName}...";

        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            replayFrames = JsonHelper.FromJson<FrameData>(jsonFile.text);
            if (replayFrames != null && replayFrames.Length > 0)
            {
                Debug.Log($"Load OK: {fileName} ({replayFrames.Length} frames)");
                // 最初のフレームを描画して待機
                UpdateInterpolatedFrame(0, 0);
            }
        }
        else
        {
            Debug.LogError($"JSON not found: {fileName}");
            if (frameCounterText != null) frameCounterText.text = "Data Not Found";
            replayFrames = null;
        }
    }

    void UpdateInterpolatedFrame(int frameIdx, float t)
    {
        if (frameCounterText != null) frameCounterText.text = $"Data: {jsonFileNames[currentFileIndex]} | Frame: {frameIdx}";
        if (replayFrames == null || frameIdx < 0 || frameIdx >= replayFrames.Length - 1) return;

        FrameData currFrame = replayFrames[frameIdx];
        FrameData nextFrame = replayFrames[frameIdx + 1];

        Dictionary<int, PlayerData> nextPlayersDict = new Dictionary<int, PlayerData>();
        if (nextFrame.players != null) foreach (var p in nextFrame.players) nextPlayersDict[p.tracker_id] = p;

        HashSet<int> currentFrameIds = new HashSet<int>();
        if (currFrame.players != null)
        {
            foreach (var player in currFrame.players)
            {
                int pid = player.tracker_id;
                currentFrameIds.Add(pid);
                if (!activePlayers.ContainsKey(pid))
                {
                    GameObject prefab = (player.team_id == 0) ? team1PlayerPrefab : team2PlayerPrefab;
                    if (prefab == null) prefab = team1PlayerPrefab;
                    if (prefab != null)
                    {
                        GameObject newPlayer = Instantiate(prefab);
                        newPlayer.name = $"Player_{pid}_Team{player.team_id}";
                        activePlayers.Add(pid, newPlayer);
                    }
                }

                if (activePlayers.ContainsKey(pid))
                {
                    GameObject obj = activePlayers[pid];
                    Vector3 startPos = GetUnityPosition(player.x, player.y);
                    if (nextPlayersDict.ContainsKey(pid))
                    {
                        PlayerData nextP = nextPlayersDict[pid];
                        Vector3 endPos = GetUnityPosition(nextP.x, nextP.y);
                        obj.transform.position = Vector3.Lerp(startPos, endPos, t);
                        if (Vector3.Distance(startPos, endPos) > 0.001f) obj.transform.LookAt(endPos);
                    }
                    else
                    {
                        obj.transform.position = startPos;
                    }
                }
            }
        }
        List<int> idsToRemove = new List<int>();
        foreach (var kvp in activePlayers) if (!currentFrameIds.Contains(kvp.Key)) { Destroy(kvp.Value); idsToRemove.Add(kvp.Key); }
        foreach (int id in idsToRemove) activePlayers.Remove(id);
    }

    Vector3 GetUnityPosition(float rawX, float rawY)
    {
        if (flipHorizontal) rawX = -rawX;
        if (flipVertical) rawY = -rawY;
        Vector3 pos = new Vector3(rawX * positionScale, heightOffset, rawY * positionScale);
        if (flipVertical) pos.z += zOffsetCorrection;
        return pos;
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"Items\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }
    [System.Serializable] private class Wrapper<T> { public T[] Items; }
}