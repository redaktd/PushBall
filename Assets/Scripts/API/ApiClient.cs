using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; }

    [Header("API Settings")]
    [SerializeField] private string baseUrl = "https://localhost:7114/api";

    private string _token;
    public bool IsLoggedIn => !string.IsNullOrEmpty(_token);
    public string Username { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved token on startup
        _token = PlayerPrefs.GetString("token", "");
        Username = PlayerPrefs.GetString("username", "");
    }

    // ---------------------------------------------------------------
    // Auth
    // ---------------------------------------------------------------

    public IEnumerator Login(string email, string password,
        System.Action<bool, string> onComplete)
    {
        var payload = JsonUtility.ToJson(new LoginRequest
        {
            email = email,
            password = password
        });

        yield return Post("/auth/login", payload, false, (success, json) =>
        {
            if (success)
            {
                var response = JsonUtility.FromJson<AuthResponse>(json);
                _token = response.token;
                Username = response.username;

                // Save to PlayerPrefs so login persists
                PlayerPrefs.SetString("token", _token);
                PlayerPrefs.SetString("username", Username);
                PlayerPrefs.Save();
            }
            onComplete(success, json);
        });
    }

    public IEnumerator Register(string username, string email, string password,
        System.Action<bool, string> onComplete)
    {
        var payload = JsonUtility.ToJson(new RegisterRequest
        {
            username = username,
            email = email,
            password = password
        });

        yield return Post("/auth/register", payload, false, (success, json) =>
        {
            if (success)
            {
                var response = JsonUtility.FromJson<AuthResponse>(json);
                _token = response.token;
                Username = response.username;

                PlayerPrefs.SetString("token", _token);
                PlayerPrefs.SetString("username", Username);
                PlayerPrefs.Save();
            }
            onComplete(success, json);
        });
    }

    public void Logout()
    {
        _token = "";
        Username = "";
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.Save();
    }

    // ---------------------------------------------------------------
    // Score Submission
    // ---------------------------------------------------------------

    public IEnumerator SubmitScore(SessionData data,
        System.Action<bool, string> onComplete)
    {
        var payload = JsonUtility.ToJson(data);
        yield return Post("/scores", payload, true, onComplete);
    }

    // ---------------------------------------------------------------
    // HTTP Helpers
    // ---------------------------------------------------------------

    private IEnumerator Post(string path, string json, bool requiresAuth,
    System.Action<bool, string> onComplete)
    {
        var url = baseUrl + path;

        using var request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (requiresAuth && IsLoggedIn)
            request.SetRequestHeader("Authorization", "Bearer " + _token);

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;
        string response = request.downloadHandler.text;

        onComplete(success, response);
    }

    // ---------------------------------------------------------------
    // Request / Response models
    // ---------------------------------------------------------------

    [System.Serializable] private class LoginRequest { public string email, password; }
    [System.Serializable] private class RegisterRequest { public string username, email, password; }
    [System.Serializable] private class AuthResponse { public string token, username; public int playerId; }
}