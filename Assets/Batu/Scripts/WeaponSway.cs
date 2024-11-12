using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    private DefaultPlayerActions _defaultPlayerActions;

    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmoothValue = 4.0f;

    private Vector3 initialPosition;
    private InputAction lookAction;

    private Rigidbody playerRB;

    private void Awake()
    {
        _defaultPlayerActions = new DefaultPlayerActions();
        
        lookAction = _defaultPlayerActions.Player.Look;
        lookAction.Enable();
    }
    
    private void Start()
    {
        GameObject.FindGameObjectWithTag("Player").gameObject.TryGetComponent<Rigidbody>(out playerRB);
        initialPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float movementX = (-lookInput.x * swayAmount)+ (-playerRB.velocity.x * swayAmount)/2 ;
        float movementY = (-lookInput.y * swayAmount) - (-playerRB.velocity.z * swayAmount)/2 + (-playerRB.velocity.y * swayAmount) / 2;
        //float movementZ = (-playerRB.velocity.y * swayAmount)/2;

        movementX = Mathf.Clamp(movementX, -maxSwayAmount, maxSwayAmount);
        movementY = Mathf.Clamp(movementY, -maxSwayAmount, maxSwayAmount);
        //movementZ = Mathf.Clamp(movementZ, -maxSwayAmount, maxSwayAmount);

        Vector3 finalPosition = new Vector3(movementX, movementY,0 /*movementZ*/);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * swaySmoothValue);
    }

    private void OnDisable()
    {
        lookAction.Disable();
    }
}
