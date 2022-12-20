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
        UpdateStats
    }

    public List<PlayerInfomation> allPlayers = new List<PlayerInfomation>();
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    // Update is called once per frame
    void Update()
    {

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
        object[] package = new object[allPlayers.Count];
        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i] = piece;
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
        for (int i = 0; i < data.Length; i++)
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
                index = i;
            }
        }
        UIController.instance.UpdateStats(allPlayers);
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
        UIController.instance.UpdateStats(allPlayers);
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
