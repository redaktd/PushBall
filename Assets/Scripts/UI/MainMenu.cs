using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuParent, levelMenuParent, loggedInParent, logInPageParent, registerPageParent;
    [SerializeField] private Button playBtn, levelSelectBtn, leaderboardBtn, logInBtn, regBtn, logOutBtn, backBtn, logInPageBtn, regPageBtn;
    [SerializeField] private Button[] levelBtn;
    [SerializeField] private TMP_Text loggedInTxt, notifTxt;
    [SerializeField] private TMP_InputField logInEmailField, logInPassField, regUsernameField, regEmailField, regPassField;

    private bool loggedIn = false;
    private string userName, email, password;

    private void Start()
    {
        if (ApiClient.Instance.IsLoggedIn)
        {
            loggedIn = true;
            logInBtn.gameObject.SetActive(false);
            regBtn.gameObject.SetActive(false);
            loggedInTxt.gameObject.SetActive(true);
            logOutBtn.gameObject.SetActive(true);
            leaderboardBtn.interactable = true;
            loggedInTxt.text = $"Logged in as \"{ApiClient.Instance.Username}\"";
        }
    }

    public void PlayBtn()
    {
        SceneManager.LoadScene(1);
    }
    public void LevelSelectBtn()
    {
        mainMenuParent.SetActive(false);
        levelMenuParent.SetActive(true);
        backBtn.gameObject.SetActive(true);
    }
    public void LeaderboardBtn()
    {
        if (loggedIn)
        {
            Debug.Log("show leaderboard");
        }
        else
        {
            notifTxt.gameObject.SetActive(true);
            notifTxt.text = "Log In to access the leaderboard";
            StartCoroutine(warnTxtCO());
            leaderboardBtn.interactable = false;
        }
    }
    public void LoginBtn()
    {
        logInPageParent.SetActive(true);
        backBtn.gameObject.SetActive(true);
        mainMenuParent.SetActive(false);
        loggedInParent.SetActive(false);
    }
    public void LogInPageBtn()
    {
        if (ValidateInputs("", logInEmailField.text, logInPassField.text, false))
        {
            StartCoroutine(LoginCoroutine(logInEmailField.text, logInPassField.text));
        }
    }
    public void RegBtn()
    {
        registerPageParent.SetActive(true);
        backBtn.gameObject.SetActive(true);
        mainMenuParent.SetActive(false);
        loggedInParent.SetActive(false);
    }
    public void RegPageBtn()
    {
        if (ValidateInputs(regUsernameField.text, regEmailField.text, regPassField.text, true))
        {
            StartCoroutine(RegisterCoroutine(regUsernameField.text, regEmailField.text, regPassField.text));
        }
    }
    public void LogOutBtn()
    {
        ApiClient.Instance.Logout();
        loggedIn = false;
        logOutBtn.gameObject.SetActive(false);
        loggedInTxt.gameObject.SetActive(false);
        logInBtn.gameObject.SetActive(true);
        regBtn.gameObject.SetActive(true);
    }
    public void LevelBtn(int levelId)
    {
        SceneManager.LoadScene(levelId);
        Debug.Log($"Loading level {levelId}");
    }
    public void BackBtn()
    {
        if(levelMenuParent.activeInHierarchy == true)
        {
            levelMenuParent.SetActive(false);
            mainMenuParent.SetActive(true);
        }
        if (logInPageParent.activeInHierarchy == true)
        {
            logInPageParent.SetActive(false);
            loggedInParent.SetActive(true);
            mainMenuParent.SetActive(true);
        }
        if (registerPageParent.activeInHierarchy == true)
        {
            registerPageParent.SetActive(false);
            loggedInParent.SetActive(true);
            mainMenuParent.SetActive(true);
        }
        backBtn.gameObject.SetActive(false);
    }

    private bool ValidateInputs(string userName, string eMail, string pass, bool regPage)
    {
        // clear previous error
        notifTxt.text = "";

        if (regPage)
        {
            string usernameError = ValidateUsername(userName);
            if (usernameError != null)
            {
                notifTxt.text = usernameError;
                notifTxt.gameObject.SetActive(true);
                StartCoroutine(warnTxtCO());
                return false;
            }
        }
        string emailError = ValidateEmail(eMail);
        if (emailError != null)
        {
            notifTxt.text = emailError;
            notifTxt.gameObject.SetActive(true);
            StartCoroutine(warnTxtCO());
            return false;
        }

        string passwordError = ValidatePass(pass);
        if (passwordError != null)
        {
            notifTxt.text = passwordError;
            notifTxt.gameObject.SetActive(true);
            StartCoroutine(warnTxtCO());
            return false;
        }

        return true;
    }

    private static string ValidateUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return "Username cannot be empty.";
        if (username.Length < 3)
            return "Username must be at least 3 characters.";
        if (username.Length > 20)
            return "Username cannot exceed 20 characters.";
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
            return "Username can only contain letters and numbers.";
        return null; // null means valid
    }
    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return "Email cannot be empty.";
        if (email.Length > 100)
            return "Email is too long.";
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return "Please enter a valid email address.";
        return null;
    }
    private static string ValidatePass(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "Password cannot be empty.";
        if (password.Length < 8)
            return "Password must be at least 8 characters.";
        return null;
    }
    private IEnumerator warnTxtCO()
    {
        yield return new WaitForSeconds(2f);
        notifTxt.gameObject.SetActive(false);
    }

    private IEnumerator LoginCoroutine(string email, string password)
    {
        logInPageBtn.interactable = false;
        notifTxt.text = "Logging in...";
        notifTxt.gameObject.SetActive(true);

        yield return ApiClient.Instance.Login(email, password, (success, response) =>
        {
            if (success)
            {
                logInEmailField.text = "";
                logInPassField.text = "";
                logInPageParent.SetActive(false);
                backBtn.gameObject.SetActive(false);
                mainMenuParent.SetActive(true);
                loggedInParent.SetActive(true);
                logInBtn.gameObject.SetActive(false);
                regBtn.gameObject.SetActive(false);
                loggedInTxt.gameObject.SetActive(true);
                logOutBtn.gameObject.SetActive(true);
                loggedInTxt.text = $"Logged in as \"{ApiClient.Instance.Username}\"";
                loggedIn = true;
                leaderboardBtn.interactable = true;
                notifTxt.gameObject.SetActive(false);
            }
            else
            {
                notifTxt.text = "Invalid email or password.";
                notifTxt.gameObject.SetActive(true);
                StartCoroutine(warnTxtCO());
            }
        });

        logInPageBtn.interactable = true;
    }

    private IEnumerator RegisterCoroutine(string username, string email, string password)
    {
        regPageBtn.interactable = false;
        notifTxt.text = "Creating account...";
        notifTxt.gameObject.SetActive(true);

        yield return ApiClient.Instance.Register(username, email, password, (success, response) =>
        {
            if (success)
            {
                regUsernameField.text = "";
                regEmailField.text = "";
                regPassField.text = "";
                registerPageParent.SetActive(false);
                backBtn.gameObject.SetActive(false);
                mainMenuParent.SetActive(true);
                loggedInParent.SetActive(true);
                logInBtn.gameObject.SetActive(false);
                regBtn.gameObject.SetActive(false);
                loggedInTxt.gameObject.SetActive(true);
                logOutBtn.gameObject.SetActive(true);
                loggedInTxt.text = $"Logged in as \"{ApiClient.Instance.Username}\"";
                loggedIn = true;
                leaderboardBtn.interactable = true;
                notifTxt.gameObject.SetActive(false);
            }
            else
            {
                notifTxt.text = "Registration failed. Email may already be in use.";
                notifTxt.gameObject.SetActive(true);
                StartCoroutine(warnTxtCO());
            }
        });

        regPageBtn.interactable = false;
    }
}
