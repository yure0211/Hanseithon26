using UnityEngine;
using UnityEngine.InputSystem;

public class BunnyController : MonoBehaviour
{
    [SerializeField] InputAction movement;
    [SerializeField] InputAction jump;

    private float _horizontalMove;
    private bool _isJumpPress;
    private bool _isJumpHold;

    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Animator _animator;

    [SerializeField] float _speed;
    public float _Yvelocity;
    [SerializeField] float jumpForce;

    public bool isGround;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Vector3 groundPosition;
    [SerializeField] Vector3 groundSize;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        movement.Enable();
        jump.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        jump.Disable();
    }

    void Update()
    {
        GetInput();

        JumpTrigg();
    }
    private void FixedUpdate()
    {
        _Yvelocity = _rb.linearVelocity.y;
        isGround = IsGround();
        MoveX();
        JumpHold();

    }

    private void GetInput()
    {
        _horizontalMove = movement.ReadValue<float>();
        _isJumpPress = jump.WasPressedThisFrame();
        _isJumpHold = jump.IsPressed();
    }

    private void JumpTrigg()
    {
        if (!_isJumpPress) return;
        if (!isGround) return;
        _rb.AddForceY(jumpForce, ForceMode2D.Impulse);

    }

    private void JumpHold()
    {
        if(!_isJumpHold) return;
        if (_Yvelocity < 0) return;
        if (isGround) return;
        _rb.AddForceY(jumpForce * 2/3, ForceMode2D.Force);
    }

    private void MoveX()
    {
        _rb.linearVelocityX = _speed * _horizontalMove;
    }

    private bool IsGround()
    {
        bool g = Physics2D.OverlapBox(groundPosition + transform.position, groundSize,0,groundLayer);
        return g;
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundPosition, groundSize);

    }
}
