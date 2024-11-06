using System.Collections;
using SmoothShakeFree;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;
using UnityEditor;


[RequireComponent(typeof(Rigidbody))]
public class PlayerInputController : MonoBehaviour
{
    private DefaultPlayerActions _defaultPlayerActions;

    private InputAction _moveAction, _lookAction, _jumpAction
        , _runAction, _crouchAction, _dragAction, _leftClickAction, _rightClickAction, 
        _scaleScrollUpAction, _scaleScrollDownAction, _scaleScrollButtonAction;

    //private InputAction _shootAction;

    [HideInInspector] public Rigidbody _rb;
    [Header("FOV Settings")]
    public CinemachineVirtualCamera mainCamera; // Assign this to the main camera in the inspector
    public float normalFOV = 60f; // Default FOV
    public float runningFOV = 70f;
    public float crouchFOV = 60f;// FOV when running
    public float fovTransitionSpeed = 2f; // Speed of FOV transition


    private float targetFOV;

    float speed;
    [Header("Movement Settings")]
    public float _moveSpeed = 5f;
    public float _runSpeedMultiplier = 5f; // Multiplier for run speed
    
    public float maxVelocity = 10f;
    Vector3 maxVelVector;
    public float groundCheckDistance = 0.1f; // Distance from the collider to check for the ground
    //public LayerMask groundLayer; // Layer to detect as ground

    public float _jumpForce = 9f; // Jump force
    public float _jumpForceOnSlope = 6f;
    public float _jumpForceOnCrouch = 4f;
    public float jumpRaycastLenght = 1.02f;
    private CapsuleCollider capsuleCollider;
    private float colliderRadius;
    private Vector3 spherePosition;
    

    

    [Header("Crouch Settings")]
    public float _crouchSpeedMultiplier = 0.5f;
    public float crouchYscale;
    public LayerMask whatIsOnTop;
    private float startYscale;

    [Header("Slope Settings")]
    public float maxSlopeAngle = 0.5f;
    private RaycastHit slopeHit;

    bool isWalking, isRunning, isCrouching;
    [Header("Camera Settings")]
    [Range(0f, 1f)]
    public float _lookTreshold = 0.25f;
    public float _lookSensitivityVertical = 5f;
    public float _lookSensitivityHorizontal = 5f;

    //private float verticalRotation = 0;
    public float rotationDeadAngle = 75f;


    //[HideInInspector] public CapsuleCollider playerCollider = null;
    



    [Header("Cam Wobble Settings")]
    public bool camWobbleOn = true;
    [ConditionalField("camWobbleOn")]
    public float camWobbleNormal, camWobbleWalking, camWobbleRunning, camWobbleCrouching;
    [ConditionalField("camWobbleOn")]
    public float camWobbleChangeSpeed = 1f;


    public Vector3 smoothShakeFrequency;
    private SmoothShake smoothShake;




    [Header("Air Speed Settings")]

    
    public float airSpeedMultiplier = 1f;
    public float maxAirSpeedMultiplier = 10f; // Maximum speed multiplier while in the air
    public float airTimeIncrement = 0.1f; // Rate at which the multiplier increases per second in the air
    public float speedDecreaseRate = 1f; // Rate at which the multiplier decreases after landing

    private bool wasGrounded = true;

    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Item Rotating Settings")]
    public float rotateSpeed = 1f;
    private Vector2 rotation;

    //private TextManager textManager;

    RaycastHit hit;
    [HideInInspector] public Vector2 cumulativeMouseDelta;
    //private NewDragAndDrop dragAndDrop;
    
    float _jumpForceAtStart;
    //Interactor interactor;

    //[Header("Pause Menu Script")]
    //public PauseMenuScript pauseMenuScript;

    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public string ConditionFieldName;

        public ConditionalFieldAttribute(string conditionFieldName)
        {
            ConditionFieldName = conditionFieldName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    public class ConditionalFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalFieldAttribute conditional = (ConditionalFieldAttribute)attribute;
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditional.ConditionFieldName);

            if (conditionProperty != null && conditionProperty.boolValue)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalFieldAttribute conditional = (ConditionalFieldAttribute)attribute;
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditional.ConditionFieldName);

            if (conditionProperty != null && conditionProperty.boolValue)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
#endif

    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        
        targetFOV = normalFOV;
        mainCamera.m_Lens.FieldOfView = normalFOV;

        _defaultPlayerActions = new DefaultPlayerActions();
        _rb = GetComponent<Rigidbody>();
        smoothShake = GetComponentInChildren<SmoothShake>();
        
        
        _jumpForceAtStart = _jumpForce;

        //_lookSensitivityVertical= PlayerPrefs.GetFloat("verticalSens");
        //_lookSensitivityHorizontal = PlayerPrefs.GetFloat("horizontalSens");

       


    }

    private void Start()
    {
        
        Cursor.visible = false;
        // Get the CapsuleCollider component
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();

        // Calculate the collider's radius
        colliderRadius = capsuleCollider.radius * 0.8f; // Slightly reduce to avoid edge cases

        
        smoothShakeFrequency = smoothShake.positionShake.frequency;
        //playerCollider = gameObject.GetComponentInChildren<CapsuleCollider>();
        
        startYscale = transform.localScale.y;
        xRotation=mainCamera.transform.localEulerAngles.x;
        //interactor= GameObject.FindAnyObjectByType<Interactor>();
    }


    private void OnEnable()
    {
        _moveAction = _defaultPlayerActions.Player.Move;
        _moveAction.Enable();
        _lookAction = _defaultPlayerActions.Player.Look;
        _lookAction.Enable();
        _jumpAction = _defaultPlayerActions.Player.Jump;
        _jumpAction.Enable();
        _runAction = _defaultPlayerActions.Player.Run;
        _runAction.Enable();
        _crouchAction = _defaultPlayerActions.Player.Crouch;
        _crouchAction.Enable();
        _dragAction = _defaultPlayerActions.Player.Drag;
        _dragAction.Enable();
        _leftClickAction = _defaultPlayerActions.Player.LeftClick;
        _leftClickAction.Enable();
        _rightClickAction = _defaultPlayerActions.Player.RightClick;
        _rightClickAction.Enable();
        _scaleScrollButtonAction = _defaultPlayerActions.Player.AxisChange;
        _scaleScrollButtonAction.Enable();
        _scaleScrollUpAction = _defaultPlayerActions.Player.AxisUp;
        _scaleScrollUpAction.Enable();
        _scaleScrollDownAction = _defaultPlayerActions.Player.AxisDown;
        _scaleScrollDownAction.Enable();


        //_shootAction = _defaultPlayerActions.Player.Fire;
        //_shootAction.Enable();

        _lookAction.performed += OnLooks;
        _moveAction.performed += OnWalk;
        _moveAction.canceled += OnWalkCancel;
        _jumpAction.performed += OnJump; // Subscribe to jump action
        _jumpAction.canceled += OnJumpCancel;
        _runAction.performed += OnRun;
        _runAction.canceled += OnRunCancel;
        _crouchAction.performed += OnCrouch;
        _crouchAction.canceled += OnCrouchCancel;
        _dragAction.performed += OnObjectDrag;
        _dragAction.canceled += OnObjectDragCancel;
        _leftClickAction.performed += OnLeftClick;
        _leftClickAction.canceled += OnLeftClickCancel;
        _rightClickAction.performed += OnRightClick;
        _rightClickAction.canceled += OnRightClickCancel;

       

        
        InputSystem.onDeviceChange += OnDeviceChange; // Subscribe to device change events


    }
    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        _runAction.Disable();
        _crouchAction.Disable();
        _dragAction.Disable();
        _leftClickAction.Disable();
        _rightClickAction.Disable();
        _scaleScrollButtonAction.Disable();
        _scaleScrollUpAction.Disable();
        _scaleScrollDownAction.Disable();

        _lookAction.performed -= OnLooks;
        _moveAction.performed -= OnWalk;
        _moveAction.canceled -= OnWalkCancel;
        _jumpAction.performed -= OnJump;
        _jumpAction.canceled -= OnJumpCancel;
        _runAction.performed -= OnRun;
        _runAction.canceled -= OnRunCancel;

        _crouchAction.performed -= OnCrouch;
        _crouchAction.canceled -= OnCrouchCancel;

        _dragAction.performed -= OnObjectDrag;
        _dragAction.canceled -= OnObjectDragCancel;

        _leftClickAction.performed -= OnLeftClick;
        _leftClickAction.canceled -= OnLeftClickCancel;
        _rightClickAction.performed -= OnRightClick;
        _rightClickAction.canceled -= OnRightClickCancel;

        



        InputSystem.onDeviceChange -= OnDeviceChange; // Unsubscribe from device change events
    }

    private bool isGamepadInput = false;
    private bool isKeyboardInput = false;

    private void OnInputDetected(InputAction.CallbackContext context)
    {
        var device = context.control.device;

        if (device is Gamepad && !isGamepadInput)
        {
            isGamepadInput = true;
            isKeyboardInput = false;
            UpdateUIForGamepad();
        }
        else if ((device is Keyboard||device is Mouse) && !isKeyboardInput)
        {
            isKeyboardInput = true;
            isGamepadInput = false;
            UpdateUIForKeyboard();
        }
    }

    private void UpdateUIForGamepad()
    {
        Debug.Log("Gamepad input detected, updating UI for gamepad.");

        // Example: Set the first UI button to be selected in the Event System
       // EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(GameObject.Find("YourGamepadUIElement"));
    }

    private void UpdateUIForKeyboard()
    {
        Debug.Log("Keyboard input detected, updating UI for keyboard.");

        // Example: Set the first UI button to be selected in the Event System
        //EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(GameObject.Find("YourKeyboardUIElement"));
    }

   
   

    private void OnLooks(InputAction.CallbackContext context)
    {
        Vector2 lookDir = _lookAction.ReadValue<Vector2>();

        if (lookDir.magnitude > _lookTreshold) OnInputDetected(context);
    }
    private void OnPauseButton(InputAction.CallbackContext context)
    {
       
    }

    


    

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {

        switch (change)
        {
            case InputDeviceChange.Added:
                Debug.Log($"Device added: {device.displayName}");
                break;
            case InputDeviceChange.Removed:
                Debug.Log($"Device removed: {device.displayName}");
                break;
            case InputDeviceChange.Disconnected:
                Debug.Log($"Device disconnected: {device.displayName}");
                //InputSystem.FlushDisconnectedDevices();
                break;
            case InputDeviceChange.Reconnected:
                Debug.Log($"Device reconnected: {device.displayName}");
                break;
        }
    }
    Coroutine airCoroutine, reduceCoroutine;

    bool  canJump = true;


    private void OnJump(InputAction.CallbackContext context)
    {
        OnInputDetected(context);
        
        
        if (coyoteTimeCounter > 0 && wasGrounded && canJump)
        {
            if (_rb.useGravity == false) { _rb.useGravity = true; }

            coyoteTimeCounter = 0f;
            wasGrounded = false;
            Jump();
        }
        
    }

    

    private void Jump()
    {
        if (Gamepad.current != null) 
        {
            StartCoroutine(VibrateController(.5f, 0, .2f));
        }
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

        
        if (airCoroutine == null)

        {
            airCoroutine = StartCoroutine(InAir());
        }
        
    }
    

    
    private void OnJumpCancel(InputAction.CallbackContext context)
    {
        
        



    }
    

    [HideInInspector] public bool inAir = false;
    IEnumerator InAir()
    {
        if (reduceCoroutine != null) 
        {
            StopCoroutine(reduceCoroutine);
            reduceCoroutine=null;
        }

        inAir = true;

        
        Debug.Log("zeminde degil");
        
        while (inAir)
        {
            //if (!wallrunScript.isWallrunning) 
            //{
            //    airTime += Time.deltaTime;
            //    airSpeedMultiplier = Mathf.Min(airSpeedMultiplier + airTime * airTimeIncrement, maxAirSpeedMultiplier);
            //}
            Debug.Log("in air");

            yield return new WaitForSeconds(0.1f);

            //if (!wallrunScript.isWallrunning) { 
            //    Debug.Log("Checkin for wall");
            //    wallrunScript.CheckForWall();
            //    if (wallrunScript.isWallRight || wallrunScript.isWallLeft)
            //    {
                    

            //        if (reduceCoroutine == null && airSpeedMultiplier >= 0) { StartCoroutine(ReduceSpeedMultiplier()); }
            //        if (!wallrunScript.isWallrunning) 
            //        {
                        
            //            wallrunScript.StartWallrun();
                        
            //        } 

                    

            //    }

            //}
            wasGrounded = false;

            if (IsGrounded()) {
                
                wasGrounded = true;
                inAir = false; 
            }
            
        }

        //if (!wallrunScript.isWallrunning&&wallrunScript.tiltCoroutine == null)
        //{
        //    Debug.Log("Landed or started wallride");
        //    wallrunScript.tiltCoroutine = StartCoroutine(wallrunScript.TiltCamera(0f));
        //}
        
        
        // After landing, slowly decrease the speed multiplier back to 1
        reduceCoroutine = StartCoroutine(ReduceSpeedMultiplier());
        
        
        airCoroutine = null;
        wasGrounded = true;
        inAir = false;
        

        yield return null;
    }

    IEnumerator ReduceSpeedMultiplier()
    {
        while (airSpeedMultiplier > 0)
        {
            
            airSpeedMultiplier -= speedDecreaseRate * Time.deltaTime;
            
            yield return null;

        }
        reduceCoroutine = null;
    }

    public bool IsGrounded()
    {
        //if (dragAndDrop.isDragging && Physics.SphereCast(transform.position, colliderRadius, Vector3.down, out RaycastHit hitWhileDragging, (transform.localScale.y) - colliderRadius + groundCheckDistance))
        //{
        //    //if (hitWhileDragging.transform.CompareTag("Draggable"))
        //    //{
        //    //    airTime = 0f;
        //    //    return false;
        //    //}
        //}
        
            // Perform the SphereCast to check if the player is grounded
            return Physics.SphereCast(transform.position, colliderRadius, Vector3.down, out RaycastHit hit, (transform.localScale.y) - colliderRadius + groundCheckDistance);
        
    }



    Vector3 vel;
    private void HandleMovement()
    {
        Vector2 moveDir = _moveAction.ReadValue<Vector2>();


        if (moveDir.magnitude > _lookTreshold)
        {
            if(_rb.isKinematic) _rb.isKinematic = false;
            isWalking = true;

            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 desiredMoveDirection = (forward * moveDir.y + right * moveDir.x).normalized;
            if (inAir) speed = _moveSpeed * airSpeedMultiplier;
            speed = _moveSpeed ;



            if (isCrouching)
            {
                Debug.Log("crouching ");
                speed *= _crouchSpeedMultiplier;

                targetFOV = crouchFOV;
                isRunning = false;
            }

            if (isRunning)
            {
                speed *= _runSpeedMultiplier;
                targetFOV = runningFOV;

            }

            vel = new Vector3(desiredMoveDirection.x * speed, _rb.velocity.y, desiredMoveDirection.z * speed);

            if (/*!wallrunScript.isWallrunning &&*/ airCoroutine != null /*&& (_rb.velocity.magnitude >= vel.magnitude)*/)
            {
                Debug.Log("move2");
                _rb.AddForce(new Vector3(vel.x* 0.6f, 0, vel.z*0.6f), ForceMode.Force);


            }
            else if (/*!wallrunScript.isWallrunning &&*/ airCoroutine == null)
            {

                if (OnSlope())
                {
                    _rb.velocity = GetSlopeMoveDirection() * vel.magnitude*.8f;
                    Debug.Log("move4");
                    Debug.Log("Onslope");
                }
                else
                {
                    Debug.Log("move3");
                    _rb.velocity = vel;
                }



            }

        }
        else
        {

            //if (waitCoroutine == null) StartCoroutine(WaitFor(2f, w8));


            if (/*!wallrunScript.isWallrunning &&*/ airCoroutine == null && !onSlope/*&& w8*/ /*&& (_rb.velocity.magnitude >= vel.magnitude)*/)
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            }else if(airCoroutine == null && onSlope&&!_rb.isKinematic) 
            {
                _rb.velocity = new Vector3(0, 0, 0);
                //_rb.isKinematic = true;
            }

        }
    }

    float xRotation;

    
   
    private void HandleLook()
    {
        if (rotateCoroutine != null) { return; }
        Vector2 lookDir = _lookAction.ReadValue<Vector2>();

        if (lookDir.magnitude > _lookTreshold)
        {

            float mouseX = lookDir.x * _lookSensitivityHorizontal * 0.1f * Time.deltaTime;
            float mouseY = lookDir.y *_lookSensitivityVertical*0.1f* Time.deltaTime;

            

            // Apply horizontal rotation to the player
            transform.Rotate(Vector3.up * mouseX);

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -rotationDeadAngle, rotationDeadAngle);

            mainCamera.transform.localRotation = Quaternion.Euler(mainCamera.transform.localRotation.x + xRotation, 0, mainCamera.transform.localRotation.z);



        }
        else if (lookDir.magnitude <= _lookTreshold)
        {
            //mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation , currentCameraTilt);

            //mainCamera.transform.localRotation = Quaternion.Euler(mainCamera.transform.localRotation.eulerAngles.x + recoilX, mainCamera.transform.localRotation.eulerAngles.y+ recoilY, mainCamera.transform.localRotation.eulerAngles.z);
            //mainCamera.transform.localRotation = recoil;
        }

    }
    


    private void OnWalk(InputAction.CallbackContext context)
    {
        Vector2 moveDir = _moveAction.ReadValue<Vector2>();


        if (moveDir.magnitude > _lookTreshold) OnInputDetected(context);

        if (!isWalking)
        {
            isWalking = true;
            if(camWobbleOn)smoothShakeFrequency *= camWobbleWalking;
            //smoothShake.positionShake.frequency = smoothShake.positionShake.frequency * camWobbleWalking;

        }


    }

    private void OnWalkCancel(InputAction.CallbackContext context)
    {
        
        isWalking = false;
        if (camWobbleOn) smoothShakeFrequency /= camWobbleWalking;
        //smoothShake.positionShake.frequency = smoothShake.positionShake.frequency / camWobbleWalking;




    }

    private void OnRun(InputAction.CallbackContext context)
    {
        OnInputDetected(context);
        isRunning = true;
        if (camWobbleOn) smoothShakeFrequency *= camWobbleRunning;
        //smoothShake.positionShake.frequency = smoothShake.positionShake.frequency * camWobbleRunning;

    }

    private void OnRunCancel(InputAction.CallbackContext context)
    {
        isRunning = false;
        targetFOV = normalFOV;
        if (camWobbleOn) smoothShakeFrequency /= camWobbleRunning;
        //smoothShake.positionShake.frequency = smoothShake.positionShake.frequency / camWobbleRunning;



    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        OnInputDetected(context);
        isCrouching = true;
        isRunning = false;
        _jumpForce = _jumpForceOnCrouch;
        targetFOV = crouchFOV;
        if (camWobbleOn) smoothShakeFrequency *= camWobbleCrouching;
        

        startYscale = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*crouchYscale, transform.localScale.z);
        _rb.AddForce(Vector3.down * 5, ForceMode.Impulse);


    }
    Coroutine checkUpCoroutine;
    private void OnCrouchCancel(InputAction.CallbackContext context)
    {
        
        if (checkUpCoroutine == null)
        {
            checkUpCoroutine = StartCoroutine(CheckUp());
        }
        else
        {
            StopCoroutine(checkUpCoroutine);
            checkUpCoroutine = null;
            checkUpCoroutine = StartCoroutine(CheckUp());
        }



    }
    IEnumerator CheckUp()
    {
        while (Physics.Raycast(transform.position- new Vector3(0, transform.localScale.y, 0) , transform.up, out RaycastHit hit, transform.localScale.y + (startYscale * 2f)+0.1f, whatIsOnTop))
        {
            Debug.Log("checkingup");
            canJump = false;
            yield return new WaitForSeconds(.5f);
            yield return null;
        }
        _jumpForce = _jumpForceAtStart;
        canJump = true;
        isCrouching = false;
        if (camWobbleOn) smoothShakeFrequency /= camWobbleCrouching;


        
        targetFOV = normalFOV;
        transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
        startYscale = 0;
        yield return null;


    }

    private bool OnSlope()
    {
        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
        
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, transform.localScale.y + ((transform.localScale.y) * Mathf.Sin(angle) * Mathf.Sin(angle))+groundCheckDistance))
        {
            
            
            //Debug.Log(angle);
            return angle <= maxSlopeAngle && angle >= 2;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(vel, slopeHit.normal).normalized;
    }


    public IEnumerator VibrateController(float intensityL, float intensityR, float time)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(intensityL, intensityR);
            yield return new WaitForSeconds(time); // Adjust duration as needed
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
        yield return null;
    }

    private void OnObjectDrag(InputAction.CallbackContext context)
    {
        OnInputDetected(context);
        //if (dragAndDrop.isDragging)
        //{
        //    dragAndDrop.StopDrag();

        //} else if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hitt, dragAndDrop.dragDistance, dragAndDrop.draggableLayer)
        //    && !dragAndDrop.isDragging)
        //{
            
        //   if(dragAndDrop.TryStartDrag(hitt))StartCoroutine(VibrateController(.5f, 0, .2f));
        //} 
    }
    private void OnObjectDragCancel(InputAction.CallbackContext context)
    {
        //isDragging = false;

    }


    [HideInInspector] public bool isLeftClick,isRightClick;
    Coroutine weightCoroutine,scaleCoroutine, dimensionCoroutine;


    private void OnLeftClick(InputAction.CallbackContext context)
    {
        OnInputDetected(context);
        isLeftClick = true;
        
        
    }
    
    
    //IEnumerator RotateObject ()
    //{
    //    while (isLeftClick && dragAndDrop.isDragging)
    //    {

    //        rotation = _lookAction.ReadValue<Vector2>();
    //        rotation *= rotateSpeed;


    //        dragAndDrop.draggedObject.transform.Rotate(transform.right, rotation.y, Space.World);
    //        dragAndDrop.draggedObject.transform.Rotate(Vector3.up, rotation.x, Space.World);

    //        if (Gamepad.current != null)
    //        {
    //            Gamepad.current.SetMotorSpeeds(0, rotation.magnitude * 0.2f);
    //        }

    //        yield return null;

    //    }
    //    if (Gamepad.current != null)
    //    {
    //        Gamepad.current.SetMotorSpeeds(0, 0);
    //    }


    //    rotateCoroutine = null;
    //}

    Coroutine rotateCoroutine;
    private void OnLeftClickCancel(InputAction.CallbackContext context)
    {
        isLeftClick = false;
       
    }
    private void OnRightClick(InputAction.CallbackContext context)
    {
        OnInputDetected(context);
        isRightClick = true;
        
    }
    private void OnRightClickCancel(InputAction.CallbackContext context)
    {


        //if (dragAndDrop.isDragging) return;
        isRightClick = false;
        //if (weightCoroutine != null)
        //{
        //    StopCoroutine(weightCoroutine);
        //    weightCoroutine = null;
        //}
        //if (scaleCoroutine != null)
        //{
        //    StopCoroutine(scaleCoroutine);
        //    scaleCoroutine = null;
        //}
    }

    

    [HideInInspector]public bool onSlope;
    private void Update()
    {
        mainCamera.m_Lens.FieldOfView = Mathf.Lerp(mainCamera.m_Lens.FieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);

        CheckCamWobble();
        if (camWobbleOn)
            smoothShake.positionShake.frequency = Vector3.MoveTowards(smoothShake.positionShake.frequency, smoothShakeFrequency, camWobbleChangeSpeed * Time.deltaTime);
        
        onSlope = OnSlope();
        //Debug.Log("onslope:" + onSlope);
        _rb.useGravity = !onSlope;

        if (onSlope)
        {
            _rb.drag = 1f;
            jumpRaycastLenght = 1.02f * transform.localScale.y;
            _jumpForce=_jumpForceOnSlope;
            //_rb.AddForce(-slopeHit.normal * 1f, ForceMode.Force);
            //Debug.Log("ziplayinca buradayiz");
        }
        else 
        {
            _rb.drag = 1f;
            _jumpForce = _jumpForceAtStart;
            jumpRaycastLenght = 1.02f * transform.localScale.y;
        }


        if (isCrouching) 
        {
            _jumpForce = _jumpForceOnCrouch;
        }

        if (IsGrounded()/*&&!wallrunScript.isWallrunning*/)
        {
            coyoteTimeCounter = coyoteTime;
            
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //textManager.TextUpdate(dragAndDrop.isDragging, interactor._numFound, interactor._colliders,isKeyboardInput,isGamepadInput);
        

        HandleMovement();
        maxVelVector = Vector3.ClampMagnitude(_rb.velocity, maxVelocity);
        if (inAir)
            Debug.DrawRay(transform.position, -transform.up * jumpRaycastLenght, Color.cyan, 1f);

    }

    private void FixedUpdate()
    {
       
        HandleLook();
        

    }

    private void CheckCamWobble() 
    {
        if (smoothShake.enabled && !camWobbleOn)
        {
            smoothShake.enabled = false;
        } else if (!smoothShake.enabled&& camWobbleOn) 
        {
            smoothShake.enabled = true;
        }
        
    }

    public void ChangeWobble(bool b) 
    {
        camWobbleOn=b;

        PlayerPrefs.SetInt("camWobble", boolToInt(b));
        PlayerPrefs.Save();
    }

    int boolToInt(bool val)
    {
        if (val)
            return 1;
        else
            return 0;
    }

    


}
