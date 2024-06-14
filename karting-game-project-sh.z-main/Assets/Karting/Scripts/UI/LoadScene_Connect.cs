using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScene_Connect : MonoBehaviour
{
    public FTMS_IndoorBike indoorBike { get; private set; }
    public GameObject FTMS_IndoorBike_GO; 
    public Text info;
    //public string device_name = "KICKR BIKE 65E9";
    //public string service_id = "{00001826-0000-1000-8000-00805f9b34fb}";
    //public string read_characteristic = "{00002ad2-0000-1000-8000-00805f9b34fb}";
    //public string write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}";
    public string device_name = "";
    public string service_id = "";
    public string read_characteristic = "";
    public string service_id2 = "";
    public string read_characteristic2 = "";
    public string write_characteristic = "";
    //private bool connected = false;

    // Start is called before the first frame update
    void Start()
    {
        indoorBike = FTMS_IndoorBike_GO.GetComponent<FTMS_IndoorBike>();

        connect();

    }

    // Update is called once per frame
    void Update()
    {
        if (indoorBike.isSubscribed)
        {
            info.text = "Indoor bike connects successfully" + "\n";
        }
    }

    public void connect()
    {
        if (device_name.Length > 0 && service_id.Length > 0 && read_characteristic.Length > 0 && write_characteristic.Length > 0)
        {
            //StartCoroutine(indoorBike.connect(device_name, service_id, read_characteristic, write_characteristic));
            StartCoroutine(indoorBike.connect(device_name, service_id, service_id2, read_characteristic, read_characteristic2, write_characteristic));
            //connected = true;
        }
    }
}
