using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class LaunchSim : MonoBehaviour
{
    public float speed;

    public float verticalAngle;
    
    public float horizontalAngle;
    
    public float restitution;
    
    public float friction;
    
    private float initSpeed;

    private float vAngle;
    
    private float hAngle;
    
    private float currentXSpeed;
    private float currentYSpeed;
    private float currentZSpeed;
    
    private float time =  0.0f;
    
    private bool moving = true;
    
    private LaunchCalculations calc;
    private BallData cam1Data;
    private BallData cam2Data;

    public float range = 0;
    public float height = 0;
    private bool heightGotten = false;

    public GameObject marker;

    private float checkSum = 0;

    public float referencePixelSize;
    public float referenceDistance;
    public bool twoCamera;

    public bool testMode;
    private bool airborne;

    private bool upwards;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        calc = gameObject.GetComponent<LaunchCalculations>();
        
        initSpeed = speed;
        vAngle = verticalAngle;
        hAngle = horizontalAngle;
        airborne = true;
        StartCoroutine(spawnMarker());
        currentXSpeed = calc.getHSpeed(initSpeed, vAngle, hAngle);
        currentYSpeed = calc.getVSpeed(time, calc.getInitVSpeed(initSpeed, vAngle));
        currentZSpeed = calc.getHSpeed(initSpeed, vAngle, 90-hAngle);
        if (!testMode)
        {
            cam1Data = LoadShot("cam1data.json");
            cam2Data = LoadShot("cam2data.json");
            checkSum = cam1Data.pos2[0] + cam1Data.pos2[1]+cam1Data.frameCount;
        }
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (moving)
        {
                transform.position = new Vector3(
                    transform.position.x + (currentXSpeed * Time.deltaTime), 
                    transform.position.y + (currentYSpeed * Time.deltaTime), 
                    transform.position.z + (currentZSpeed * Time.deltaTime));
                upwards = currentYSpeed > 0;
                if (upwards)
                {
                    airborne = true;
                }
                if (airborne)
                {
                    currentXSpeed -= (calc.getAirResAccel(currentXSpeed))* Time.deltaTime;
                    currentYSpeed -= (calc.getAirResAccel(currentYSpeed) + 9.81f)* Time.deltaTime;
                    currentZSpeed -= (calc.getAirResAccel(currentZSpeed))* Time.deltaTime;
                }
                else
                {
                    currentXSpeed -= (calc.getFrictionAccel(friction))* Time.deltaTime;
                    currentYSpeed -= (calc.getAirResAccel(currentYSpeed) + 9.81f)* Time.deltaTime;
                    currentZSpeed -= (calc.getFrictionAccel(friction))* Time.deltaTime;
                }

                

                if (currentXSpeed < 0)
                {
                    currentXSpeed = 0;
                }
                if (currentZSpeed < 0)
                {
                    currentZSpeed = 0;
                }

                if (currentYSpeed > 0 && !heightGotten)
                {
                    heightGotten = true;
                    height = transform.position.y;
                   
                }
                if (transform.position.y <= 0)
                {
                    if ((-currentYSpeed * restitution) > 0.01f && !upwards)
                    {
                        currentYSpeed = -currentYSpeed * restitution;
                    }
                    else
                    {
                        transform.position = new Vector3(
                            transform.position.x, 
                            0, 
                            transform.position.z);
                        airborne = false;
                        if (currentXSpeed == 0 && currentZSpeed == 0)
                        {
                            range = Mathf.Sqrt((transform.position.x * transform.position.x) + (transform.position.z * transform.position.z));
                            moving = false;
                            StartCoroutine(RestartAnimation());
                        }
                    }
                    
                   
                }
        }
    }
    
    IEnumerator RestartAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        initSpeed = speed;
        vAngle = verticalAngle;
        hAngle = horizontalAngle;
        time = 0.0f;
        currentXSpeed = calc.getHSpeed(initSpeed, vAngle, hAngle);
        currentYSpeed = calc.getVSpeed(time, calc.getInitVSpeed(initSpeed, vAngle));
        currentZSpeed = calc.getHSpeed(initSpeed, vAngle, 90-hAngle);
        transform.position = new Vector3(0, 0, 0);
        moving = true;
        airborne = true;
        upwards = true;
        if (!testMode)
        {
            cam1Data = LoadShot("cam1data.json");
            cam2Data = LoadShot("cam2data.json");
            if (checkSum > cam1Data.pos2[0] + cam1Data.pos2[1]+cam1Data.frameCount+0.01f || checkSum < cam1Data.pos2[0] + cam1Data.pos2[1]+cam1Data.frameCount-0.01f)
            {
                checkSum = cam1Data.pos2[0] + cam1Data.pos2[1];
                Imgs2Data(cam1Data, cam2Data, twoCamera);
            }
        }
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Marker");
        foreach(GameObject go in gos)
            Destroy(go);
    }
    
    IEnumerator spawnMarker()
    {
        yield return new WaitForSeconds(0.2f);
        Instantiate(marker, transform.position, transform.rotation);
        StartCoroutine(spawnMarker());
    }
    
    public static BallData LoadShot(string filename)
    {
        string fileData = File.ReadAllText(filename);
        BallData loadedData = JsonUtility.FromJson<BallData>(fileData);
        return loadedData;
    }

    public void Imgs2Data(BallData data1, BallData data2, bool twoCameras)
    {
        float x;
        float y;
        float z;
        float imgTime = (1 / data1.frameRate) * data1.frameCount;
        //remember to replace parameters with a known value once we get the cameras
        float focalLength = (calc.getFocalLength(referencePixelSize, referenceDistance, 0.046f));
        
        x = ((Math.Abs(data1.pos2[0]-data1.pos1[0]))/data1.size1)*0.046f;
        y = ((data1.pos2[1]-data1.pos1[1])/data1.size1)*0.046f;
        z = (calc.getDistanceFromPixelSize(data1.size2, focalLength, 0.046f) - calc.getDistanceFromPixelSize(data1.size1, focalLength, 0.046f));

        initSpeed = (Mathf.Sqrt(Mathf.Pow(Mathf.Sqrt(Mathf.Pow(x, 2)+Mathf.Pow(z, 2)), 2)+Mathf.Pow(y, 2)))/imgTime;
        hAngle = Mathf.Atan(y/x) * Mathf.Rad2Deg-data1.angle;
        vAngle = Mathf.Atan(z/x) * Mathf.Rad2Deg;

        if (twoCameras)
        {
            x = ((Math.Abs(data2.pos2[0]-data2.pos1[0]))/data2.size1)*0.046f;
            y = ((data2.pos2[1]-data2.pos1[1])/data2.size1)*0.046f;
            z = (calc.getDistanceFromPixelSize(data2.size2, focalLength, 0.046f) - calc.getDistanceFromPixelSize(data2.size1, focalLength, 0.046f));

            initSpeed += (Mathf.Sqrt(Mathf.Pow(Mathf.Sqrt(Mathf.Pow(x, 2)+Mathf.Pow(z, 2)), 2)+Mathf.Pow(y, 2)))/imgTime;
            hAngle += Mathf.Atan(y/x) * Mathf.Rad2Deg-data2.angle;
            vAngle += Mathf.Atan(z/x) * Mathf.Rad2Deg;

            initSpeed /= 2;
            hAngle /= 2;
            vAngle /= 2;
        }
        

    }

    public class BallData
    {
        public float[] pos1;
        public float[] pos2;
        public float frameCount;
        public float frameRate;
        public float size1;
        public float size2;
        public float angle;

    }
}
