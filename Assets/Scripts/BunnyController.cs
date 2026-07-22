using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BunnyController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Animator animator;
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
    public Vector2 velocity;

    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
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

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
        jumpQueued = false;

        if (jumpHeld && !isGrounded && body.linearVelocityY > 0f)
        {
            body.AddForceY(jumpForce * jumpHoldForceRatio, ForceMode2D.Force);
        }

        velocity = body.linearVelocity;
        UpdateAnimatorState();
    }

    private void UpdateAnimatorState()
    {
        if (animator == null) return;
        bool a = Mathf.Abs(velocity.x) > 0.01f;
        if (a)
        {
            transform.localScale = new Vector3(horizontalMove, 1f, 1f); 
            
        }
        animator.SetBool("IsRun",a);
        animator.SetBool("IsGround", isGrounded);
        animator.SetFloat("YVelocity", velocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + groundPosition, groundSize);
    }
}
