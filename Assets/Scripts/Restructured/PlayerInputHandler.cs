using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// The ONLY script that knows about the Input System.
/// Translates raw actions into game-meaningful values (polled) and events (pushed).
/// Runs early so every other script sees this frame's input.
/// </summary>
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    // --- Continuous input: other scripts POLL these every frame ---
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    // --- Discrete input: other scripts SUBSCRIBE to these ---
    public event Action JumpPressed;
    public event Action InteractPressed;
    public event Action InteractReleased;
    public event Action SelectPressed;
    public event Action SelectReleased;
    public event Action<float> GravityControlPerformed;
    public event Action GravityReleasePerformed;
    public event Action<int> ChangeCurrentLinkPerformed;
    public event Action PausePerformed;
    public event Action RestartPerformed;

    PlayerInput playerInput;
    InputAction moveAction, lookAction, jumpAction, interactAction, selectAction, gravityControlAction, gravityReleaseAction, changeCurrentLinkAction, pauseAction, restartAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        var actions = playerInput.actions;

        moveAction              = actions.FindAction("Move", throwIfNotFound: true);
        lookAction              = actions.FindAction("Look", throwIfNotFound: true);
        jumpAction              = actions.FindAction("Jump", throwIfNotFound: true);
        interactAction          = actions.FindAction("Interact", throwIfNotFound: true);
        selectAction            = actions.FindAction("Select", throwIfNotFound: true);
        gravityControlAction    = actions.FindAction("GravityControl", throwIfNotFound: true);
        gravityReleaseAction    = actions.FindAction("GravityRelease", throwIfNotFound: true);
        changeCurrentLinkAction = actions.FindAction("CartridgeChange", throwIfNotFound: true);

        pauseAction             = actions.FindAction("Pause", throwIfNotFound: true);
        restartAction           = actions.FindAction("Restart", throwIfNotFound: true);
    }

    void OnEnable()
    {
        jumpAction.performed              += OnJumpPerformed;
        interactAction.performed          += OnInteractPerformed;
        interactAction.canceled           += OnInteractReleased;
        selectAction.performed            += OnSelectPerformed;
        selectAction.canceled             += OnSelectReleased;
        gravityControlAction.performed    += OnGravityControlPerformed;
        gravityReleaseAction.performed    += OnGravityReleasePerformed;
        changeCurrentLinkAction.performed += OnChangeCurrentLinkPerformed;

        pauseAction.performed             += OnPausePerformed;
        restartAction.performed           += OnRestartPerformed;
    }

    void OnDisable()
    {
        jumpAction.performed              -= OnJumpPerformed;
        interactAction.performed          -= OnInteractPerformed;
        interactAction.canceled           -= OnInteractReleased;
        selectAction.performed            -= OnSelectPerformed;
        selectAction.canceled             -= OnSelectReleased;
        gravityControlAction.performed    -= OnGravityControlPerformed;
        gravityReleaseAction.performed    -= OnGravityReleasePerformed;
        changeCurrentLinkAction.performed -= OnChangeCurrentLinkPerformed;

        pauseAction.performed             -= OnPausePerformed;
        restartAction.performed           -= OnRestartPerformed;

        // Don't leave stale values behind when input is turned off
        Move = Vector2.zero;
        Look = Vector2.zero;
    }

    void Update()
    {
        Move = moveAction.ReadValue<Vector2>();
        Look = lookAction.ReadValue<Vector2>();
    }

    void OnJumpPerformed(InputAction.CallbackContext _)            => JumpPressed?.Invoke();
    void OnInteractPerformed(InputAction.CallbackContext _)        => InteractPressed?.Invoke();
    void OnInteractReleased(InputAction.CallbackContext _)         => InteractReleased?.Invoke();
    void OnSelectPerformed(InputAction.CallbackContext _)          => SelectPressed?.Invoke();
    void OnSelectReleased(InputAction.CallbackContext _)           => SelectReleased?.Invoke();
    void OnGravityControlPerformed(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();
        if (Mathf.Approximately(value, 0f)) return;
        GravityControlPerformed?.Invoke(Mathf.Sign(value));
    }
    void OnGravityReleasePerformed(InputAction.CallbackContext _)  => GravityReleasePerformed?.Invoke();
    void OnChangeCurrentLinkPerformed(InputAction.CallbackContext ctx)
    {
        float cartridge = ctx.ReadValue<float>();
        ChangeCurrentLinkPerformed?.Invoke((int) cartridge-1);
    }
    void OnPausePerformed(InputAction.CallbackContext _)           => PausePerformed?.Invoke();
    void OnRestartPerformed(InputAction.CallbackContext _)
    {
        RestartPerformed?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- Helpers so other systems (pause menu, cutscenes) never touch PlayerInput directly ---
    public void EnableGameplayInput() => playerInput.SwitchCurrentActionMap("Player");
    public void EnableUIInput()       => playerInput.SwitchCurrentActionMap("UI");
}