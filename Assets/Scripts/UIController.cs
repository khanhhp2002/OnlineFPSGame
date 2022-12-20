using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }
    public TMP_Text overHeatMessage;
    public Slider weaponHeatSlider;

    public Slider healthPoint;

    public GameObject deathScreen;
    public TMP_Text deathMessage;

    public GameObject playerInfo;

    public GameObject statsScreen;

    public void UpdateStats(List<PlayerInfomation> allPlayerInfo)
    {
        for (int i = 1; i < statsScreen.transform.childCount; i++)
        {
            if (i == 1)
            {
                continue;
            }
            else
            {
                Destroy(statsScreen.transform.GetChild(i).gameObject);
            }
        }
        for (int i = 0; i < allPlayerInfo.Count; i++)
        {
            GameObject newPlayerInfo = Instantiate(playerInfo, playerInfo.transform.parent);
            newPlayerInfo.transform.GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString();
            newPlayerInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = allPlayerInfo[i].name;
            newPlayerInfo.transform.GetChild(2).GetComponent<TMP_Text>().text = allPlayerInfo[i].kills.ToString();
            newPlayerInfo.transform.GetChild(3).GetComponent<TMP_Text>().text = allPlayerInfo[i].deaths.ToString();
            Debug.Log(newPlayerInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = allPlayerInfo[i].name);
        }
    }
}
