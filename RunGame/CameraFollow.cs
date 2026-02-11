using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 主角的Transform（在Inspector中拖拽赋值）
    public float smoothSpeed = 0.125f; // 相机跟随的平滑度
    public Vector3 offset; // 相机与主角的相对偏移（可微调垂直/深度位置）

    void LateUpdate()
    {
        if (target == null) return; // 防止主角未赋值时报错

        // 计算目标位置：主角位置 + 偏移量
        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            offset.y, // 垂直位置固定，可根据需要调整
            offset.z  // 深度位置固定，2D场景一般设为-10
        );

        // 平滑移动相机（让跟随更自然）
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}