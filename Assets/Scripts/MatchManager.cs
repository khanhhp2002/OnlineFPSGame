using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }

    public enum EvenCodes : byte
    {
        NewPlayer,
        ListPlayer,
        UpdateStats,
        NextMatch,
        TimeSync
    }

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public GameState state = GameState.Waiting;
    public float autoJoinTime = 5f;

    public float matchLength = 300f;
    private float currentMatchTime;
    private float sendTime;

    public List<PlayerInfomation> allPlayers = new List<PlayerInfomation>();
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        UIController.instance.leaveButton.SetActive(false);
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            state = GameState.Playing;
            SetupTimer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentMatchTime > 0f && state == GameState.Playing)
            {
                currentMatchTime -= Time.deltaTime;
                if (currentMatchTime <= 0f)
                {
                    currentMatchTime = 0f;
                    state = GameState.Ending;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        ListPlayerSend();
                        StateCheck();
                    }
                }
                UpdateTime();
                sendTime -= Time.deltaTime;
                if (sendTime <= 0)
                {
                    sendTime += 1f;
                    TimerSend();
                }
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EvenCodes events = (EvenCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            Debug.Log("Event: " + events);

            switch (events)
            {
                case EvenCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EvenCodes.ListPlayer:
                    ListPlayerReceive(data);
                    break;
                case EvenCodes.UpdateStats:
                    UpdateStatsReceive(data);
                    break;
                case EvenCodes.NextMatch:
                    NextMatchReceive();
                    break;
                case EvenCodes.TimeSync:
                    TimeReceive(data);
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string username)
    {
        // creating data;
        object[] package = new object[4];

        package[0] = username; //player name
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;// index
        package[2] = 0; //kills
        package[3] = 0; //deaths
        // sending data:
        PhotonNetwork.RaiseEvent(
            (byte)EvenCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerReceive(object[] data)
    {
        PlayerInfomation playerInfo = new PlayerInfomation((string)data[0], (int)data[1], (int)data[2], (int)data[3]);
        allPlayers.Add(playerInfo);

        ListPlayerSend();
    }

    public void ListPlayerSend()
    {
        object[] package = new object[allPlayers.Count + 1];
        package[0] = state;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i + 1] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EvenCodes.ListPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ListPlayerReceive(object[] data)
    {
        allPlayers.Clear();
        state = (GameState)data[0];
        for (int i = 1; i < data.Length; i++)
        {
            object[] piece = (object[])data[i];
            PlayerInfomation playerInfo = new PlayerInfomation(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );
            allPlayers.Add(playerInfo);

            if (PhotonNetwork.LocalPlayer.ActorNumber == playerInfo.actor)
            {
                index = i - 1;
            }
        }
        UIController.instance.UpdateStats(SortPlayers(allPlayers));
        StateCheck();
    }
    public void UpdateStatsSend(int actorSending, bool statToUpdate, int amountToChange)// statToUpdate: true = kill / false = death
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
           (byte)EvenCodes.UpdateStats,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
           );
    }

    public void UpdateStatsReceive(object[] data)
    {
        int actor = (int)data[0];
        bool stats = (bool)data[1];
        int amount = (int)data[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                if (stats)
                {
                    allPlayers[i].kills += amount;
                }
                else
                {
                    allPlayers[i].deaths += amount;
                }
                break;
            }
        }
        UIController.instance.UpdateStats(SortPlayers(allPlayers));
    }

    private List<PlayerInfomation> SortPlayers(List<PlayerInfomation> allPlayers)
    {
        List<PlayerInfomation> sorted = new List<PlayerInfomation>();
        while (sorted.Count < allPlayers.Count)
        {
            int highest = -1;
            PlayerInfomation selection = allPlayers[0];
            foreach (PlayerInfomation player in allPlayers)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kills > highest)
                    {
                        selection = player;
                        highest = player.kills;
                    }
                }
            }

            sorted.Add(selection);
        }
        return sorted;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }

    void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        state = GameState.Ending;
        UIController.instance.deathScreen.SetActive(false);
        UIController.instance.statsScreen.SetActive(true);
        UIController.instance.leaveButton.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        StartCoroutine(EndCo());
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(autoJoinTime);
        if (PhotonNetwork.IsMasterClient)
        {
            NextMatchSend();
        }

    }

    public void LeaveRoom()
    {
        if (state == GameState.Ending)
        {
            StopAllCoroutines();
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
    }
    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
           (byte)EvenCodes.NextMatch,
           null,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
           );
    }
    public void NextMatchReceive()
    {
        state = GameState.Playing;
        UIController.instance.statsScreen.SetActive(false);
        UIController.instance.leaveButton.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        foreach (PlayerInfomation player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }
        UIController.instance.UpdateStats(allPlayers);
        PlayerSpawner.Instance.SpawnPlayer();
        SetupTimer();
    }

    public void SetupTimer()
    {
        if (matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTime();
        }
    }

    public void UpdateTime()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);
        UIController.instance.timer.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
    }

    public void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };
        PhotonNetwork.RaiseEvent(
           (byte)EvenCodes.TimeSync,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
           );
    }

    public void TimeReceive(object[] data)
    {
        currentMatchTime = (int)data[0];
        state = (GameState)data[1];
        UpdateTime();
    }
}
[System.Serializable]
public class PlayerInfomation
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfomation()
    {

    }

    public PlayerInfomation(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}
