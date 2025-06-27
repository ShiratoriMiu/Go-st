using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action<Vector2> OnTouchStart;
    public event Action<Vector2> OnTouchMove;
    public event Action OnTouchEnd;

    private PlayerInputAction inputAction;

    public event Action OnAnyTouchDown;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        inputAction = new PlayerInputAction();
    }

    private void OnEnable()
    {
        inputAction.Enable();
        inputAction.Player.Touch.performed += HandleTouchMove;
        inputAction.Player.TouchClick.started += HandleTouchStart;
        inputAction.Player.TouchClick.canceled += HandleTouchEnd;
    }

    private void OnDisable()
    {
        inputAction.Player.Touch.performed -= HandleTouchMove;
        inputAction.Player.TouchClick.started -= HandleTouchStart;
        inputAction.Player.TouchClick.canceled -= HandleTouchEnd;
        inputAction.Disable();
    }

    private void HandleTouchStart(InputAction.CallbackContext ctx)
    {
        Vector2 pos = ReadInputPosition();
        OnTouchStart?.Invoke(pos);
        OnAnyTouchDown?.Invoke(); // どこでもタッチしたら呼び出す
    }

    private void HandleTouchMove(InputAction.CallbackContext ctx)
    {
        Vector2 pos = ReadInputPosition();
        OnTouchMove?.Invoke(pos);
    }

    private void HandleTouchEnd(InputAction.CallbackContext ctx)
    {
        OnTouchEnd?.Invoke();
    }

    private Vector2 ReadInputPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Vector2.zero;
    }
}
