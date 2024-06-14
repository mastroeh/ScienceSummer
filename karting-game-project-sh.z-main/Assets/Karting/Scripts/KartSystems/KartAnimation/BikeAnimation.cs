using System;
using UnityEngine;

namespace KartGame.KartSystems
{
    [DefaultExecutionOrder(100)]
    public class BikeAnimation : MonoBehaviour
    {
        [Serializable] public class Wheel
        {
            [Tooltip("A reference to the transform of the wheel.")]
            public Transform wheelTransform;
            [Tooltip("A reference to the WheelCollider of the wheel.")]
            public WheelCollider wheelCollider;
            
            Quaternion m_SteerlessLocalRotation;

            public void Setup() => m_SteerlessLocalRotation = wheelTransform.localRotation;

            public void StoreDefaultRotation() => m_SteerlessLocalRotation = wheelTransform.localRotation;
            public void SetToDefaultRotation() => wheelTransform.localRotation = m_SteerlessLocalRotation;
        }

        [Tooltip("What bike do we want to listen to?")]
        public ArcadeBike bikeController;

        [Space]
        [Tooltip("The damping for the appearance of steering compared to the input.  The higher the number the less damping.")]
        public float steeringAnimationDamping = 10f;

        [Space]
        [Tooltip("The maximum angle in degrees that the front wheels can be turned away from their default positions, when the Steering input is either 1 or -1.")]
        public float maxSteeringAngle;
        [Tooltip("Information referring to the front  wheel of the kart.")]
        public Wheel Wheel_F;
        [Tooltip("Information referring to the rear wheel of the kart.")]
        public Wheel Wheel_B;


        float m_SmoothedSteeringInput;

        void Start()
        {
            Wheel_F.Setup();
            Wheel_B.Setup();
        }

        void FixedUpdate() 
        {
            m_SmoothedSteeringInput = Mathf.MoveTowards(m_SmoothedSteeringInput, bikeController.Input.TurnInput, 
                steeringAnimationDamping * Time.deltaTime);

            // Steer front wheels
            float rotationAngle = m_SmoothedSteeringInput * maxSteeringAngle;

            Wheel_F.wheelCollider.steerAngle = rotationAngle;

            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(Wheel_F);
            UpdateWheelFromCollider(Wheel_B);
        }

        void LateUpdate()
        {
            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(Wheel_F);
            UpdateWheelFromCollider(Wheel_B);
        }

        void UpdateWheelFromCollider(Wheel wheel)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheel.wheelTransform.position = position;
            wheel.wheelTransform.rotation = rotation;
        }
    }
}
