using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace KartGame.KartSystems
{
    public class TestBikeInput : BaseInput
    {
        public FTMS_IndoorBike indoorBike;
        private NetworkBehaviour networkBehaviour;

        private void Awake()
        {
            networkBehaviour = GetComponent<NetworkBehaviour>();
            if (networkBehaviour == null)
            {
                UnityEngine.Debug.LogError("NetworkBehaviour not found on the GameObject.");
            }
        }

        public override InputData GenerateInput()
        {
            if (networkBehaviour != null && networkBehaviour.isLocalPlayer)
            {
                // 使用 indoorBike 的数据生成 InputData
                return new InputData
                {
                    Accelerate = indoorBike.speed > 0,
                    Brake = false,  // Adjust as needed
                    TurnInput = indoorBike.is_right_clicked ? 1 : (indoorBike.is_left_clicked ? -1 : 0)
                };
            }

            return new InputData(); // If it is not a local player or networkBehaviour is null, return the default InputData.
        }
    }
}


