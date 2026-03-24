using UnityEngine;

/// <summary>
/// Gắn lên mỗi Waypoint GameObject.
/// Cấu hình animation nào NPC sẽ chơi và hướng quay mặt khi dừng tại đây.
/// </summary>
public class MarketWaypoint : MonoBehaviour
{
    [Header("Facing Direction")]
    [Tooltip("Hướng NPC quay mặt khi dừng tại waypoint này (dùng mũi tên xanh Z của Transform)")]
    public bool useFacingDirection = true;

    [Header("Allowed Idle Animations")]
    [Tooltip("Danh sách idleIndex được phép chơi tại waypoint này (0=Idle, 1=PointingForward, 2=LookAround, 3=OldManIdle)")]
    public int[] allowedIdleIndices = { 0 };

    /// <summary>
    /// Random chọn 1 idleIndex từ danh sách cho phép.
    /// </summary>
    public int GetRandomIdleIndex()
    {
        if (allowedIdleIndices == null || allowedIdleIndices.Length == 0)
            return 0;
        return allowedIdleIndices[Random.Range(0, allowedIdleIndices.Length)];
    }

    // Vẽ mũi tên hướng quay mặt trong Scene view
    private void OnDrawGizmos()
    {
        if (!useFacingDirection) return;

        Gizmos.color = Color.yellow;
        Vector3 from = transform.position + Vector3.up * 0.5f;
        Vector3 to = from + transform.forward * 1.5f;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(to, 0.1f);
    }
}
