using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private bool paused;

    public static UIManager _Instance { get; set; }

    [SerializeField] private GameObject pauseScreen;

    private void Awake()
    {
        _Instance = this;
    }

    public void Restart()
    {
        TransitionManager._Instance.FadeOut(delegate
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    private void TogglePauseState()
    {
        if (paused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        paused = true;

        Time.timeScale = 0;

        pauseScreen.SetActive(true);
    }

    public void Unpause()
    {
        paused = false;

        Time.timeScale = 1;

        pauseScreen.SetActive(false);
    }

    public void PlayGame()
    {
        TransitionManager._Instance.FadeOut(() => SceneManager.LoadScene("Tutorial"));
    }

    public void Simulate()
    {
        TransitionManager._Instance.FadeOut(() => SceneManager.LoadScene("Simulation"));
    }

    public void GoToMainMenu()
    {
        TransitionManager._Instance.FadeOut(() => SceneManager.LoadScene(0));
    }

    public void Exit()
    {
        TransitionManager._Instance.FadeOut(() => Application.Quit());
    }

    public void GoToLevelSelect()
    {
        TransitionManager._Instance.FadeOut(() => SceneManager.LoadScene(1));
    }

    public void GoToLevel(int buildIndex)
    {
        TransitionManager._Instance.FadeOut(() => SceneManager.LoadScene(buildIndex));
    }

    public void SavePlayerPrefs()
    {
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseState();
        }
    }

    [ContextMenu("ClearPlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
