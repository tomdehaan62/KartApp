using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class GPS : MonoBehaviour
{
    //Output
    //[SerializeField] Text text;
    [SerializeField] private GameObject Gizmo;
    //[SerializeField] private Image gCirc;

   //Instance
    public static GPS instance { set; get; }
    public bool telemetry;
    //Private var
    bool gpsON = false;
    float _eQuatorialEarthRadius = 6378.1370f;
    float _d2r = (Mathf.PI / 180f);
    float lons = 0;
    float lats = 0;
    Vector2 position;
    Vector3 tilt;
    Quaternion up;
    public List<data> accelerationData = new List<data>();

    private void Start()
    {
        //Instance
        instance = this;
        DontDestroyOnLoad(gameObject);
        //Turn on GPS
        StartCoroutine(GetPosition());
        //Android permissions
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
        //Enable Gyroscope
        Input.gyro.enabled = true;
    }

    public struct data
    {
        public Vector2 location2D;
        public Vector3 accleretaion;
        public double timestamp;
    }
    private IEnumerator GetPosition()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled Location");
            yield break;
        }
        //Start getting Location with 20s extra time
        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 20)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        //Timeout, no response
        if (maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }
        //Failed getting location
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to locate device");
            yield break;
        }
        //Succes
        gpsON = true;
        yield break;
    }
    private void FixedUpdate()
    { 
        if (!gpsON)
        {
            return;
        }
        position = CalculatDistanceFromOrigin(Input.location.lastData);
        DrawPoint(position);
        addData();
    }
    void addData()
    {
        data savedata = new data();
        tilt = Input.acceleration;
        up = Input.gyro.attitude;
        tilt = up * tilt;
        savedata.accleretaion = tilt;
        savedata.timestamp = Time.time;
        savedata.location2D = position;
        accelerationData.Add(savedata);
    }
    private void DrawPoint(Vector2 position)
    {
        //Create Point on Screen
        GameObject.Instantiate(Gizmo, new Vector3(position.x + 540, position.y + 1080, 0), Quaternion.identity, GameObject.Find("Cotainer").transform);
        //text.text = (position.x + 540).ToString() + "--- " + (position.y + 1080).ToString();
    }

    /*private void accel()
    {
        //Get acceleration
        tilt = Input.acceleration;
        up = Input.gyro.attitude;
        tilt = up * tilt; //acceleration in G  Get Gyroscope upwards position.
        text.text = tilt.magnitude.ToString();
        gCirc.transform.localPosition = tilt * 30;
    }*/
    

    private Vector2 CalculatDistanceFromOrigin(LocationInfo loc)
    {
        //Initialize Origin
        if (lats == 0)
        {
            if (Input.location.lastData.latitude == 0)
            {
                return new Vector2(0,0);
            }
            lons = Input.location.lastData.longitude;
            lats = Input.location.lastData.latitude;
        }
        //Convert to Radian
        float lat1 = loc.latitude * _d2r;
        float lat2 = lats * _d2r;
        float lon1 = loc.longitude * _d2r;
        float lon2 = lons * _d2r;
        //Calculat delta x and y on plane
        float deltaLat = (lat1 - lat2) * Mathf.Cos((lon2 - lon1) / 2) * _eQuatorialEarthRadius * 3000f;
        float deltaLong = (lon2 - lon1) * _eQuatorialEarthRadius * 3000f;

        return new Vector2(deltaLat, deltaLong);
    }
    

}
