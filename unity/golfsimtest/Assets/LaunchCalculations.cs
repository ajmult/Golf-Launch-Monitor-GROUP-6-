using UnityEngine;

public class LaunchCalculations : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    public float getHSpeed(float initSpeed, float vAngle, float hAngle)
    {
        return initSpeed * Mathf.Cos(Mathf.Deg2Rad * vAngle) * Mathf.Cos(Mathf.Deg2Rad * hAngle);
    }
    
    public float getSideSpeed(float initSpeed, float vAngle)
    {
        return initSpeed * Mathf.Cos(Mathf.Deg2Rad * vAngle);
    }
    
    public float getInitVSpeed(float initSpeed, float vAngle)
    {
        return initSpeed * Mathf.Sin(Mathf.Deg2Rad * vAngle);
    }
    
    public float getVSpeed(float launchTime, float initVSpeed)
    {
        return initVSpeed + (-9.81f * launchTime);
    }

    public float getHPos(float initSpeed, float launchTime)
    {
        return (initSpeed * launchTime);
    }
    
    public float getVPos(float initSpeed, float launchTime)
    {
        return (initSpeed * launchTime) + ((-9.81f/2)*Mathf.Pow(launchTime, 2));
    }

    public float getMaxHeight(float initYSpeed)
    {
        return ((Mathf.Pow(initYSpeed,2))/(2*9.81f));
    }

    public float getRange(float xSpeed, float initYSpeed)
    {
        return xSpeed * 2 * (initYSpeed / 9.81f);
    }

    public float getFlightTime(float xSpeed, float range)
    {
        return range/xSpeed;
    }


    public float vSpeedBounce(float vspeed, float restitution)
    {
        return (vspeed*restitution);
    }
    
    public float hSpeedBounce(float hspeed, float vspeed, float restitution, float friction)
    {
        return hspeed - (friction*(1+restitution)*vspeed);
    }

    public float rollDistance(float initSpeed, float friction)
    {
        return (Mathf.Pow(initSpeed, 2))/(2*friction*9.81f);
    }

    public float rollDisplacement(float initSpeed, float friction, float time)
    {
        if (rollSpeed(initSpeed, friction, time) < 0)
        {
            return (initSpeed*(initSpeed/(friction*9.81f)) - (((friction/2)*9.81f)*Mathf.Pow(initSpeed/(friction*9.81f), 2)));
        }
        else
        {
            return (initSpeed*time) - (((friction/2)*9.81f)*Mathf.Pow(time, 2));
        }
        
    }

    public float rollSpeed(float initSpeed, float friction, float time)
    {
        return initSpeed - (friction*9.81f)*time;
    }
}
