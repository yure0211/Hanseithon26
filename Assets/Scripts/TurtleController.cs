using UnityEngine;
using UnityEngine.InputSystem;

public class TurtleController : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Animator _animator;

    [SerializeField] InputAction movement;

    private Vector2 move;
    [SerializeField] float moveForce;

    private void OnEnable()
    {
        movement.Enable();

    }
    private void OnDisable()
    {
        movement.Disable();
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

    }

    void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        _rb.AddForce(move * moveForce, ForceMode2D.Force);
    }

    private void GetInput()
    {
        move = movement.ReadValue<Vector2>();
    }


}
