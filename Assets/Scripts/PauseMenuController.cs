using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    private InputAction crouchAction;
    private bool isPaused = false;

    private void Awake()
    {
        // Find the existing Crouch action in the default Input System actions.
        crouchAction = InputSystem.actions.FindAction("Crouch", true);
    }

    private void OnEnable()
    {
        if (crouchAction != null)
            crouchAction.performed += OnCrouch;
    }

    private void OnDisable()
    {
        if (crouchAction != null)
            crouchAction.performed -= OnCrouch;
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
        SetPaused(false);
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        SetPaused(true);
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        Cursor.lockState = paused
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = paused;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}