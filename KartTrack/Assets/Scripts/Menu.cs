using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Menu : MonoBehaviour
{
    string path;
    private void Awake()
    {
        path = Application.persistentDataPath + "/data";
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void StartTelemetry()
    {
        GPS.instance.telemetry = true;
    }
    public void StopTelemetry()
    {
        GPS.instance.telemetry = false;
    }

    public void save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(path);
        bf.Serialize(file, GPS.instance.accelerationData);
        file.Close();
    }
}
