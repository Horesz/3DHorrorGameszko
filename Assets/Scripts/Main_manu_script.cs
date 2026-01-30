using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_manu_script : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Hospital_main");
    }

    public void QuitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
