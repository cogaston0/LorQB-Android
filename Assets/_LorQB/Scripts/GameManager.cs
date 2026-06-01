using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void ReloadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void OnCubeSelected(string cubeName) { }

    public void OnCubeDeselected(string cubeName) { }

    public bool IsInputAllowed()
    {
        return true;
    }
}
