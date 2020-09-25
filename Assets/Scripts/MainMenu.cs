using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    
    [Header("UI")]
    [SerializeField] private InputField nameInputField = null;
    [SerializeField] private Button submitButton = null;
    [SerializeField] private NetworkManagerLobby networkManager = null;

    [SerializeField] private GameObject mainMenuBackground= null;

    [SerializeField] private InputField ipAddressInputField = null;

    public static string DisplayName { get; private set;}

    private const string PlayerPrefsNameKey = "PlayerName";

       public void HostLobby()
    {

        SavePlayerName();

        networkManager.StartHost();

        mainMenuBackground.SetActive(false);

    }

    private void Start() => SetUpInputField();

    private void SetUpInputField() 
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return; }

        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name)
    {
        submitButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;

        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }

    private void OnEnable()
    {
        NetworkManagerLobby.OnClientConnected += HandleClientConnected;
        NetworkManagerLobby.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerLobby.OnClientConnected -= HandleClientConnected;
        NetworkManagerLobby.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        SavePlayerName();

        string ipAddress = ipAddressInputField.text;

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        submitButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        submitButton.interactable = true;

        mainMenuBackground.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        submitButton.interactable = true;
    }
}
