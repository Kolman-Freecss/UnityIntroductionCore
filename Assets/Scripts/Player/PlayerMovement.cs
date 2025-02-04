
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputController))]
public class PlayerMovement: MonoBehaviour
{
    #region Data
    [Header("Player")]
    [SerializeField] float speed;
    [SerializeField] [Range(0.0f, 10f)]  float sprint = 10f;
    [SerializeField] [Range(0.0f, 0.3f)] float rotationSmooth= 0.05f; // TO-DO rotacion en mov

    [Space(10)] 
    [Header("Jump")]
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;
    [SerializeField] float gravity = -9.81f; //stiamted, unity use his own gravity system
    [SerializeField] float jumpHeight;
    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Space(10)] [Header("Player is grounded")] [Tooltip ("Scans the assignated layer in order to create the jump action")] [SerializeField] LayerMask GroundLayer;
    [SerializeField] public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")][SerializeField] public float GroundedRadius = 0.28f;
    #endregion

    #region Auxiliary data
    
    //AUXILIARY DATA 
    [SerializeField]PlayerInputController playerInputController;
    [SerializeField] GameObject _mainCamera;
    CharacterController _controller;
    //Movement aux data
    private float targetRotation;
    //Jump aux data
    [SerializeField] bool isGrounded;
    private Vector3 playerVelocity;
    private float verticalVelocity;
    // timeout jump
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;
    private float _terminalVelocity = 53.0f;
    
    #endregion
    
    #region Initialize script
    private void Awake()
    {
        GetReferences();
    }

    void GetReferences()
    {
        // Intenta encontrar un objeto con el script PlayerInputController adjunto.
        playerInputController = FindObjectOfType<PlayerInputController>();
        _controller = GetComponent<CharacterController>();

        // Aseg�rate de que playerInputController no sea nulo despu�s de la b�squeda.
        if (playerInputController == null)
        {
            Debug.LogError("PlayerInputController not found in the scene. Make sure it is attached to an object.");
        }
    }
    #endregion


    #region Script's logic
    void Update()
    {
        GroundCheck();
        Move();
        Jump();
    }

    void Move()
    {
        // Verifica si playerInputController es nulo antes de usarlo.
        if (playerInputController != null)
        {
            float targetSpeed = speed;
            if (playerInputController.moveDirection == Vector2.zero) targetSpeed = 0.0f;
            
            Vector3 move = new Vector3(playerInputController.moveDirection.x, 0, playerInputController.moveDirection.y).normalized;
            
            Vector3 currentHorizontalSpeed =
                new Vector3(playerInputController.moveDirection.x, 0.0f, playerInputController.moveDirection.y);

            if (move != Vector3.zero)
            {
                targetRotation = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
                                 //_mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref playerVelocity.y, rotationSmooth);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            Vector3 newPosition = transform.position + (new Vector3(move.x, verticalVelocity, move.z) * Time.deltaTime);
            
            currentHorizontalSpeed = _mainCamera.transform.forward * currentHorizontalSpeed.z +
                                     _mainCamera.transform.right * currentHorizontalSpeed.x;
            currentHorizontalSpeed.y = 0.0f;
            float currentHorizontalSpeedMagnitude = currentHorizontalSpeed.magnitude;
            
            
            
            _controller.Move(targetDirection.normalized *
                             (currentHorizontalSpeedMagnitude * Time.deltaTime * targetSpeed) +
                             new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else
        {
            Debug.LogError("PlayerInputController is null. Make sure it is properly initialized.");
        }
    }
    void GroundCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,  transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayer,QueryTriggerInteraction.Ignore);
        // if (_hasAnimator)
        // {
        //     _animator.SetBool(_animIDOnGround, Grounded);
        // }
    }
    void Jump()
    {
 

        // stop our velocity dropping when grounded
        if (isGrounded)
        {
            // reset the fall timeout timer
            fallTimeoutDelta = FallTimeout;

            playerVelocity.y = 0.0f;
            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }
            // Jump pressed and player on ground
            if (playerInputController.jump && jumpTimeoutDelta <= 0.0f)
            {
                Debug.Log("Salto");
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            }
            // jump timeout
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }

        else
        {
            // reset the jump timeout timer
            jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            // if we are not grounded, do not jump
            playerInputController.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < _terminalVelocity)
        {
            // if (_hasAnimator)
            // {
            //     _animator.SetFloat(_animIDJumpVelocity, _verticalVelocity);
            // }
            verticalVelocity += gravity * Time.deltaTime;
        }
        // playerVelocity.y += gravity * Time.deltaTime;



    }

    #endregion

}
