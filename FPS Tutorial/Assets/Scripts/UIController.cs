using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
// This class is a singletone responsible of giving access to the ui
public class UIController : MonoBehaviour
{
    public static UIController instance;
    // Start is called every time a object is activated before start
    private void Awake()
    {
        instance = this;
    }

    public TMP_Text overheatedMessage;
    public Slider weaponTempSlider;

    public Slider healthSlider;

    public GameObject deathScrean;
    public TMP_Text deathText;

    public TMP_Text killsCountText;
    public TMP_Text deathsCountText;

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}
