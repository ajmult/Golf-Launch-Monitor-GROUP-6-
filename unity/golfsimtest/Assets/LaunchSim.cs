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
    private BallData shotData;

    public float range = 0;
    public float height = 0;
    private bool heightGotten = false;

    public GameObject marker;

    private float checkSum = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        calc = gameObject.GetComponent<LaunchCalculations>();
        
        initSpeed = speed;
        vAngle = verticalAngle;
        hAngle = horizontalAngle;
        StartCoroutine(spawnMarker());
        currentXSpeed = calc.getHSpeed(initSpeed, vAngle, hAngle);
        currentYSpeed = calc.getVSpeed(time, calc.getInitVSpeed(initSpeed, vAngle));
        currentZSpeed = calc.getHSpeed(initSpeed, vAngle, 90-hAngle);
        shotData = LoadShot("shotdata.json");
        checkSum = shotData.pos2[0] + shotData.pos2[1]+shotData.frameCount;
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

                currentXSpeed -= (calc.getAirResAccel(currentXSpeed))* Time.deltaTime;
                currentYSpeed -= (calc.getAirResAccel(currentYSpeed) + 9.81f)* Time.deltaTime;
                currentZSpeed -= (calc.getAirResAccel(currentZSpeed))* Time.deltaTime;

                if (currentXSpeed < 0)
                {
                    currentXSpeed = 0;
                }
                if (currentZSpeed < 0)
                {
                    currentZSpeed = 0;
                }

                if (currentYSpeed < 0 && !heightGotten)
                {
                    heightGotten = true;
                    height = transform.position.y;
                }
                if (transform.position.y <= 0)
                {
                    transform.position = new Vector3(
                        transform.position.x, 
                        0, 
                        transform.position.z);
                    range = Mathf.Sqrt((transform.position.x * transform.position.x) + (transform.position.z * transform.position.z));
                    moving = false;
                    StartCoroutine(RestartAnimation());
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
        
        shotData = LoadShot("shotdata.json");
        if (checkSum > shotData.pos2[0] + shotData.pos2[1]+shotData.frameCount+0.01f || checkSum < shotData.pos2[0] + shotData.pos2[1]+shotData.frameCount-0.01f)
        {
            checkSum = shotData.pos2[0] + shotData.pos2[1];
            Imgs2Data(shotData);
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

    public void Imgs2Data(BallData data)
    {
        float x;
        float y;
        float z;
        float imgTime = (1 / data.frameRate) * data.frameCount;
        //remember to replace parameters with a known value once we get the cameras
        float focalLength = (calc.getFocalLength(0, 0, 0.046f));
        
        x = ((Math.Abs(data.pos2[0]-data.pos1[0]))/data.size1)*0.046f;
        y = ((data.pos2[1]-data.pos1[1])/data.size1)*0.046f;
        z = (calc.getDistanceDromPixelSize(data.size2, focalLength, 0.046f) - calc.getDistanceDromPixelSize(data.size1, focalLength, 0.046f));

        initSpeed = (Mathf.Sqrt(Mathf.Pow(Mathf.Sqrt(Mathf.Pow(x, 2)+Mathf.Pow(z, 2)), 2)+Mathf.Pow(y, 2)))/imgTime;
        hAngle = Mathf.Atan(y/x) * Mathf.Rad2Deg;
        vAngle = Mathf.Atan(z/x) * Mathf.Rad2Deg;

    }

    public class BallData
    {
        public float[] pos1;
        public float[] pos2;
        public float frameCount;
        public float frameRate;
        public float size1;
        public float size2;

    }
}
