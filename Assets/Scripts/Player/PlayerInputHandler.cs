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
    public event Action ModeSwitchPressed;
    public event Action<float> GravityControlPressed;
    public event Action GravityReleasePressed;
    public event Action<int> ChangeCurrentLinkPressed;
    public event Action PausePressed;
    public event Action RestartPressed;
    public event Action RewindPressed;
    public event Action RewindReleased;

    PlayerInput playerInput;
    InputAction moveAction, lookAction, jumpAction, interactAction, modeSwitchAction, gravityControlAction, gravityReleaseAction, changeCurrentLinkAction, pauseAction, restartAction, rewindAction;

    private static PlayerInputHandler _instance;

    public static PlayerInputHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PlayerInputHandler>();

                if (_instance == null)
                {
                    var go = new GameObject("PlayerInputHandler");
                    _instance = go.AddComponent<PlayerInputHandler>();
                }
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        playerInput = GetComponent<PlayerInput>();
        var actions = playerInput.actions;

        moveAction              = actions.FindAction("Move", throwIfNotFound: true);
        lookAction              = actions.FindAction("Look", throwIfNotFound: true);
        jumpAction              = actions.FindAction("Jump", throwIfNotFound: true);
        interactAction          = actions.FindAction("Interact", throwIfNotFound: true);
        modeSwitchAction        = actions.FindAction("ModeSwitch", throwIfNotFound: true);
        gravityControlAction    = actions.FindAction("GravityControl", throwIfNotFound: true);
        gravityReleaseAction    = actions.FindAction("GravityRelease", throwIfNotFound: true);
        changeCurrentLinkAction = actions.FindAction("CartridgeChange", throwIfNotFound: true);

        pauseAction             = actions.FindAction("Pause", throwIfNotFound: true);
        restartAction           = actions.FindAction("Restart", throwIfNotFound: true);
        rewindAction            = actions.FindAction("Rewind", throwIfNotFound: true);
    }

    void OnEnable()
    {
        jumpAction.performed              += OnJumpPressed;
        interactAction.performed          += OnInteractPressed;
        interactAction.canceled           += OnInteractReleased;
        modeSwitchAction.performed        += OnModeSwitchPressed;
        gravityControlAction.performed    += OnGravityControlPressed;
        gravityReleaseAction.performed    += OnGravityReleasePressed;
        changeCurrentLinkAction.performed += OnChangeCurrentLinkPressed;

        pauseAction.performed             += OnPausePressed;
        restartAction.performed           += OnRestartPressed;
        rewindAction.performed            += OnRewindPressed;
        rewindAction.canceled             += OnRewindReleased;
    }

    void OnDisable()
    {
        jumpAction.performed              -= OnJumpPressed;
        interactAction.performed          -= OnInteractPressed;
        interactAction.canceled           -= OnInteractReleased;
        modeSwitchAction.performed        -= OnModeSwitchPressed;
        gravityControlAction.performed    -= OnGravityControlPressed;
        gravityReleaseAction.performed    -= OnGravityReleasePressed;
        changeCurrentLinkAction.performed -= OnChangeCurrentLinkPressed;

        pauseAction.performed             -= OnPausePressed;
        restartAction.performed           -= OnRestartPressed;
        rewindAction.performed            -= OnRewindPressed;
        rewindAction.canceled             -= OnRewindReleased;

        Move = Vector2.zero;
        Look = Vector2.zero;
    }

    void Update()
    {
        Move = moveAction.ReadValue<Vector2>();
        Look = lookAction.ReadValue<Vector2>();
    }

    void OnJumpPressed(InputAction.CallbackContext _)            => JumpPressed?.Invoke();
    void OnInteractPressed(InputAction.CallbackContext _)        => InteractPressed?.Invoke();
    void OnInteractReleased(InputAction.CallbackContext _)       => InteractReleased?.Invoke();
    void OnModeSwitchPressed(InputAction.CallbackContext _)      => ModeSwitchPressed?.Invoke();
    void OnGravityControlPressed(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();
        if (Mathf.Approximately(value, 0f)) return;
        GravityControlPressed?.Invoke(Mathf.Sign(value));
    }
    void OnGravityReleasePressed(InputAction.CallbackContext _)  => GravityReleasePressed?.Invoke();
    void OnChangeCurrentLinkPressed(InputAction.CallbackContext ctx)
    {
        float cartridge = ctx.ReadValue<float>();
        ChangeCurrentLinkPressed?.Invoke((int) cartridge-1);
    }
    void OnPausePressed(InputAction.CallbackContext _)           => PausePressed?.Invoke();
    void OnRestartPressed(InputAction.CallbackContext _)
    {
        RestartPressed?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void OnRewindPressed(InputAction.CallbackContext _)          => RewindPressed?.Invoke();
    void OnRewindReleased(InputAction.CallbackContext _)         => RewindReleased?.Invoke();

    // --- Helpers so other systems (pause menu, cutscenes) never touch PlayerInput directly ---
    public void EnableGameplayInput() => playerInput.SwitchCurrentActionMap("Player");
    public void EnableUIInput()       => playerInput.SwitchCurrentActionMap("UI");
}