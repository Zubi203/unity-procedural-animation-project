using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{
    public CharacterController Controller { get; private set; }
    public CharacterMotor Motor { get; private set; }
    public CharacterStateMachine StateMachine { get; private set; }
    public CharacterInteractor Interactor { get; private set; }
    public CameraController CameraController { get; private set; }

    [Header("Config")]
    public CharacterStats stats;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    public bool SprintHeld() => Input.GetKey(sprintKey);
    public bool CrouchHeld() => Input.GetKey(crouchKey);
    
    [Header("Ground Settings")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckDistance = 0.25f;

    public bool IsGrounded { get; private set; }
    public bool IsStableGround { get; private set; }
    public RaycastHit GroundHit { get; private set; }
    
    bool isCrouching = false;

    // public bool IsGrounded => TryGetGround(out _);
    
    private void Awake()
    {
        StateMachine = new CharacterStateMachine();
        Interactor = GetComponent<CharacterInteractor>();
        CameraController = GetComponentInChildren<CameraController>();
        Controller = GetComponent<CharacterController>();
        Motor = new CharacterMotor(
            Controller,
            stats
        );
    }

    private void Start()
    {
        StateMachine.SetState(new GroundedState(this));
    }

    private void Update()
    {
        UpdateGround();
        StateMachine.Tick();
        Motor.TickGravity(IsGrounded);
        
    }
    
    void LateUpdate()
    {
        CheckCrouch();
    }

    void CheckCrouch()
    {
        float desired = stats.standHeight;

        if (CrouchHeld())
        {
            desired = stats.crouchHeight;
            isCrouching = true;
        }
        if (!CrouchHeld() && CanStandUp() && isCrouching)
        {
            desired = stats.standHeight;
            isCrouching = false;
        }
        else if (!CrouchHeld() && !CanStandUp() && isCrouching)
        {
            desired = stats.crouchHeight;
        }
        if (Mathf.Abs(Controller.height - desired) > 0.01f)
        {
            Controller.height = Mathf.Lerp(
                Controller.height,
                desired,
                Time.deltaTime * stats.crouchTransitionSpeed
            );
        }
        
        
        CameraController.view.transform.localScale = new Vector3(1, 1 * desired/1.8f, 1); 

        Controller.center = Vector3.up * (Controller.height * 0.5f);
        
        CameraController.cameraPivot.transform.localPosition = new Vector3(
            CameraController.cameraPivot.localPosition.x,
            desired - 0.5f,
            CameraController.cameraPivot.localPosition.z
        );
    }

    public bool CanStandUp()
    {
        float checkDistance = stats.standHeight;

        return !Physics.SphereCast(
            transform.position,
            Controller.radius,
            Vector3.up,
            out _,
            checkDistance,
            groundMask
        );
    }

    public bool TryGetGround(out RaycastHit hit)
    {
        float radius = Controller.radius * 0.95f;
        float castDistance = 0.15f;

        Vector3 origin = transform.position + Vector3.up * radius;

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out hit,
            castDistance,
            groundMask
        );
    }
    
    void UpdateGround()
    {
        IsGrounded = false;
        IsStableGround = false;

        if (!Controller.isGrounded)
            return;

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            GroundHit = hit;
            IsGrounded = true;

            if (angle <= stats.maxSlopeAngle)
                IsStableGround = true;
        }
    }
    
    public bool TryGetMantleZone(out MantleZone zone)
    {
        zone = null;

        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 0.6f,
            0.6f
        );

        foreach (var col in hits)
        {
            if (col.TryGetComponent(out MantleZone z) && z.CanMantle(transform))
            {
                zone = z;
                return true;
            }
        }

        return false;
    }
    
    
    public bool IsOnSteepSlope(out RaycastHit hit)
    {
        if (!TryGetGround(out hit))
            return false;

        float angle = Vector3.Angle(hit.normal, Vector3.up);
        return angle > stats.maxSlopeAngle;
    }
}
