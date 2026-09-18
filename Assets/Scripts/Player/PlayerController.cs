// ============================================================================
// PlayerController.cs
// 3D 물리 기반 2.5D 플레이어 컨트롤러 - WASD 이동 + 점프 + SpriteAnimator 연동
// Input System (Unity 6) + Rigidbody (3D) 호환
// ============================================================================

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ==================== 이동 설정 ====================
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.15f;
    
    // ==================== 공격 설정 ====================
    [Header("Attack")]
    [SerializeField] private string attack1Anim = "attack1";
    [SerializeField] private string attack2Anim = "attack2";
    [SerializeField] private float attack1Duration = 0.3f;
    [SerializeField] private float attack2Duration = 0.6f;
    [SerializeField] private float attackCooldown = 0.3f;
    
    // ==================== 레이어 ====================
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer = 1; // Default layer
    
    // ==================== 컴포넌트 ====================
    private Rigidbody rb;
    private SpriteAnimator spriteAnimator;
    private SpriteRenderer spriteRenderer;
    
    // ==================== 상태 ====================
    private Vector2 moveDirection;
    private bool isGrounded;
    private bool isAttacking;
    private float nextAttackTime;
    private string idleAnim = "ready";
    private string walkAnim = "walk";
    private float lastMoveX = -1f;  // 마지막 이동 방향 기억 (기본값 -1: 왼쪽)
    
    // ==================== Input Actions ====================
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attack1Action;
    private InputAction attack2Action;

    // ========================================================================
    // 초기화
    // ========================================================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteAnimator = GetComponent<SpriteAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (rb != null)
        {
            // RigidbodyConstraints.FreezeRotationX | FreezeRotationY | FreezeRotationZ = 15
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        SetupInputActions();
    }
    
    // ========================================================================
    // Input System 설정
    // ========================================================================
    private void SetupInputActions()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }
        
        var map = new InputActionMap("Player");
        
        // 이동 (WASD + 화살표)
        moveAction = map.AddAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        
        // 점프 (Space)
        jumpAction = map.AddAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        
        // 공격1 (좌클릭)
        attack1Action = map.AddAction("Attack1", InputActionType.Button);
        attack1Action.AddBinding("<Mouse>/leftButton");
        
        // 공격2 (우클릭)
        attack2Action = map.AddAction("Attack2", InputActionType.Button);
        attack2Action.AddBinding("<Mouse>/rightButton");
        
        // Input callbacks 등록
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        jumpAction.performed += OnJump;
        attack1Action.performed += OnAttack1;
        attack2Action.performed += OnAttack2;
        
        map.Enable();
    }
    
    // ========================================================================
    // Input Callbacks
    // ========================================================================
    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        moveDirection = value;
        
        // 마지막 이동 방향 기억 (좌우 이동 시)
        if (Mathf.Abs(value.x) > 0.1f)
        {
            lastMoveX = value.x;
        }
        
        // 스프라이트 방향 (카메라가 Y=180도로 캐릭터를 바라봄)
        // D키(value.x=+1) → world -X 이동 → 스프라이트 플립
        // A키(value.x=-1) → world +X 이동 → 스프라이트 플립 없음
        if (spriteAnimator != null)
        {
            bool flip = (lastMoveX > 0.1f);
            spriteAnimator.SetFlipX(flip);
        }
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isAttacking)
        {
            Jump();
        }
    }
    
    private void OnAttack1(InputAction.CallbackContext context)
    {
        if (!isAttacking && Time.time >= nextAttackTime)
        {
            StartAttack(1);
        }
    }
    
    private void OnAttack2(InputAction.CallbackContext context)
    {
        if (!isAttacking && Time.time >= nextAttackTime)
        {
            StartAttack(2);
        }
    }

    // ========================================================================
    // 업데이트
    // ========================================================================
    private void Update()
    {
        CheckGrounded();
        UpdateAnimation();
    }

    // ========================================================================
    // 물리
    // ========================================================================
    private void FixedUpdate()
    {
        ApplyMovement();
    }

    // ========================================================================
    // 땅 체크 (3D Raycast)
    // ========================================================================
    private void CheckGrounded()
    {
        // Raycast로 땅 체크 (아래 방향)
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.1f, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    // ========================================================================
    // 점프 (3D)
    // ========================================================================
    private void Jump()
    {
        if (rb != null)
        {
            // Y축 속도만 설정 (기존 Y 속도 유지)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    // ========================================================================
    // 시작 시 초기화
    // ========================================================================
    private void Start()
    {
        // Rigidbody Interpolation 설정 (고스트 이미지 방지)
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // 카메라가 캐릭터를 바라보므로 캐릭터는 기본 방향 유지
        transform.rotation = Quaternion.identity;
    }
    
    // ========================================================================
    // 공격 시작
    // ========================================================================
    private void StartAttack(int attackType)
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        
        if (attackType == 1)
        {
            spriteAnimator.Play(attack1Anim, false);
            Invoke(nameof(EndAttack), attack1Duration);
        }
        else
        {
            spriteAnimator.Play(attack2Anim, false);
            Invoke(nameof(EndAttack), attack2Duration);
        }
    }

    // ========================================================================
    // 공격 종료
    // ========================================================================
    private void EndAttack()
    {
        isAttacking = false;
    }

    // ========================================================================
    // 이동 적용 (3D)
    // ========================================================================
    private void ApplyMovement()
    {
        if (isAttacking)
        {
            // 공격 중 마찰 (X,Z 속도 감소)
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x * 0.9f,
                    rb.linearVelocity.y,
                    rb.linearVelocity.z * 0.9f
                );
            }
            return;
        }
        
        // 8방향 이동 (X,Z 평면에서만)
        // 카메라가 Y축 180도 회전하여 forward 벡터가 -Z이므로 moveDirection.y도 반전
        Vector3 newVelocity = rb.linearVelocity;
        newVelocity.x = -moveDirection.x * moveSpeed; // X축 반전 (카메라 회전 반영)
        newVelocity.z = -moveDirection.y * moveSpeed;  // Z축 반전 (카메라 회전 반영)
        rb.linearVelocity = newVelocity;
    }

    // ========================================================================
    // 애니메이션 업데이트
    // ========================================================================
    private void UpdateAnimation()
    {
        if (isAttacking) return;
        
        // X축 또는 Z축으로 이동 중인지 체크
        bool isMoving = Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.y) > 0.1f;
        
        if (isMoving)
        {
            spriteAnimator.Play(walkAnim);
        }
        else
        {
            spriteAnimator.Play(idleAnim);
        }
    }

    // ========================================================================
    // 정리
    // ========================================================================
    private void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
        }
        if (attack1Action != null)
        {
            attack1Action.performed -= OnAttack1;
        }
        if (attack2Action != null)
        {
            attack2Action.performed -= OnAttack2;
        }
    }

    // ========================================================================
    // 기즈모
    // ========================================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(origin, Vector3.down * (groundCheckDistance + 0.1f));
    }
}
