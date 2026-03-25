using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

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
    
    private float time =  0.0f;
    
    private float lastX = 0;
    
    private float lastZ = 0;
    
    private float newVSpeed;
    private float newXSpeed;
    
    private bool rolling = false;
    private bool moving = true;
    
    private LaunchCalculations calc;
    private BallData shotData;

    public GameObject marker;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        calc = gameObject.GetComponent<LaunchCalculations>();
        
        initSpeed = speed;
        vAngle = verticalAngle;
        hAngle = horizontalAngle;
        StartCoroutine(spawnMarker());
        shotData = LoadShot("shotdata.json");
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (moving)
        {
                transform.position = new Vector3(
                    lastX + calc.getHPos(calc.getHSpeed(initSpeed, vAngle, hAngle), time), 
                    calc.getVPos(calc.getVSpeed(time, calc.getInitVSpeed(initSpeed, vAngle)), time), 
                    lastZ - calc.getHPos(calc.getHSpeed(initSpeed, vAngle, 90-hAngle), time));
                if (transform.position.y <= 0)
                {
                    transform.position = new Vector3(
                        lastX + calc.getHPos(calc.getHSpeed(initSpeed, vAngle, hAngle), time), 
                        0, 
                        lastZ - calc.getHPos(calc.getHSpeed(initSpeed, vAngle, 90-hAngle), time));
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
        lastX = 0;
        lastZ = 0;
        rolling = false;
        moving = true;
        shotData = LoadShot("shotdata.json");
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Marker");
        foreach(GameObject go in gos)
            Destroy(go);
    }
    
    IEnumerator spawnMarker()
    {
        yield return new WaitForSeconds(0.1f);
        Instantiate(marker, transform.position, transform.rotation);
        StartCoroutine(spawnMarker());
    }
    
    public static BallData LoadShot(string filename)
    {
        string fileData = File.ReadAllText(filename);
        BallData loadedData = JsonUtility.FromJson<BallData>(fileData);
        return loadedData;
    }

    public class BallData
    {
        public float[] pos1;
        public float[] pos2;
        public float speed;
        public float size1;
        public float size2;

    }
}
