using UnityEngine;

// Currently only used for button functionality, while the general Menu script handles things like animations
public class TitleScreenController : MonoBehaviour
{
    [SerializeField] int gameSceneIndex = 1;

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
