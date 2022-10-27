using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Image _batteryLine;
    private float startingBatteryWidth;
    public Image _madnessLine;
    private float startingMadWidth;
    private Light2D _globalLight;
    private float startingLight;

    public TextMeshProUGUI _enemyText;
    public List<Button> _actionButtons;

    public static UIManager instance;


    [Range(0, 100)] public int batteryLeft;
    [Range(0, 100)] public int madnessValue;

    // Start is called before the first frame update
    void Start()
    {
        startingBatteryWidth = _batteryLine.rectTransform.sizeDelta.x;
        startingMadWidth = _madnessLine.rectTransform.sizeDelta.x;
        _globalLight = GameObject.Find("Global Light").GetComponent<Light2D>();
        startingLight = _globalLight.intensity;
    }

    public void LinkButtons()
    {
        PlayerController player = Regenerate.instance.player.GetComponent<PlayerController>();
        if(player != null)
        {
            foreach(Button b in gameObject.GetComponentsInChildren<Button>())
            {
                b.onClick.AddListener(delegate {player.GUIMove(b.GetComponent<ActionButton>().action); });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayerController player = Regenerate.instance.player.GetComponent<PlayerController>();
        // Compute battery life
        float batteryPerc = (float)(player.maxRound - player.actualRound) / (float)player.maxRound;
        _batteryLine.rectTransform.sizeDelta = new Vector2(startingBatteryWidth*batteryPerc, _batteryLine.rectTransform.sizeDelta.y );
        
        _madnessLine.rectTransform.sizeDelta = new Vector2(startingMadWidth*GlobalBlackboard.instance.GetMadnessPerc(),_madnessLine.rectTransform.sizeDelta.y );
        _globalLight.intensity = startingLight * (1 - GlobalBlackboard.instance.GetMadnessPerc());
        _enemyText.text = "" + player.getNumberAliveEnemies();
        
        // Change light
    }
}
