using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BunnyController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpHoldForceRatio = 0.67f;
    [SerializeField] private LayerMask groundLayer = 1 << 6;
    [SerializeField] private Vector3 groundPosition = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 groundSize = new Vector3(0.7f, 0.3f, 0f);

    private float horizontalMove;
    private bool jumpQueued;
    private bool jumpHeld;
    private bool isGrounded;

    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

    private void OnDisable()
    {
        horizontalMove = 0f;
        jumpQueued = false;
        jumpHeld = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            horizontalMove = 0f;
            jumpHeld = false;
            return;
        }

        horizontalMove = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontalMove -= 1f;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontalMove += 1f;
        }

        jumpQueued |= keyboard.spaceKey.wasPressedThisFrame;
        jumpHeld = keyboard.spaceKey.isPressed;
    }

    private void FixedUpdate()
    {
        if (body == null)
        {
            return;
        }

        isGrounded = Physics2D.OverlapBox(
            transform.position + groundPosition,
            groundSize,
            0f,
            groundLayer) != null;

        body.linearVelocityX = moveSpeed * horizontalMove;

        if (jumpQueued && isGrounded)
        {
            body.AddForceY(jumpForce, ForceMode2D.Impulse);
        }
        jumpQueued = false;

        if (jumpHeld && !isGrounded && body.linearVelocityY > 0f)
        {
            body.AddForceY(jumpForce * jumpHoldForceRatio, ForceMode2D.Force);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + groundPosition, groundSize);
    }
}
