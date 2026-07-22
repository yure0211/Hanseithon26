using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TurtleController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Animator animator; 
    [SerializeField] private float moveForce = 5f;
    [SerializeField] private float maximumSpeed = 4f;

    private Vector2 movement;
    private Vector2 facingDirection = Vector2.right;

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
        movement = Vector2.zero;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            movement = Vector2.zero;
            return;
        }

        movement = Vector2.zero;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            movement.x -= 1f;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            movement.x += 1f;
        }
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            movement.y -= 1f;
        }
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            movement.y += 1f;
        }

        UpdateFacingDirection(keyboard);
        animator.SetBool("IsRun", movement.magnitude > 0.1f);

        movement = Vector2.ClampMagnitude(movement, 1f);
    }

    private void FixedUpdate()
    {
        if (body == null)
        {
            return;
        }

        FaceInputDirection();
        body.AddForce(movement * moveForce, ForceMode2D.Force);
        body.linearVelocity = Vector2.ClampMagnitude(body.linearVelocity, maximumSpeed);
    }

    private void FaceInputDirection()
    {
        float facingAngle;
        if (facingDirection == Vector2.right)
        {
            facingAngle = 0f;
        }
        else if (facingDirection == Vector2.down)
        {
            facingAngle = -90f;
        }
        else if (facingDirection == Vector2.left)
        {
            facingAngle = 180f;
        }
        else
        {
            facingAngle = 90f;
        }

        body.SetRotation(facingAngle);
    }

    private void UpdateFacingDirection(Keyboard keyboard)
    {
        bool hasHorizontalInput = Mathf.Abs(movement.x) > 0.01f;
        bool hasVerticalInput = Mathf.Abs(movement.y) > 0.01f;

        if (hasHorizontalInput && !hasVerticalInput)
        {
            facingDirection = movement.x > 0f ? Vector2.right : Vector2.left;
        }
        else if (hasVerticalInput && !hasHorizontalInput)
        {
            facingDirection = movement.y > 0f ? Vector2.up : Vector2.down;
        }

        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            facingDirection = Vector2.left;
        }
        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            facingDirection = Vector2.right;
        }
        if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
        {
            facingDirection = Vector2.down;
        }
        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
        {
            facingDirection = Vector2.up;
        }
    }
}
