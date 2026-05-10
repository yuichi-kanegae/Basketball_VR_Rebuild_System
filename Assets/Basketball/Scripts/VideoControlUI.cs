using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoControlUI : MonoBehaviour
{
    [Header("参照")]
    public GameController gameController;

    [Header("UI要素")]
    public Button playPauseButton;
    public TextMeshProUGUI playButtonText;

    public Slider timeSlider;
    public Slider speedSlider;
    public TextMeshProUGUI speedValueText;

    [Header("★追加: データ切り替え")]
    public Button changeDataButton; // 新しいボタン

    [Header("設定")]
    public string playText = "play";
    public string pauseText = "pause";

    private bool isDraggingSlider = false;

    void Start()
    {
        if (playPauseButton) playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        if (timeSlider) timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        if (speedSlider) speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);

        // ★追加: データ切り替えボタンのイベント登録
        if (changeDataButton != null)
        {
            changeDataButton.onClick.AddListener(OnChangeDataClicked);
        }

        // 初期化
        if (speedSlider)
        {
            speedSlider.minValue = 0.0f;
            speedSlider.maxValue = 2.0f;
            speedSlider.value = 1.0f; // 初期値1.0倍速
            OnSpeedSliderChanged(1.0f);
        }
    }

    void Update()
    {
        if (gameController == null) return;

        if (playButtonText != null)
            playButtonText.text = gameController.IsPlaying ? pauseText : playText;

        if (!isDraggingSlider && timeSlider != null)
        {
            timeSlider.SetValueWithoutNotify(gameController.Progress);
        }
    }

    public void OnPlayPauseClicked()
    {
        gameController.TogglePlayPause();
    }

    public void OnTimeSliderChanged(float value)
    {
        gameController.SeekTo(value);
    }

    public void OnSpeedSliderChanged(float value)
    {
        gameController.SetSpeed(value);
        if (speedValueText != null) speedValueText.text = $"x {value:F1}";
    }

    // ★追加: データ切り替え処理
    public void OnChangeDataClicked()
    {
        if (gameController != null)
        {
            gameController.SwitchToNextData();
        }
    }
}