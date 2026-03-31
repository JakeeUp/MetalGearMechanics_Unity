using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UITransitionManager : MonoBehaviour
{
    public CinemachineVirtualCamera currentCamera;
    public Button startButton;
    public Button exitButton;
    public Button menuButton;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
        if (menuButton != null)
            menuButton.onClick.AddListener(StartMenu);

        EnableCamera(currentCamera);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void StartMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void UpdateCamera(CinemachineVirtualCamera target)
    {
        DisableCamera(currentCamera);
        currentCamera = target;
        EnableCamera(currentCamera);
    }

    private void EnableCamera(CinemachineVirtualCamera camera)
    {
        if (camera != null)
            camera.gameObject.SetActive(true);
    }

    private void DisableCamera(CinemachineVirtualCamera camera)
    {
        if (camera != null)
            camera.gameObject.SetActive(false);
    }
}
