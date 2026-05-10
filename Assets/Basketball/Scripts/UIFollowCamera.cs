using UnityEngine;

public class UIFollowCamera : MonoBehaviour
{
    [Header("追従対象（VRカメラ）")]
    public Transform targetCamera;

    [Header("設定")]
    public float distance = 1.5f; // カメラからの距離(m)

    // SmoothTimeなどの遅延設定は削除しました

    void Start()
    {
        // カメラが未設定なら自動で探す
        if (targetCamera == null)
        {
            if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // --- 1. 位置の計算 ---
        // カメラの正面(distanceメートル先)に、遅延なしで配置
        transform.position = targetCamera.position + (targetCamera.forward * distance);

        // --- 2. 向きの計算 ---
        // カメラと全く同じ回転にする（常に真正面を向く）
        transform.rotation = targetCamera.rotation;
    }
}