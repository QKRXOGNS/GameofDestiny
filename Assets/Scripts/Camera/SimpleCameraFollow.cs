// ============================================================================
// SimpleCameraFollow.cs
// 카메라가 플레이어를 따라다니는 간단한 스크립트
// ============================================================================

using UnityEngine;

/// <summary>
/// 카메라가 대상 오브젝트를 따라다니며, Y위치는 고정, X는 부드럽게 이동
/// </summary>
public class SimpleCameraFollow : MonoBehaviour
{
    // ========================================================================
    // 설정
    // ========================================================================
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);  // 카메라 위치 오프셋
    
    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;  // 이동 부드러움 (클수록 빠름)
    [SerializeField] private bool lookAtTarget = false;  // 타겟 바라보기
    
    // ========================================================================
    // 업데이트
    // ========================================================================
    private void LateUpdate()
    {
        if (target == null) return;
        
        // 목표 위치 계산 (X, Y, Z 모두 target 따라가기)
        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            target.position.z + offset.z
        );
        
        // 부드러운 이동
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            smoothSpeed * Time.deltaTime
        );
        
        // 타겟 바라보기
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
    
    // ========================================================================
    // 초기화
    // ========================================================================
    private void Start()
    {
        // 태그로 플레이어 자동 탐색
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
}
