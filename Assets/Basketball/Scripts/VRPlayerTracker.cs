using UnityEngine;
using System.Collections.Generic; // ★リストを使うために必要

public class VRPlayerTracker : MonoBehaviour
{
    [Header("追尾設定")]
    public Transform targetPlayer;
    public float positionSmoothSpeed = 0.1f;
    public float rotationSmoothSpeed = 0.05f;

    [Header("視点の高さ")]
    public float eyeHeight = 1.7f;

    [Header("俯瞰（ふかん）視点設定")]
    // ★変更: 1つのTransformから、リストに変更
    public List<Transform> overheadTransforms = new List<Transform>();

    // 現在どの俯瞰視点を使っているかの番号（0番、1番...）
    private int currentOverheadIndex = 0;

    private Renderer targetRenderer;

    void LateUpdate()
    {
        // A. 選手追尾モード
        if (targetPlayer != null)
        {
            // 位置の移動
            Vector3 targetPos = targetPlayer.position + new Vector3(0, eyeHeight, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, positionSmoothSpeed);

            // 角度の補正（水平に戻す処理）
            Vector3 currentEuler = transform.rotation.eulerAngles;
            Quaternion uprightRotation = Quaternion.Euler(0, currentEuler.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, uprightRotation, rotationSmoothSpeed);
        }
        // B. 俯瞰モード
        else if (overheadTransforms != null && overheadTransforms.Count > 0)
        {
            // ★変更: リストから現在の番号の場所を取得
            Transform targetOverhead = overheadTransforms[currentOverheadIndex];

            // その場所へ移動・回転
            transform.position = Vector3.Lerp(transform.position, targetOverhead.position, positionSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetOverhead.rotation, rotationSmoothSpeed);
        }
    }

    /// <summary>
    /// ★追加: 次の俯瞰視点へ切り替える
    /// </summary>
    public void CycleOverheadView()
    {
        if (overheadTransforms == null || overheadTransforms.Count == 0) return;

        // 番号を1つ進める
        currentOverheadIndex++;

        // もし最後の番号を超えたら、0番（最初）に戻す
        if (currentOverheadIndex >= overheadTransforms.Count)
        {
            currentOverheadIndex = 0;
        }
        Debug.Log($"[VRTracker] 俯瞰視点切り替え: パターン {currentOverheadIndex + 1}");
    }

    public void SetTarget(GameObject newTarget)
    {
        if (targetRenderer != null) targetRenderer.enabled = true;
        targetPlayer = newTarget.transform;
        targetRenderer = newTarget.GetComponentInChildren<Renderer>();
        if (targetRenderer != null) targetRenderer.enabled = false;

        Debug.Log($"[VRTracker] 視点を {newTarget.name} に切り替えました");
    }

    public void ClearTarget()
    {
        if (targetRenderer != null) targetRenderer.enabled = true;
        targetPlayer = null;
        targetRenderer = null;

        // 俯瞰に戻るときは、とりあえず0番（メイン）に戻すか、
        // あるいは前回の場所を維持するか選べますが、今回は「維持」します。
        Debug.Log("[VRTracker] 俯瞰視点に戻ります");
    }
}