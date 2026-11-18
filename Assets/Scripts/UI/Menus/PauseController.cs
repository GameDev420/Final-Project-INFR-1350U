using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] PauseMenu pauseMenu;

    [Header("Menu Button Fields")]
    [SerializeField] int mainMenuSceneIndex = 0;

    public void OnPauseInput(InputAction.CallbackContext context)
    {
        if (context.canceled && (SceneLoader.Instance == null || !SceneLoader.Instance.IsLoading)) pauseMenu.Toggle();
    }

    // Button Functions
    public void Resume()
    {
        pauseMenu.Close();
    }

    public void MainMenu()
    {
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(mainMenuSceneIndex);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneIndex);
    }
}
