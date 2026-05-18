using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text deathText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text submitText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private UnityEngine.UI.Button submitBtn;

    private int _levelId;
    private float _timeSeconds;
    private int _deaths;
    private int _score;

    private void Start()
    {
        panel.SetActive(false);
    }

    public void Show(int levelId, float timeSeconds, int deaths, int score)
    {
        _levelId = levelId;
        _timeSeconds = timeSeconds;
        _deaths = deaths;
        _score = score;

        titleText.text = ("Level " + levelId + " Complete!");
        timeText.text = GameManager.Instance.FormatTime(timeSeconds);
        deathText.text = deaths.ToString();
        scoreText.text = score.ToString();
        panel.SetActive(true);
    }

    public void NextBtn()
    {
        int nextLevel = GameManager.Instance.GetLevelId() + 1;
        if (SceneManager.GetSceneByName("Level_"+ nextLevel.ToString())!=null)
        {
            SceneManager.LoadScene(nextLevel);
        }
    }

    public void UpdateSubmitButton()
    {
        if (!ApiClient.Instance.IsLoggedIn)
        {
            statusText.text = "Please log in from the main menu.";
            return;
        }
        submitBtn.interactable = false;
        statusText.text = "Submitting...";
        StartCoroutine(SubmitScore());
    }

    private IEnumerator SubmitScore()
    {
        var data = new SessionData
        {
            levelId = _levelId,
            timeSeconds = _timeSeconds,
            deaths = _deaths,
            score = _score
        };

        yield return ApiClient.Instance.SubmitScore(data, (success, response) =>
        {
            if (success)
            {
                statusText.text = "Score submitted!";
                submitBtn.gameObject.SetActive(false);
            }
            else
            {
                statusText.text = "Submission failed. Try again.";
                submitBtn.interactable = true;
            }
        });
    }
}
