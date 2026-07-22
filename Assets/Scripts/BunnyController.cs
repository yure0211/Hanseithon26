using UnityEngine;
using UnityEngine.InputSystem;

public class BunnyController : MonoBehaviour
{
    [SerializeField] InputAction movement;
    [SerializeField] InputAction jump;
    [SerializeField] InputAction jumpHold;

    private float _horizontalMove;
    private bool _isJumpPress;
    private bool _isJumpHold;
    
    

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
        
    }

    private void GetInput()
    {

        
    }
}
