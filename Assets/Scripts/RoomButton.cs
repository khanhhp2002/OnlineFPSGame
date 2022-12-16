using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text roomName;

    private RoomInfo roomInfo;

    public void SetButtonDetail(RoomInfo info)
    {
        roomInfo = info;

        roomName.text = roomInfo.Name;
    }

    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(roomInfo);
    }
}
