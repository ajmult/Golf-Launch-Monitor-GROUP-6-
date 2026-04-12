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
    
    public float getInitVSpeed(float initSpeed, float vAngle)
    {
        return initSpeed * Mathf.Sin(Mathf.Deg2Rad * vAngle);
    }
    
    public float getVSpeed(float launchTime, float initVSpeed)
    {
        return initVSpeed + (-9.81f * launchTime);
    }

    public float getAirResAccel(float velocity)
    {
        return (0.5f*1.293f*(Mathf.Pow(velocity, 2)*0.001662f*0.47f))/0.046f;
    }
    
    public float getFocalLength(float pixelSize, float knownDistance, float ballSize)
    {
        return (pixelSize * knownDistance) / ballSize;
    }
    
    public float getDistanceDromPixelSize(float pixelSize, float focalLength, float ballSize)
    {
        return (ballSize * focalLength) / pixelSize;
    }
}
