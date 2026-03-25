using System.IO;
using UnityEngine;

public class jsonTest : MonoBehaviour
{

    public float speed = 13;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SavePlant("testjson.json");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SavePlant(string filename)
    {
        string jsonData = JsonUtility.ToJson(this);
        File.WriteAllText(filename, jsonData);
    }
}
