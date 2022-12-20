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
}
