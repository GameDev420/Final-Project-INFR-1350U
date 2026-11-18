using UnityEngine;

// Currently only used for button functionality, while the general Menu script handles things like animations
public class TitleScreenController : MonoBehaviour
{
    [SerializeField] int gameSceneIndex = 1;

    private void Awake()
    {
        // Ensure cursor is unlocked and visible if player unpaused during loading screen animation (or what have you)
        Utility.SetCursorState(false);
    }

    public void StartGame()
    {
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(gameSceneIndex);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneIndex);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
