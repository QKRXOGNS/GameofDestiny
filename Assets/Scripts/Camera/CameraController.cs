// ============================================================================
// CameraController.cs
// 2.5D 게임용 카메라 컨트롤러 - 줌인/줌아웃 + Cheating Perspective
// 마우스 휠: 줌인/줌아웃
// ============================================================================

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // ==================== 타겟 ====================
    [Header("Target")]
    [SerializeField] private Transform target;
    
    // ==================== 위치 오프셋 ====================
    [Header("Offset")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0f, 8f, -8f);  // 기본 카메라 위치 오프셋
    
    // ==================== 줌 설정 ====================
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;        // 줌 속도
    [SerializeField] private float minZoom = 1f;          // 최소 거리 (줌인)
    [SerializeField] private float maxZoom = 4f;          // 최대 거리 (줌아웃)
    [SerializeField] private float currentZoom = 2f;       // 현재 줌 값
    
    // ==================== 회전 설정 (Cheating Perspective) ====================
    [Header("Rotation - Cheating Perspective")]
    [SerializeField] private float baseRotationX = 45f;   // 기본 X축 회전 (비스듬한 각도)
    [SerializeField] private float rotationSpeed = 3f;     // 회전 속도
    [SerializeField] private float minRotationX = 20f;     // 최소 회전 (더 수직)
    [SerializeField] private float maxRotationX = 70f;     // 최대 회전 (더 수평)
    
    [Header("Zoom-Rotation Curve")]
    [Tooltip("X축: 정규화된 줌 값 (0=줌인, 1=줌아웃), Y축: 회전각 배율 (0=수직, 1=수평)")]
    [SerializeField] private AnimationCurve zoomRotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    // ==================== 이동 설정 ====================
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;       // 따라가는 속도
    [SerializeField] private bool followX = true;          // X축 따라가기
    [SerializeField] private bool followY = true;           // Y축 따라가기
    
    // ==================== 내부 변수 ====================
    private float currentRotationX;
    private float currentRotationZ;
    
    // ========================================================================
    // 초기화
    // ========================================================================
    private void Start()
    {
        // 타겟 자동 탐색
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // 초기 회전값 설정 (기본 20도)
        currentRotationX = 27.526f;
        currentRotationZ = 0f;
        
        // 초기 줌 값도 20도에 맞게 설정
        currentZoom = (minZoom + maxZoom) / 2f;
        
        // 카메라 초기 회전 적용
        ApplyRotation();
    }
    
    // ========================================================================
    // 업데이트
    // ========================================================================
    private void LateUpdate()
    {
        // 줌 입력 처리
        HandleZoomInput();
        
        // 회전 입력 처리
        HandleRotationInput();
        
        // 카메라 위치 업데이트
        UpdateCameraPosition();
    }
    
    // ========================================================================
    // 줌인/줌아웃 (마우스 휠) - Input System
    // ========================================================================
    private void HandleZoomInput()
    {
        // 마우스 휠로 줌인/줌아웃 (Input System)
        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
        if (scrollDelta.y != 0f)
        {
            currentZoom -= scrollDelta.y * zoomSpeed * 0.01f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            UpdateRotationFromZoom(); // 줌에 따라 각도 자동 조정
        }
        
        // Q/E 키로 줌인/줌아웃 (Input System)
        if (Keyboard.current.qKey.isPressed)
        {
            currentZoom -= zoomSpeed * Time.deltaTime * 2f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            UpdateRotationFromZoom(); // 줌에 따라 각도 자동 조정
        }
        if (Keyboard.current.eKey.isPressed)
        {
            currentZoom += zoomSpeed * Time.deltaTime * 2f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            UpdateRotationFromZoom(); // 줌에 따라 각도 자동 조정
        }
    }
    
    // ========================================================================
    // 줌 값에 따라 회전 각도 자동 조정 (커스텀 곡선 사용)
    // AnimationCurve: X=정규화된 줌, Y=회전값 (0~1)
    // ========================================================================
    private void UpdateRotationFromZoom()
    {
        // 줌 범위를 정규화 (0 ~ 1)
        float zoomRange = maxZoom - minZoom;
        float zoomNormalized = (currentZoom - minZoom) / zoomRange;
        
        // AnimationCurve에서 회전값 가져오기
        float curveValue = zoomRotationCurve.Evaluate(zoomNormalized);
        
        // curveValue (0~1)를 minRotationX ~ maxRotationX로 매핑
        currentRotationX = Mathf.Lerp(minRotationX, maxRotationX, curveValue);
    }
    
    // ========================================================================
    // 회전 입력 (R/F 키) - Input System
    // ========================================================================
    private void HandleRotationInput()
    {
        // R/F 키로 회전 각도 조절 (Input System)
        // Note: 현재는 줌에 따라 각도가 자동 설정되므로 수동 조정은 비활성화
        // 필요시 아래 주석을 해제하여 사용
        /*
        if (Keyboard.current.rKey.isPressed)
        {
            currentRotationX -= rotationSpeed * Time.deltaTime * 30f;
            currentRotationX = Mathf.Clamp(currentRotationX, minRotationX, maxRotationX);
        }
        if (Keyboard.current.fKey.isPressed)
        {
            currentRotationX += rotationSpeed * Time.deltaTime * 30f;
            currentRotationX = Mathf.Clamp(currentRotationX, minRotationX, maxRotationX);
        }
        */
    }
    
    // ========================================================================
    // 회전 적용 - 캐릭터를 바라보도록 Y축 180도
    // ========================================================================
    private void ApplyRotation()
    {
        // X축 회전 (비스듬한 각도) + Y축 180도 (캐릭터 방향)
        transform.rotation = Quaternion.Euler(currentRotationX, 180f, currentRotationZ);
    }
    
    // ========================================================================
    // 카메라 위치 업데이트 - 원호 형태로 휘어지며 이동
    // ========================================================================
    private void UpdateCameraPosition()
    {
        if (target == null) return;
        
        // 목표 위치 계산
        Vector3 targetPosition = target.position;
        
        // 원호 형태로 카메라 위치 계산
        // 각도(θ)에 따라 수평/수직 거리 계산
        float angleRad = currentRotationX * Mathf.Deg2Rad;
        float horizontalDistance = currentZoom * Mathf.Cos(angleRad); // Z 방향 거리
        float verticalDistance = currentZoom * Mathf.Sin(angleRad);   // Y 방향 거리
        
        // X축 따라가기
        float targetX = followX ? targetPosition.x + baseOffset.x : baseOffset.x;
        
        // 캐릭터 기준 원호 위치 계산
        // 캐릭터가 -Z에 있고, 카메라가 +Z 방향(정면)에 위치
        float cameraY = targetPosition.y + verticalDistance;
        float cameraZ = targetPosition.z + horizontalDistance;
        
        // 부드러운 이동
        Vector3 desiredPosition = new Vector3(targetX, cameraY, cameraZ);
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );
        
        // 회전 적용
        ApplyRotation();
    }
    
    // ========================================================================
    // 외부에서 줌 값 설정 (UI 버튼 등)
    // ========================================================================
    public void SetZoom(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
    
    // ========================================================================
    // 외부에서 회전값 설정
    // ========================================================================
    public void SetRotation(float rotationX)
    {
        currentRotationX = Mathf.Clamp(rotationX, minRotationX, maxRotationX);
    }
    
    // ========================================================================
    // 줌 값 반환 (UI에 표시용)
    // ========================================================================
    public float GetCurrentZoom()
    {
        return currentZoom;
    }
    
    // ========================================================================
    // 회전값 반환 (UI에 표시용)
    // ========================================================================
    public float GetCurrentRotation()
    {
        return currentRotationX;
    }
}
