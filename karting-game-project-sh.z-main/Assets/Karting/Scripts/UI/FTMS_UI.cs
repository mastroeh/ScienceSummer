using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FTMS_UI : MonoBehaviour
{
    // Start is called before the first frame update
    public static FTMS_UI Instance; // 单例实例
    //private bool connected = false;
    public FTMS_IndoorBike connector;
    public GameObject IndoorBike_GO;
    public Text info;

    //public Text resistance_show;
    //public Text grade_show;

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


    void Awake()
    {
        // Implementation of singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //It is not destroyed when loading a new scene
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject); // If an instance already exists, destroy the new object
                return;
            }
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject); // Prevent objects from being destroyed when loading a new scene
        //connector = IndoorBike_GO.GetComponent<FTMS_IndoorBike>();
        connector = FTMS_IndoorBike.Instance;
        connect();
    }

    public void connect() {
        if (device_name.Length > 0 && service_id.Length > 0 && read_characteristic.Length > 0 && write_characteristic.Length > 0)
        {
            //StartCoroutine(connector.connect(device_name, service_id, read_characteristic, write_characteristic));
            StartCoroutine(connector.connect(device_name, service_id, service_id2, read_characteristic, read_characteristic2, write_characteristic));
            //connected = true;
            UnityEngine.Debug.Log("Automatic Connect is called.");
        }
    }

    /*public void write_resistance(float val) {
        if (connected)
        {
            connector.write_resistance(val);

            //resistance_show.text = "Resistance: " + val / 10000;
        }
    }

	/*public void write_grade(float val) {
        if (connected)
        {
            val = val * 5;
            connector.write_grade(Mathf.FloorToInt(val));

            //grade_show.text = "Grade: " + Mathf.FloorToInt(val).ToString();
        }
    }*/

    // Update is called once per frame
    void Update()
    {
        //UpdateUIWithOutput(connector.output);
        info.text = connector.output;
    }

    private void OnApplicationQuit()
    {
        connector.quit();
    }

    public void change_device_name(string _device_name)
        {
            device_name = _device_name;
        }
    public void change_service_id(string _service_id)
        {
            service_id = _service_id;
        }
    public void change_read_characteristic(string _read_characteristic)
        {
            read_characteristic = _read_characteristic;
        }
    public void change_write_characteristic(string _write_characteristic)
        {
            write_characteristic = _write_characteristic;
        }

}

