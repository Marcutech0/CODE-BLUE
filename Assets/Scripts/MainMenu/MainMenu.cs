using CodeBlue;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void HostGame()
    {
        LobbyManager.Instance.StartServer(() => {
            
        });
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
