using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] GameObject pauseScreen, pauseBtn;

    public void RestartBtn()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameManager.Instance.GetLevelId());
    }
    public void MenuBtn()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    public void ResumeBtn()
    {
        pauseBtn.SetActive(true);
        pauseScreen.SetActive(false);
        Time.timeScale = 1f;
    }
    public void PauseBtn()
    {
        pauseBtn.SetActive(false);
        pauseScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Show()
    {
        PauseBtn();
    }
}
