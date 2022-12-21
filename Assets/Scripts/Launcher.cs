using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    public GameObject loadingScreen;
    public TMP_Text loadingText;

    public GameObject menuButtons;

    public GameObject createRoomScreen;
    public TMP_InputField roomName;

    public GameObject roomScreen;
    public TMP_Text roomNameDisplay;
    public TMP_Text playerName;
    private List<TMP_Text> allPlayerName = new List<TMP_Text>();

    public GameObject errorScreen;
    public TMP_Text errorText;

    public GameObject roomListScreen;
    public RoomButton roomButton;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();

    public GameObject nameInputScreen;
    public TMP_InputField userName;
    public static bool hasNickName;
    public GameObject startButton;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        PhotonNetwork.ConnectUsingSettings();
#if UNITY_EDITOR
        //UIController.instance.
#endif
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        menuButtons.SetActive(true);
        Invoke("CloseLoadingScreen", 0.5f);
        //PhotonNetwork.NickName = Random.Range(0, 1000).ToString();
        if (!hasNickName)
        {
            nameInputScreen.SetActive(true);
            if (PlayerPrefs.HasKey("playerName"))
            {
                userName.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(userName.text))
        {
            PhotonNetwork.NickName = userName.text;
            PlayerPrefs.SetString("playerName", userName.text);
            hasNickName = true;
            nameInputScreen.SetActive(false);
        }
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomName.text))
        {
            RoomOptions option = new RoomOptions();
            option.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomName.text, option);

            loadingText.text = "Creating Room..";
            loadingScreen.SetActive(true);
        }
        roomName.text = null;
    }

    public override void OnJoinedRoom()
    {
        roomScreen.SetActive(true);
        roomNameDisplay.text = PhotonNetwork.CurrentRoom.Name;
        Invoke("CloseLoadingScreen", 0.5f);
        ListAllPlayer();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            startButton.GetComponent<Button>().interactable = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        TMP_Text newPlayerName = Instantiate(playerName, playerName.transform.parent);
        newPlayerName.text = newPlayer.NickName;
        newPlayerName.gameObject.SetActive(true);
        allPlayerName.Add(newPlayerName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayer();
    }

    private void ListAllPlayer()
    {
        foreach (TMP_Text player in allPlayerName)
        {
            Destroy(player.gameObject);
        }
        allPlayerName.Clear();
        Player[] players = PhotonNetwork.PlayerList;
        playerName.gameObject.SetActive(false);
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerName = Instantiate(playerName, playerName.transform.parent);
            newPlayerName.text = players[i].NickName;
            newPlayerName.gameObject.SetActive(true);
            allPlayerName.Add(newPlayerName);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed To Create Room: " + message;
        errorScreen.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        loadingText.text = "Leaving...";
    }

    private void CloseLoadingScreen()
    {
        loadingScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public override void OnLeftRoom()
    {
        Invoke("CloseLoadingScreen", 0.5f);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();
        roomButton.gameObject.SetActive(false);
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(roomButton, roomButton.transform.parent);
                newButton.SetButtonDetail(roomList[i]);
                newButton.gameObject.SetActive(true);
                allRoomButtons.Add(newButton);
            }
        }
    }

    public void JoinRoom(RoomInfo roomInfo)
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
        loadingText.text = "Joining room " + roomInfo.Name;
    }

    public void Play()
    {
#if UNITY_EDITOR
        PhotonNetwork.CreateRoom("test");
        loadingText.text = "Creating room";
        Invoke("CloseLoadingScreen", 0.5f);
#endif
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel("Map 2");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            startButton.GetComponent<Button>().interactable = false;
        }
    }
}
