using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TurtleController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private float moveForce = 5f;
    [SerializeField] private float maximumSpeed = 4f;

    private Vector2 movement;

    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
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

        movement = Vector2.ClampMagnitude(movement, 1f);
    }

    private void FixedUpdate()
    {
        if (body == null)
        {
            return;
        }

        body.AddForce(movement * moveForce, ForceMode2D.Force);
        body.linearVelocity = Vector2.ClampMagnitude(body.linearVelocity, maximumSpeed);
    }
}
