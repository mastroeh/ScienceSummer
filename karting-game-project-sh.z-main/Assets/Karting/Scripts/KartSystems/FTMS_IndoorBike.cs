using System;
using System.Collections;
using System.Collections.Generic;

//using System.Diagnostics;
using UnityEngine;

public class FTMS_IndoorBike: MonoBehaviour
{
    public static FTMS_IndoorBike Instance;
    
    string device_name;
    string service_id;
    //Define attributes for speed subscription
    public string read_characteristic;
    string write_characteristic;
    
    //Define attributes for steering subscription
    string service_id2;
    string read_characteristic2;

    public bool want_connect = true;
    Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();
    public string selectedDeviceId = "";
    string selectedServiceId = "{00001826-0000-1000-8000-00805f9b34fb}";
    string selectedCharacteristicId = "{00002ad2-0000-1000-8000-00805f9b34fb}";

    //Initialize steering Ids
    string selectedServiceId2 = "{a026ee0d-0a7d-4ab3-97fa-f1500f9feb8b}";
    string selectedCharacteristicId2 = "{a026e03c-0a7d-4ab3-97fa-f1500f9feb8b}";

    public bool isSubscribed;

    public string output;
    public float speed; public bool has_speed = false;
    public float average_speed; public bool has_average_speed = false;
    public float rpm; public bool has_rpm = false;
    public float average_rpm; public bool has_average_rpm = false;
    public float distance; public bool has_distance = false;
    public float resistance; public bool has_resistance = false;
    public float power; public bool has_power = false;
    public float average_power; public bool has_average_power = false;
    public float expended_energy; public bool has_expended_energy = false;
    private bool leftButtonPressed = false;
    private bool rightButtonPressed = false;

    //Variables to store the last reasonable value
    private float lastReasonableRPM = 0;
    private float lastReasonableAvg_RPM = 0;
    private float lastReasonableSpeed = 0;
    private float lastReasonableAvg_Speed = 0;
    private float lastReasonableDistance = 0;
    private float lastReasonableResistance = 0;
    private float lastReasonablePower = 0;
    private float lastReasonableAvg_Power = 0;
    private float lastReasonableEnergy = 0;

    // Define reasonable maximum RPM values and distance values
    private float reasonableMaxRPM = 200;
    private float distancePerMinute = 60; // Distance gained per minute
    private float reasonableMaxAvg_RPM = 200;
    private float reasonableMaxSpeed = 200;
    private float reasonableMaxAvg_Speed = 200;
    private float reasonableMaxResistance = 50;
    private float reasonableMaxPower = 1000;
    private float reasonableMaxAvg_Power = 1000;
    private float reasonableMaxEnergy = 200;


    private byte[] grade_bytes = {0x00, 0x00};
    private byte resistance_byte =  0x00;
    private byte[] wind_speed_byte = {0x00, 0x00};

    string lastError;
    float last_write_time = 0.0f;

    //Attributes for accessing the steering buttons
    public bool is_left_clicked = false;
    public int left_button_id = 32;
    public bool is_right_clicked = false;
    public int right_button_id = 2048;

    MonoBehaviour mono;
    // FTMS_IndoorBike(MonoBehaviour _mono)
    //{
    //    mono = _mono;
    //} 

    // Start is called before the first frame update
    // public IEnumerator connect(string _device_name = "KICKR BIKE 65E9", string _service_id = "{a026ee0d-0a7d-4ab3-97fa-f1500f9feb8b}", string _read_characteristic = "{00002ad2-0000-1000-8000-00805f9b34fb}", string _write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}")
    public IEnumerator connect(string _device_name, string _service_id, string _service_id2, string _read_characteristic, string _read_characteristic2, string _write_characteristic)
    {
        if (!want_connect) yield break;
        device_name = _device_name;

        service_id = _service_id;
        read_characteristic = _read_characteristic;

        service_id2 = _service_id2;
        read_characteristic2 = _read_characteristic2;


        write_characteristic = _write_characteristic;

        quit();

        //yield return mono.StartCoroutine(connect_device());
        yield return connect_device();
        if (selectedDeviceId.Length == 0) yield break;

        UnityEngine.Debug.Log("connecting device finish");

        //yield return mono.StartCoroutine(connect_service());
        yield return connect_service();
        if (selectedServiceId.Length == 0) yield break;

        UnityEngine.Debug.Log("connecting service finish");


        //yield return mono.StartCoroutine(connect_read_characteristic());
        yield return connect_read_characteristic();
        if (selectedCharacteristicId.Length == 0) yield break;

        UnityEngine.Debug.Log("connecting read characteristic finish");

        read_subscribe();
    }

    IEnumerator connect_device()
    {
        UnityEngine.Debug.Log("connecting device...");
        BleApi.StartDeviceScan();
        BleApi.ScanStatus status = BleApi.ScanStatus.AVAILABLE;
        BleApi.DeviceUpdate device_res = new BleApi.DeviceUpdate();
        do
        {
            status = BleApi.PollDevice(ref device_res, false);
            //Debug.Log(count++);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                if (!devices.ContainsKey(device_res.id))
                    devices[device_res.id] = new Dictionary<string, string>() {
                            { "name", "" },
                            { "isConnectable", "False" }
                        };
                if (device_res.nameUpdated)
                    devices[device_res.id]["name"] = device_res.name;
                if (device_res.isConnectableUpdated)
                    devices[device_res.id]["isConnectable"] = device_res.isConnectable.ToString();
                // consider only devices which have a name and which are connectable
                if (devices[device_res.id]["name"] == device_name && devices[device_res.id]["isConnectable"] == "True")
                {
                    //BleApi.Connect(device_res.id);
                    selectedDeviceId = device_res.id;
                    UnityEngine.Debug.Log("Connected to " + devices[device_res.id]["name"]);
                    break;
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {

                if (selectedDeviceId.Length == 0)
                {
                    UnityEngine.Debug.LogError("device " + device_name + " not found!");
                }
            }
            yield return 0;
        } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);
    }

    /* IEnumerator connect_service()
    {
        Debug.Log("connecting service...");
        BleApi.ScanServices(selectedDeviceId);
        BleApi.ScanStatus status;
        BleApi.Service service_res = new BleApi.Service();
        do
        {
            status = BleApi.PollService(out service_res, false);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                Debug.Log("Service ID Found: " + service_res.uuid);
                if (service_res.uuid == service_id)
                {
                    selectedServiceId = service_res.uuid;
                    break;
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                if (selectedServiceId.Length == 0)
                {
                    Debug.LogError("service " + service_id  + " not found!");
                }
            }
            yield return 0;
        } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);
    }
    */
    IEnumerator connect_service()
    {
        UnityEngine.Debug.Log("SB connecting service...");
        BleApi.ScanServices(selectedDeviceId);
        BleApi.ScanStatus status;
        BleApi.Service service_res = new BleApi.Service();
        List<Guid> foundServiceIds = new List<Guid>(); // Used to store the found service_id

        // Convert target service_id string to Guid
        Guid targetServiceId1 = new Guid("00001826-0000-1000-8000-00805f9b34fb");
        Guid targetServiceId2 = new Guid("a026ee0d-0a7d-4ab3-97fa-f1500f9feb8b");

        do
        {
            status = BleApi.PollService(out service_res, false);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                UnityEngine.Debug.Log("SB Service ID Found: " + service_res.uuid);

                // Convert the scanned service_res.uuid to Guid
                Guid scannedServiceId = new Guid(service_res.uuid);

                if (scannedServiceId == targetServiceId1 || scannedServiceId == targetServiceId2)
                {
                    foundServiceIds.Add(scannedServiceId);
                    if (foundServiceIds.Count == 2)
                    {
                        UnityEngine.Debug.Log("ServiceIds are found:" + foundServiceIds[0] + " and " + foundServiceIds[1]);// Quit the loop when two different service_ids are found
                        break;
                    }
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                if (foundServiceIds.Count < 2)
                {
                    UnityEngine.Debug.LogError("SB not enough service_ids found!");
                }
            }
            yield return 0;
        } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);
    }
    /*IEnumerator connect_read_characteristic()
        {
            UnityEngine.Debug.Log("connecting characteristic...");
            BleApi.ScanCharacteristics(selectedDeviceId, selectedServiceId);
            BleApi.ScanStatus status;
            BleApi.Characteristic characteristics_res = new BleApi.Characteristic();

            do
            {
                status = BleApi.PollCharacteristic(out characteristics_res, false);
                UnityEngine.Debug.Log("status: " + status);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    UnityEngine.Debug.Log("Read characteristics: " + characteristics_res.uuid);
                    if (characteristics_res.uuid == read_characteristic)
                    {
                        selectedCharacteristicId = characteristics_res.uuid;
                        break;
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    if (selectedCharacteristicId.Length == 0)
                    {
                        UnityEngine.Debug.LogError("characteristic " + read_characteristic + " not found!");
                    }
                }
                yield return 0;
            } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);
        }*/
        
    IEnumerator connect_read_characteristic()
        {
            UnityEngine.Debug.Log("SB connecting characteristic...");
            BleApi.ScanCharacteristics(selectedDeviceId, selectedServiceId);
            UnityEngine.Debug.Log("SB selectedDeviceId: " + selectedDeviceId);
            BleApi.ScanStatus status = BleApi.ScanStatus.AVAILABLE;
            BleApi.Characteristic characteristics_res = new BleApi.Characteristic();
            List<Guid> foundCharacteristicIds = new List<Guid>(); // Used to store the found characteristics

            // Convert target read_characteristic string to Guid
            //Guid targetCharacteristicId1 = new Guid("00002ad2-0000-1000-8000-00805f9b34fb");
            Guid targetCharacteristicId2 = new Guid("a026ee0d-0a7d-4ab3-97fa-f1500f9feb8b");

            do
            {
                status = BleApi.PollCharacteristic(out characteristics_res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    UnityEngine.Debug.Log("SB Read characteristics: " + characteristics_res.uuid);

                    // Convert the scanned characteristics_res.uuid to Guid
                    Guid scannedCharacteristicId = new Guid(characteristics_res.uuid);

                //if (scannedCharacteristicId == targetCharacteristicId1 || scannedCharacteristicId == targetCharacteristicId2)
                if (scannedCharacteristicId == targetCharacteristicId2)
                {
                    foundCharacteristicIds.Add(scannedCharacteristicId);
                    UnityEngine.Debug.Log("Characteristics found: " + foundCharacteristicIds[0]);

                    if (foundCharacteristicIds.Count > 2)
                        {
                        // Find two different characteristics and exit the loop
                        UnityEngine.Debug.Log("Total characteristics found: " + foundCharacteristicIds.Count);
                        break;
                        }
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    ////UnityEngine.Debug.LogError("SB not enough characteristics found! ");
                    //if (foundCharacteristicIds.Count < 2)
                    //{
                    //UnityEngine.Debug.Log("Found characteristics: " + foundCharacteristicIds);
                    //}
                }
                yield return 0;
            } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);

            // The foundCharacteristicIds list contains two different characteristics
        }

    /* void read_subscribe()
    {
        UnityEngine.Debug.Log("Subscribe...");
        BleApi.SubscribeCharacteristic_Read(selectedDeviceId, selectedServiceId, selectedCharacteristicId, false);
        isSubscribed = true;
    }
*/
    void read_subscribe()
        {
            UnityEngine.Debug.Log("SB Subscribe...");
            BleApi.SubscribeCharacteristic_Read(selectedDeviceId, selectedServiceId, selectedCharacteristicId, false);
            BleApi.ScanStatus status = BleApi.ScanStatus.AVAILABLE;
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                BleApi.SubscribeCharacteristic_Read(selectedDeviceId, selectedServiceId2, selectedCharacteristicId2, false);
                UnityEngine.Debug.LogError("Button service subscribed!");
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                UnityEngine.Debug.LogError("SB subscribes only one service!");
            }

            isSubscribed = true;
        }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void quit()
    {
        BleApi.Quit();
    }

    public void Update()
    {
        //UnityEngine.Debug.Log("isSubscribed: " + isSubscribed);
        if (isSubscribed)
        {
            
            
            BleApi.BLEData res = new BleApi.BLEData();
            
            while (BleApi.PollData(out res, false))
            {
                // Check which service the packet belongs to
                if (res.serviceUuid == selectedServiceId)
                {
                    // Data processing for cycling services
                    ProcessCyclingServiceData(ref res);
                }
                else if (res.serviceUuid == selectedServiceId2)
                {
                    // Data processing for steer services
                    ProcessSteeringServiceData(ref res);
                }
                

                // log potential errors
                BleApi.ErrorMessage res_err = new BleApi.ErrorMessage();
                BleApi.GetError(out res_err);
                if (lastError != res_err.msg)
                {
                    UnityEngine.Debug.LogError(res_err.msg);
                    lastError = res_err.msg;
                }
            }
        }
    }

    private void ProcessCyclingServiceData(ref BleApi.BLEData res)
    {
        // Parse cycling service data from res
        output = String.Empty;
        int index = 0;
        int flags = BitConverter.ToUInt16(res.buf, index);
        index += 2;

        // Parse speed data
        //UnityEngine.Debug.Log("Current Game Mode: " + GameModeManager.Instance.currentGameMode);
        //if (GameModeManager.Instance.currentGameMode == GameModeManager.GameMode.Mode1)
        //{
        // // In the first game mode, set the speed to a preset constant value
        //    float presetSpeedValue = 10.0f; // Assumed constant speed value
        //    speed = presetSpeedValue;
        //}
        //else
        //{
        // Other modes, parse and use the actual speed value
        if ((flags & 0) == 0)
        {
            has_speed = true;
                float value = (float)BitConverter.ToUInt16(res.buf, index);
                speed = (value * 1.0f) / 100.0f;

                if (speed >= 0 && speed <= reasonableMaxSpeed)
                {
                    lastReasonableSpeed = speed;
                }
                else
                {
                    speed = lastReasonableSpeed;
                }

                output += "Speed: " + speed + "\n";
                index += 2;
            }
        //}

        if ((flags & 2) > 0)
        {
            //??
            has_average_speed = true;
            average_speed = BitConverter.ToUInt16(res.buf, index);

            if (average_speed >= 0 && average_speed <= reasonableMaxAvg_Speed)
            {
                lastReasonableAvg_Speed = average_speed; // Update the last reasonable average_speed value
            }
            else
            {
                average_speed = lastReasonableAvg_Speed; // Use the last reasonable average_speed value
            }

            //output += "Average Speed: " + average_speed + "\n";
            index += 2;
        }
        if ((flags & 4) > 0)
        {
            rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;

            if (rpm >= 0 && rpm <= reasonableMaxRPM)
            {
                lastReasonableRPM = rpm; // Update the last reasonable RPM value
            }
            else
            {
                rpm = lastReasonableRPM; // Use the last reasonable RPM value
            }

            output += "RPM: (rev/min): " + rpm + "\n";
            index += 2;
        }
        if ((flags & 8) > 0)
        {
            average_rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;

            if (average_rpm >= 0 && average_rpm <= reasonableMaxAvg_RPM)
            {
                lastReasonableAvg_RPM = average_rpm; // Update the last reasonable average_rpm value
            }
            else
            {
                average_rpm = lastReasonableAvg_RPM; // Use the last reasonable average_rpm value
            }

            //output += "Average RPM: " + average_rpm + "\n";
            index += 2;
        }
        if ((flags & 16) > 0)
        {
            // Calculate maximum distance threshold based on time
            float maxDistance = distancePerMinute * Time.time / 60.0f;
            distance = BitConverter.ToUInt16(res.buf, index);

            if (distance >= 0 && distance <= maxDistance)
            {
                lastReasonableDistance = distance; //  Update the last reasonable distance value
            }
            else
            {
                distance = lastReasonableDistance; //  Use the last reasonable distance value
            }

            output += "Distance (meter): " + distance + "\n";
            index += 2;
        }
        if ((flags & 32) > 0)
        {
            resistance = BitConverter.ToInt16(res.buf, index);

            if (resistance >= 0 && resistance <= reasonableMaxResistance)
            {
                lastReasonableResistance = resistance; //  Update the last reasonable resistance value 
            }
            else
            {
                resistance = lastReasonableDistance; // Use the last reasonable resistance value 
            }

            //output += "Resistance: " + resistance + "\n";
            index += 2;
        }
        if ((flags & 64) > 0)
        {
            power = BitConverter.ToInt16(res.buf, index);

            if (power >= 0 && power <= reasonableMaxPower)
            {
                lastReasonablePower = power; // Update the last reasonable power value 
            }
            else
            {
                power = lastReasonablePower; // Use the last reasonable power value 
            }

            //output += "Power (Watt): " + power + "\n";
            index += 2;
        }
        if ((flags & 128) > 0)
        {
            average_power = BitConverter.ToInt16(res.buf, index);

            if (average_power >= 0 && average_power <= reasonableMaxAvg_Power)
            {
                lastReasonableAvg_Power = average_power; // Update the last reasonable average_power value
            }
            else
            {
                average_power = lastReasonableAvg_Power; // Use the last reasonable average_power value
            }

            //output += "AveragePower: " + average_power + "\n";
            index += 2;
        }
        if ((flags & 256) > 0)
        {
            expended_energy = BitConverter.ToUInt16(res.buf, index);

            if (expended_energy >= 0 && expended_energy <= reasonableMaxEnergy)
            {
                lastReasonableEnergy = expended_energy; // Update the last reasonable expanded_energy value
            }
            else
            {
                expended_energy = lastReasonableEnergy; // Use the last reasonable expanded_energy value
            }

            //output += "ExpendedEnergy: " + expended_energy + "\n";
            index += 2;
        }

    }

    private void ProcessSteeringServiceData(ref BleApi.BLEData res)
    {
        // Parse from res to service data
        int flags = BitConverter.ToUInt16(res.buf, 0); // Assume flags is at the beginning of res.buf

        // Update steer button status
        if (flags == left_button_id)
        {
            leftButtonPressed = !is_left_clicked;
            is_left_clicked = !is_left_clicked;
            UnityEngine.Debug.Log("Left button state updated: " + is_left_clicked);
        }
        else if (flags == right_button_id)
        {
            rightButtonPressed = !is_right_clicked;
            is_right_clicked = !is_right_clicked;
            UnityEngine.Debug.Log("Right button state updated: " + is_right_clicked);
        }
    }

    //private void UpdateSteeringButtonState(ref BleApi.BLEData res)
    //{
    //    int flags = BitConverter.ToUInt16(res.buf, 0); // Assume flags is at the beginning of res.buf

    //    if (flags == left_button_id)
    //    {
    //        leftButtonPressed = !is_left_clicked;
    //        is_left_clicked = !is_left_clicked;
    //        UnityEngine.Debug.Log("Left button state updated: " + is_left_clicked);
    //    }
    //    else if (flags == right_button_id)
    //    {
    //        rightButtonPressed = !is_right_clicked;
    //        is_right_clicked = !is_right_clicked;
    //        UnityEngine.Debug.Log("Right button state updated: " + is_right_clicked);
    //    }
    //}

    private byte[] Convert16(string strText)
    {
        strText = strText.Replace(" ", "");
        byte[] bText = new byte[strText.Length / 2];
        for (int i = 0; i < strText.Length / 2; i++)
        {
            bText[i] = Convert.ToByte(Convert.ToInt32(strText.Substring(i * 2, 2), 16));
        }
        return bText;
    }

    public void Write(string msg)
    {
        
        byte[] payload22 = Convert16(msg);
        BleApi.BLEData data = new BleApi.BLEData();
        data.buf = new byte[512];
        data.size = (short)payload22.Length;
        data.deviceId = selectedDeviceId;
        data.serviceUuid = selectedServiceId;
        data.characteristicUuid = write_characteristic;
        for (int i = 0; i < payload22.Length; i++)
        {
            data.buf[i] = payload22[i];
        }
        BleApi.SendData(in data, false);
    }
    // Update is called once per frame
    /* public void Update()
     {


         if (isSubscribed)
         {

             BleApi.BLEData res = new BleApi.BLEData();
             while (BleApi.PollData(out res, false))
             {
                 {
                     has_speed = false;
                     has_average_speed = false;
                     has_rpm = false;
                     has_average_rpm = false;
                     has_distance = false;
                     has_resistance = false;
                     has_power = false;
                     has_average_power = false;
                     has_expended_energy = false;
                 }

                 output = String.Empty;
                 int index = 0;
                 int flags = BitConverter.ToUInt16(res.buf, index);
                 index += 2;
                 if ((flags & 0) == 0)
                 {
                     has_speed = true;
                     float value = (float)BitConverter.ToUInt16(res.buf, index);
                     speed = (value * 1.0f) / 100.0f;

                     if (speed >= 0 && speed <= reasonableMaxSpeed)
                     {
                         lastReasonableSpeed = speed; // Update the last reasonable speed value
                     }
                     else
                     {
                         speed = lastReasonableSpeed; // Update the last reasonable speed value
                     }

                     output += "Speed: " + speed + "\n";
                     index += 2;
                     //UnityEngine.Debug.Log("has_speed:" + has_speed+ "\n" + "speed:"+speed);
                 }
                 if ((flags & 2) > 0)
                 {
                     //??
                     has_average_speed = true;
                     average_speed = BitConverter.ToUInt16(res.buf, index);

                     if (average_speed >= 0 && average_speed <= reasonableMaxAvg_Speed)
                     {
                         lastReasonableAvg_Speed = average_speed; // Update the last reasonable average_speed value
                     }
                     else
                     {
                         average_speed = lastReasonableAvg_Speed; // Update the last reasonable average_speed value
                     }

                     //output += "Average Speed: " + average_speed + "\n";
                     index += 2;
                 }
                 if ((flags & 4) > 0)
                 {
                     rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;

                     if (rpm >= 0 && rpm <= reasonableMaxRPM)
                     {
                         lastReasonableRPM = rpm; // Update the last reasonable RPM value
                     }
                     else
                     {
                         rpm = lastReasonableRPM; // Update the last reasonable RPM value
                     }

                     output += "RPM: (rev/min): " + rpm + "\n";
                     index += 2;
                 }
                 if ((flags & 8) > 0)
                 {
                     average_rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;

                     if (average_rpm >= 0 && average_rpm <= reasonableMaxAvg_RPM)
                     {
                         lastReasonableAvg_RPM = average_rpm; // Update the last reasonable average_rpm value
                     }
                     else
                     {
                         average_rpm = lastReasonableAvg_RPM; // Update the last reasonable average_rpm value
                     }

                     //output += "Average RPM: " + average_rpm + "\n";
                     index += 2;
                 }
                 if ((flags & 16) > 0)
                 {
                     float maxDistance = distancePerMinute * Time.time / 60.0f; 
                     distance = BitConverter.ToUInt16(res.buf, index); 

                     if (distance >= 0 && distance <= maxDistance)
                     {
                         lastReasonableDistance = distance; //  Update the last reasonable distance value
                     }
                     else
                     {
                         distance = lastReasonableDistance; // Update the last reasonable distance value
                     }

                     output += "Distance (meter): " + distance + "\n";
                     index += 2;
                 }
                 if ((flags & 32) > 0)
                 {
                     resistance = BitConverter.ToInt16(res.buf, index);

                     if (resistance >= 0 && resistance <= reasonableMaxResistance)
                     {
                         lastReasonableResistance = resistance; //Update the last reasonable resistance value
                     }
                     else
                     {
                         resistance = lastReasonableDistance; // Update the last reasonable resistance value
                     }

                     //output += "Resistance: " + resistance + "\n";
                     index += 2;
                 }
                 if ((flags & 64) > 0)
                 {
                     power = BitConverter.ToInt16(res.buf, index);

                     if (power >= 0 && power <= reasonableMaxPower)
                     {
                         lastReasonablePower = power; // Update the last reasonable power value
                     }
                     else
                     {
                         power = lastReasonablePower; // Update the last reasonable power value
                     }

                     //output += "Power (Watt): " + power + "\n";
                     index += 2;
                 }
                 if ((flags & 128) > 0)
                 {
                     average_power = BitConverter.ToInt16(res.buf, index);

                     if (average_power >= 0 && average_power <= reasonableMaxAvg_Power)
                     {
                         lastReasonableAvg_Power = average_power; // Update the last reasonable average_power value
                     }
                     else
                     {
                         average_power = lastReasonableAvg_Power; // Update the last reasonable average_power value
                     }

                     //output += "AveragePower: " + average_power + "\n";
                     index += 2;
                 }
                 if ((flags & 256) > 0)
                 {
                     expended_energy = BitConverter.ToUInt16(res.buf, index);

                     if (expended_energy >= 0 && expended_energy <= reasonableMaxEnergy)
                     {
                         lastReasonableEnergy = expended_energy; // Update the last reasonable expended_energy value
                     }
                     else
                     {
                         expended_energy = lastReasonableEnergy; // Update the last reasonable expended_energy value

                     //output += "ExpendedEnergy: " + expended_energy + "\n";
                     index += 2;
                 }
                 //Steering
                 UpdateSteeringButtonState(ref res);

                 /*if (flags == left_button_id && is_left_clicked == false)
                 {
                     is_left_clicked = true;
                     UnityEngine.Debug.Log("Left button is pressed");
                 }
                 else if (flags == left_button_id && is_left_clicked == true)
                 {
                     is_left_clicked = false;
                     UnityEngine.Debug.Log("Left button is released");
                 }
                 else if (flags == right_button_id && is_right_clicked == false)
                 {
                     is_right_clicked = true;
                     UnityEngine.Debug.Log("Right button is pressed");
                 }
                 else if (flags == right_button_id && is_right_clicked == true)
                 {
                     is_right_clicked = false;
                     UnityEngine.UnityEngine.Debug.Log("Right button is released");
                 }
             }

             // log potential errors
             BleApi.ErrorMessage res_err = new BleApi.ErrorMessage();
             BleApi.GetError(out res_err);
             if (lastError != res_err.msg)
             {
                 UnityEngine.UnityEngine.Debug.LogError(res_err.msg);
                 lastError = res_err.msg;
             }
         }


     }*/

    public void write_resistance(float val)
    {
        write_resistance(Mathf.FloorToInt(val));
    }
    public void write_resistance(int val)
    {
        if (Time.time - last_write_time < 0.1f)
        {
            return;
        }
        else
        {
            last_write_time = Time.time;
        }

        UnityEngine.Debug.Log("write resistance: " + val);

        BleApi.SubscribeCharacteristic_Write(selectedDeviceId, selectedServiceId, write_characteristic, false);
        // Write("00");
        byte[] bytes = BitConverter.GetBytes(val);
        resistance_byte = bytes[0];
        UnityEngine.Debug.Log("Byte: " + $"0x{resistance_byte:X2}");
        byte[] payload = { 0x11, wind_speed_byte[0], wind_speed_byte[1], grade_bytes[0], grade_bytes[1], resistance_byte, 0x00 };

        BleApi.BLEData data = new BleApi.BLEData();
        data.buf = new byte[512];
        data.deviceId = selectedDeviceId;
        data.serviceUuid = selectedServiceId;
        data.characteristicUuid = write_characteristic;
        for (int i = 0; i < payload.Length; i++)
        {
            data.buf[i] = payload[i];
        }
        data.size = (short)payload.Length;
        BleApi.SendData(in data, false);
    }

    public void write_windSpeed(float val)
    {
        write_windSpeed(Mathf.FloorToInt(val));
    }
    public void write_windSpeed(int val)
    {
        if (Time.time - last_write_time < 0.1f)
        {
            return;
        }
        else
        {
            last_write_time = Time.time;
        }
        short signed_val = (short)(val * 100);

        UnityEngine.Debug.Log("write wind speed: " + signed_val);

        BleApi.SubscribeCharacteristic_Write(selectedDeviceId, selectedServiceId, write_characteristic, false);
        // Write("00");
        byte[] bytes = BitConverter.GetBytes(val);
        wind_speed_byte = BitConverter.GetBytes(signed_val);
        UnityEngine.Debug.Log("Byte: " + $"0x{wind_speed_byte:X2}");
        byte[] payload = { 0x11, wind_speed_byte[0], wind_speed_byte[1], grade_bytes[0], grade_bytes[1], resistance_byte, 0x00 };

        BleApi.BLEData data = new BleApi.BLEData();
        data.buf = new byte[512];
        data.deviceId = selectedDeviceId;
        data.serviceUuid = selectedServiceId;
        data.characteristicUuid = write_characteristic;
        for (int i = 0; i < payload.Length; i++)
        {
            data.buf[i] = payload[i];
        }
        data.size = (short)payload.Length;
        BleApi.SendData(in data, false);
    }

    public void write_grade(int val)
    {
        if (Time.time - last_write_time < 0.1f)
        {
            return;
        }
        else
        {
            last_write_time = Time.time;
        }


        short signed_val = (short)(val * 100);

        BleApi.SubscribeCharacteristic_Write(selectedDeviceId, selectedServiceId, write_characteristic, false);
        // Write("00");
        UnityEngine.Debug.Log("write grade: " + signed_val);
        grade_bytes = BitConverter.GetBytes(signed_val);


        UnityEngine.Debug.Log("Byte 0: " + $"0x{grade_bytes[0]:X2}");
        UnityEngine.Debug.Log("Byte 1: " + $"0x{grade_bytes[1]:X2}");
        byte[] payload = { 0x11, wind_speed_byte[0], wind_speed_byte[1], grade_bytes[0], grade_bytes[1], resistance_byte, 0x00 };
        BleApi.BLEData data = new BleApi.BLEData();
        data.buf = new byte[512];
        data.deviceId = selectedDeviceId;
        data.serviceUuid = selectedServiceId;
        data.characteristicUuid = write_characteristic;
        for (int i = 0; i < payload.Length; i++)
        {
            data.buf[i] = payload[i];
            UnityEngine.Debug.Log("Buffer-byte " + i + ": " + $"0x{data.buf[i]:X2}");
        }
        data.size = (short)payload.Length;
        BleApi.SendData(in data, false);
    }

}
