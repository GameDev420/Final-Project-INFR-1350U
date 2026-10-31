using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] bool startWithGameplay = true;
    [SerializeField] GameObject gameplayCamera;
    [Tooltip("Components to be disabled when switching from regular gameplay to the floating camera (or vice-versa).")]
    [SerializeField] MonoBehaviour[] gameplayComponents;
    [Space]
    [Tooltip("Floating camera object to be enabled when switching from regular gameplay to the floating camera (or vise-versa).")]
    [SerializeField] GameObject floatingCamera;

    bool gameplayState;

    private void Awake() => ApplyState(startWithGameplay);

    public void SwitchCameraInput(InputAction.CallbackContext context)
    {
        if (!context.performed || !isActiveAndEnabled) return;

        ToggleCamera();
    }

    void ToggleCamera() => ApplyState(!gameplayState);

    void ApplyState(bool newState)
    {
        // Return if state is already active.
        if (gameplayState == newState) return;

        if (gameplayComponents != null && gameplayComponents.Length > 0)
        {
            for (int i = 0; i < gameplayComponents.Length; i++)
            {
                var c = gameplayComponents[i];
                if (c != null) c.enabled = newState;
            }
        }

        gameplayCamera.SetActive(newState);
        floatingCamera.SetActive(!newState);

        gameplayState = newState;
    }
}
