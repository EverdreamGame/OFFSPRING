using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ToScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void QuitApp()
    {
       Application.Quit();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
