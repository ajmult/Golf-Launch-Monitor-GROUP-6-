using TMPro;
using UnityEngine;

public class UIStats : MonoBehaviour
{

    public GameObject leftArrow;
    public GameObject rightArrow;
    
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI vAngleText;
    public TextMeshProUGUI hAngleText;

    public LaunchSim stats;
    public LaunchCalculations calc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distanceText.SetText(
            Mathf.Round((calc.getRange(calc.getSideSpeed(stats.speed, stats.verticalAngle), calc.getInitVSpeed(stats.speed, stats.verticalAngle)))*10)/10.0
            +"m");
        heightText.SetText(
            Mathf.Round((calc.getMaxHeight(calc.getInitVSpeed(stats.speed, stats.verticalAngle)))*10)/10.0
                        +"m");
        speedText.SetText(stats.speed+"m/s");
        vAngleText.SetText(stats.verticalAngle+"°");
        hAngleText.SetText(Mathf.Abs(stats.horizontalAngle)+"°");
        
        if (stats.horizontalAngle > 0)
        {
            rightArrow.SetActive(true);
            leftArrow.SetActive(false);
        }
        else if (stats.horizontalAngle < 0)
        {
            rightArrow.SetActive(false);
            leftArrow.SetActive(true);
        }
        else
        {
            rightArrow.SetActive(false);
            leftArrow.SetActive(false);
        }
    }
}
