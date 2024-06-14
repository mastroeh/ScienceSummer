using System.Diagnostics;
using UnityEngine;
using Mirror;


namespace KartGame.KartSystems {

    public class KeyboardInput : BaseInput
    {
        public string TurnInputName = "Horizontal";
        public string AccelerateButtonName = "Accelerate";
        public string BrakeButtonName = "Brake";
        private NetworkBehaviour networkBehaviour;

        private void Awake()
        {
            networkBehaviour = GetComponent<NetworkBehaviour>();
            if (networkBehaviour == null)
            {
                UnityEngine.Debug.LogError("NetworkBehaviour not found on the GameObject.");
            }
        }

        public override InputData GenerateInput() {
            if (networkBehaviour != null && networkBehaviour.isLocalPlayer)
            {
                return new InputData
                {
                    Accelerate = Input.GetButton(AccelerateButtonName),
                    Brake = Input.GetButton(BrakeButtonName),
                    TurnInput = Input.GetAxis("Horizontal")
                };
            }
            return new InputData();
        }
    }
}
