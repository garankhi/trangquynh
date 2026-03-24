using System.Collections;
using UnityEngine;

/// <summary>
/// NPC đi lại giữa các waypoint trong chợ.
/// Dừng lại xem hàng (Idle) rồi đi tiếp (Walk).
/// </summary>
public class MarketNPCWander : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Danh sách các điểm NPC sẽ đi qua")]
    public Transform[] waypoints;

    [Header("Movement")]
    [Tooltip("Tốc độ đi bộ")]
    public float walkSpeed = 1.5f;

    [Tooltip("Khoảng cách tối thiểu để coi là đã đến waypoint")]
    public float arrivalDistance = 0.3f;

    [Header("Wait Time (xem hàng)")]
    [Tooltip("Thời gian dừng tối thiểu (giây)")]
    public float minWaitTime = 2f;

    [Tooltip("Thời gian dừng tối đa (giây)")]
    public float maxWaitTime = 5f;

    [Header("Rotation")]
    [Tooltip("Tốc độ xoay hướng")]
    public float rotationSpeed = 5f;

    [Header("Ground Snapping")]
    [Tooltip("Layer mặt đất để raycast")]
    public LayerMask groundLayer = ~0; // Mặc định = tất cả layer
    [Tooltip("Độ cao raycast bắt đầu (từ trên NPC chiếu xuống)")]
    public float raycastHeight = 5f;
    [Tooltip("Khoảng cách raycast tối đa")]
    public float raycastDistance = 10f;

    [Header("Animation")]
    [Tooltip("Tên parameter bool trong Animator")]
    public string walkParameter = "isWalking";

    [Tooltip("Tên parameter bool để kích hoạt action (BlendTree)")]
    public string actingParameter = "isActing";

    [Tooltip("Tên parameter int để chọn action animation")]
    public string idleIndexParameter = "idleIndex";

    [Tooltip("Số lượng action animation trong BlendTree")]
    public int idleAnimationCount = 3;

    [Header("Idle Pause (nghỉ sau action)")]
    [Tooltip("Thời gian Idle trước khi đi tiếp (giây)")]
    public float idlePauseTime = 1f;

    private Animator _animator;
    private int _currentWaypointIndex;
    private bool _isWaiting;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[MarketNPCWander] {gameObject.name}: Chưa gán waypoint!");
            enabled = false;
            return;
        }

        // Bắt đầu từ waypoint gần nhất
        _currentWaypointIndex = GetClosestWaypointIndex();
        SnapToGround(); // Đặt NPC xuống mặt đất ngay từ đầu
        SetWalking(true);
    }

    private void Update()
    {
        if (_isWaiting || waypoints.Length == 0) return;

        Transform target = waypoints[_currentWaypointIndex];
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // Chỉ di chuyển trên mặt phẳng XZ

        float distance = direction.magnitude;

        if (distance <= arrivalDistance)
        {
            // Đã đến waypoint → dừng lại xem hàng
            StartCoroutine(WaitAtWaypoint());
            return;
        }

        // Di chuyển về phía waypoint
        Vector3 moveDir = direction.normalized;
        transform.position += moveDir * walkSpeed * Time.deltaTime;

        // Snap xuống mặt đất mỗi frame
        SnapToGround();

        // Xoay mặt về hướng đi
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Raycast xuống để đặt NPC lên mặt đất.
    /// </summary>
    private void SnapToGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y;
            transform.position = pos;
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        _isWaiting = true;
        SetWalking(false); // Walk → dừng di chuyển

        // Lấy MarketWaypoint component (nếu có)
        Transform currentWP = waypoints[_currentWaypointIndex];
        MarketWaypoint wpData = currentWP.GetComponent<MarketWaypoint>();

        // Quay mặt theo hướng waypoint
        if (wpData != null && wpData.useFacingDirection)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentWP.forward);
            transform.rotation = targetRot;
        }

        // === PHASE 1: Action (BlendTree) ===
        int randomIdle;
        if (wpData != null)
        {
            randomIdle = wpData.GetRandomIdleIndex();
        }
        else
        {
            randomIdle = Random.Range(0, idleAnimationCount);
        }

        if (_animator != null)
        {
            _animator.SetFloat(idleIndexParameter, randomIdle);
            _animator.SetBool(actingParameter, true); // → ActionBlendTree
        }

        float actionTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(actionTime);

        // === PHASE 2: Quay về Idle ===
        if (_animator != null)
        {
            _animator.SetBool(actingParameter, false); // → Idle
        }

        yield return new WaitForSeconds(idlePauseTime);

        // === PHASE 3: Đi tiếp ===
        _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
        SetWalking(true);
        _isWaiting = false;
    }

    private void SetWalking(bool walking)
    {
        if (_animator != null)
        {
            _animator.SetBool(walkParameter, walking);
        }
    }

    /// <summary>
    /// Tìm waypoint gần NPC nhất để bắt đầu.
    /// </summary>
    private int GetClosestWaypointIndex()
    {
        int closest = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }

        return closest;
    }

    // Vẽ đường đi trong Scene view để dễ debug
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Vẽ điểm waypoint
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

            // Vẽ đường nối giữa các waypoint
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }
    }
}
