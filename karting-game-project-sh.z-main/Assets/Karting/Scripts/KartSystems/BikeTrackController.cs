using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace KartGame.KartSystems
{
    public class BikeTrackController : MonoBehaviour
    {
        private ArcadeBike BikeControll;
        public DataSender dataSender;
        private FTMS_IndoorBike Bike;
        public GameObject FTMS_IndoorBike_GO;

        private object lockObject = new object();

        private SegmentTrack currentTrack;
        private int previousGrade = 0; // Variable to store the previous grade
        private float previousResistance = 0; // Variable to store the previous grade
        private float previousWindSpeed = 0; // Variable to store the previous grade


        private void Start()
        {
            BikeControll = GetComponent<ArcadeBike>();
            Bike = FTMS_IndoorBike_GO.GetComponent<FTMS_IndoorBike>();
            //StartCoroutine(WaitForBikeConnection());
        }

        /* private IEnumerator WaitForScanningAndConnect()
        {
            while (!scanningComplete)
            {
                lock (lockObject)
                {
                    if (scanningComplete)
                        break;
                }
                yield return null;
            }

            Update();
            //BleApi.StopDeviceScan();
            //WaitForBikeConnection();
            //ConnectToBike(); // Call the connection function when scanning is complete
        }
        
        private void ConnectToBike()
        {
            if (Kart.connected)
            {
                StartCoroutine(WaitForBikeConnection());
            }
            else
            {
                UnityEngine.Debug.Log("Kart not connected.");
            }
        }

        private IEnumerator WaitForBikeConnection()
        {
            yield return new WaitUntil(() => Bike.isSubscribed);
            WaitForScanningAndConnect();
            scanningComplete = true;
            UnityEngine.Debug.Log("Bike connected.");
        } */


        private void Update()
        {
            //UnityEngine.Debug.Log("BikeTrackController Update is called");
            //if (BikeControll.connected && Bike.isSubscribed)
            if (Bike.isSubscribed)
            {
                RaycastHit hit;
                Vector3 forwardDown = (transform.forward - transform.up).normalized;

                if (Physics.Raycast(transform.position, forwardDown, out hit, 1f, LayerMask.GetMask("Track")))
                {
                    currentTrack = hit.collider.GetComponent<SegmentTrack>();
                    if (currentTrack != null)
                    {
                        int grade = currentTrack.grade;
                        float resistance = currentTrack.resistance;
                        float windSpeed = currentTrack.WindSpeed;

                        if (GameModeManager.Instance.currentGameMode == GameModeManager.GameMode.Mode4 || GameModeManager.Instance.currentGameMode == GameModeManager.GameMode.Mode2)
                        {
                            // Only use inclination and resistance values in Mode2 and 4
                            if (grade != previousGrade)
                            {
                                //dataSender.SendData(grade);
                                Bike.write_grade(grade);
                                previousGrade = grade;
                            }
                            if (resistance != previousResistance)
                            {
                                UnityEngine.Debug.Log("Current Track:" + currentTrack + "\n" + "privoursResistance： " + previousResistance + "\n" + "currentResistance： " + resistance);
                                Bike.write_resistance(resistance);
                                previousResistance = resistance;
                            }
                            if (windSpeed != previousWindSpeed)
                            {
                                UnityEngine.Debug.Log("Current Track:" + currentTrack + "\n" + "privoursWindSpeed： " + previousWindSpeed + "\n" + "currentWindSpeed： " + windSpeed);
                                Bike.write_windSpeed(windSpeed);
                                previousWindSpeed = windSpeed;
                            }

                        }
                    }
                }
            }
        }
    }
}
