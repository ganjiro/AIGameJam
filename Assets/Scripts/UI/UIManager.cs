using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image _batteryLine;
    private float startingBatteryWidth;
    public Image _madnessLine;
    private float startingMadWidth;

    public TextMeshProUGUI _enemyText;

    [Range(0, 100)] public int batteryLeft;
    [Range(0, 100)] public int madnessValue;

    // Start is called before the first frame update
    void Start()
    {
        startingBatteryWidth = _batteryLine.rectTransform.sizeDelta.x;
        startingMadWidth = _madnessLine.rectTransform.sizeDelta.x;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerController player = Regenerate.instance.player.GetComponent<PlayerController>();
        // Compute battery life
        float batteryPerc = (float)(player.maxRound - player.actualRound) / (float)player.maxRound;
        _batteryLine.rectTransform.sizeDelta = new Vector2(startingBatteryWidth*batteryPerc, _batteryLine.rectTransform.sizeDelta.y );
        float madnessPerc = (float)(GlobalBlackboard.instance.madnessValue) / (float)GlobalBlackboard.instance.maxMadnessValue;
        _madnessLine.rectTransform.sizeDelta = new Vector2(startingMadWidth*madnessPerc,_madnessLine.rectTransform.sizeDelta.y );

        _enemyText.text = "" + player.getNumberAliveEnemies();
    }
}
