using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(LineRenderer))]
public class VRInputController : MonoBehaviour
{
    [Header("参照")]
    public VRPlayerTracker playerTracker;
    public GameController gameController;

    [Header("UI設定")]
    // ★追加: 表示/非表示を切り替えたいUIオブジェクト（VR_UI または Canvas）
    public GameObject uiTarget;

    [Header("レーザー設定")]
    public float maxDistance = 50.0f;
    public Color normalColor = Color.red;
    public Color hoverColor = Color.green;

    // 内部変数
    private LineRenderer lineRenderer;

    // 右手用フラグ
    private bool wasTriggerPressed = false;   // 選択
    private bool wasSecondaryPressed = false; // Bボタン(俯瞰)
    private bool wasPrimaryPressed = false;   // Aボタン(再生)

    // ★追加: 左手用フラグ
    private bool wasMenuPressed = false;      // Xボタン(UI表示切替)

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        // --- 1. アクションフラグ ---
        bool isSelectAction = false;    // 選手選択
        bool isCancelAction = false;    // 俯瞰に戻る / アングル切替
        bool isPlayPauseAction = false; // 再生切り替え
        bool isToggleUIAction = false;  // ★追加: UI表示切り替え

        // --- 2. デバイス入力の取得 ---

        // 【右手】 (Right Controller)
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightDevice.isValid)
        {
            // トリガー (選択)
            bool triggerValue = false;
            if (rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue))
            {
                if (triggerValue && !wasTriggerPressed) isSelectAction = true;
                wasTriggerPressed = triggerValue;
            }

            // Bボタン (Secondary - キャンセル/視点切替)
            bool secondaryValue = false;
            if (rightDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue))
            {
                if (secondaryValue && !wasSecondaryPressed) isCancelAction = true;
                wasSecondaryPressed = secondaryValue;
            }

            // Aボタン (Primary - 再生/停止)
            bool primaryValue = false;
            if (rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue))
            {
                if (primaryValue && !wasPrimaryPressed) isPlayPauseAction = true;
                wasPrimaryPressed = primaryValue;
            }
        }

        // 【左手】 (Left Controller) ★追加
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftDevice.isValid)
        {
            // Xボタン (Primary - UI表示切替)
            // ※Questでは左手のPrimaryがX、SecondaryがYです
            bool menuValue = false;
            if (leftDevice.TryGetFeatureValue(CommonUsages.primaryButton, out menuValue))
            {
                if (menuValue && !wasMenuPressed) isToggleUIAction = true;
                wasMenuPressed = menuValue;
            }
        }

        // 【PCデバッグ用】 (マウス & キーボード)
        if (!rightDevice.isValid && !leftDevice.isValid)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) isSelectAction = true;
            if (Input.GetKeyDown(KeyCode.Mouse1)) isCancelAction = true;
            if (Input.GetKeyDown(KeyCode.Space)) isPlayPauseAction = true;
            if (Input.GetKeyDown(KeyCode.M)) isToggleUIAction = true; // ★追加: MキーでUI切替
        }

        // --- 3. アクション実行 ---

        // ★追加: UIの表示/非表示切り替え
        if (isToggleUIAction && uiTarget != null)
        {
            // アクティブ状態を反転させる (ONならOFFへ、OFFならONへ)
            uiTarget.SetActive(!uiTarget.activeSelf);
        }

        // 再生・一時停止
        if (isPlayPauseAction && gameController != null)
        {
            gameController.TogglePlayPause();
        }

        // キャンセル / 視点切り替え
        if (isCancelAction)
        {
            if (playerTracker != null)
            {
                if (playerTracker.targetPlayer != null)
                    playerTracker.ClearTarget();
                else
                    playerTracker.CycleOverheadView();
            }
            return;
        }

        // レーザーと選択処理 (UIが非表示のときでも選択できた方が便利なのでそのまま実行)
        if (playerTracker != null)
        {
            UpdateLaserAndSelection(isSelectAction);
        }
    }

    void UpdateLaserAndSelection(bool isSelectClicked)
    {
        // (以前と同じコードのため省略なしで記述)
        Ray ray;
        if (!InputDevices.GetDeviceAtXRNode(XRNode.RightHand).isValid && Application.isEditor)
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
            ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;
        bool isHit = Physics.Raycast(ray, out hit, maxDistance);

        lineRenderer.SetPosition(0, transform.position);

        if (isHit)
        {
            lineRenderer.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("Player"))
            {
                lineRenderer.startColor = hoverColor;
                lineRenderer.endColor = hoverColor;
                if (isSelectClicked) playerTracker.SetTarget(hit.collider.gameObject);
            }
            else
            {
                lineRenderer.startColor = normalColor;
                lineRenderer.endColor = normalColor;
            }
        }
        else
        {
            lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
            lineRenderer.startColor = normalColor;
            lineRenderer.endColor = normalColor;
        }
    }
}