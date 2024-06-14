using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace KartGame.KartSystems
{
    public class KartTrackController : MonoBehaviour
    {
        private ArcadeKart Kart;
        public DataSender dataSender; 
        private FTMS_IndoorBike Bike;
        public GameObject FTMS_IndoorBike_GO;

        private object lockObject = new object();

        private SegmentTrack currentTrack;
        private int previousGrade = 0; // Variable to store the previous grade
        private float previousResistance = 0; // Variable to store the previous grade


        private void Start()
        {
            Kart = GetComponent<ArcadeKart>();
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
            //UnityEngine.Debug.Log("KartTrackController Update is called");
            //if (Kart.connected && Bike.isSubscribed)
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

                        // 更正：使用静态成员访问方式
                        if (GameModeManager.Instance.currentGameMode == GameModeManager.GameMode.Mode4)
                        {
                            // 只有在 Mode4 中才使用坡度和阻力值
                            if (grade != previousGrade)
                            {
                                dataSender.SendData(grade);
                                previousGrade = grade;
                            }
                            if (resistance != previousResistance)
                            {
                                UnityEngine.Debug.Log("Current Track:" + currentTrack + "\n" + "privoursResistance： " + previousResistance + "\n" + "currentResistance： " + resistance);
                                previousResistance = resistance;
                            }
                        }
                    }
                }
            }
        }
    }
}
