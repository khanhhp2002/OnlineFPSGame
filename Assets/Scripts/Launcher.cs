using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    public GameObject loadingScreen;
    public TMP_Text loadingText;

    public GameObject menuButtons;

    public GameObject createRoomScreen;
    public TMP_InputField roomName;

    public GameObject roomScreen;
    public TMP_Text roomNameText;

    public GameObject errorScreen;
    public TMP_Text errorText;

    public GameObject roomListScreen;
    public RoomButton roomButton;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();
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
    }

    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();
        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        menuButtons.SetActive(true);
        Invoke("CloseLoadingScreen", 0.5f);
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
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        Invoke("CloseLoadingScreen", 0.5f);
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
        SceneManager.LoadScene("Map 1");
    }
}
