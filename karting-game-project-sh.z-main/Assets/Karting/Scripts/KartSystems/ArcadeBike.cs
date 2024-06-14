﻿using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;
using System.Security.Cryptography;
using System.Diagnostics;
using UnityEngine.UI;

namespace KartGame.KartSystems
{
    public class ArcadeBike : MonoBehaviour
    {

        [System.Serializable]
        public class StatPowerup
        {
            public ArcadeBike.Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
        }
        public class CycleGeometry
        {
            public GameObject handles, lowerFork, fWheelVisual, RWheel, crank, lPedal, rPedal, fGear, rGear;
        }
        [HideInInspector]
        public float cycleOscillation;
        [HideInInspector]
        public float bunnyHopAmount;
        [HideInInspector]
        public float turnLeanAmount;
        [HideInInspector]
        public Vector3 lastVelocity, deceleration, lastDeceleration;
        [HideInInspector]
        public float crankSpeed, crankCurrentQuat, crankLastQuat, restingCrank;
        [HideInInspector]
        public float customSteerAxis, customLeanAxis, customAccelerationAxis, rawCustomAccelerationAxis;
        [HideInInspector]
        public int bunnyHopInputState;
        [Range(0, 8)]
        public float oscillationAmount;
        [System.Serializable]
        public class AirTimeSettings
        {
            public bool freestyle;
            public float airTimeRotationSensitivity;
            [Range(0.5f, 10)]
            public float heightThreshold;
            public float groundSnapSensitivity;
        }
        public AirTimeSettings airTimeSettings;

        [System.Serializable]
        public struct Stats
        {
            [Header("Movement Settings")]
            [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
            public float TopSpeed;

            [Tooltip("How quickly the kart reaches top speed.")]
            public float Acceleration;

            [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
            public float ReverseSpeed;

            [Tooltip("How quickly the kart reaches top speed, when moving backward.")]
            public float ReverseAcceleration;

            [Tooltip("How quickly the kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
            [Range(0.2f, 1)]
            public float AccelerationCurve;

            [Tooltip("How quickly the kart slows down when the brake is applied.")]
            public float Braking;

            [Tooltip("How quickly the kart will reach a full stop when no inputs are made.")]
            public float CoastingDrag;

            [Range(0.0f, 1.0f)]
            [Tooltip("The amount of side-to-side friction.")]
            public float Grip;

            [Tooltip("How tightly the kart can turn left or right.")]
            public float Steer;

            [Tooltip("Additional gravity for when the kart is in the air.")]
            public float AddedGravity;

            // allow for stat adding for powerups.
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    Acceleration = a.Acceleration + b.Acceleration,
                    AccelerationCurve = a.AccelerationCurve + b.AccelerationCurve,
                    Braking = a.Braking + b.Braking,
                    CoastingDrag = a.CoastingDrag + b.CoastingDrag,
                    AddedGravity = a.AddedGravity + b.AddedGravity,
                    Grip = a.Grip + b.Grip,
                    ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                    ReverseSpeed = a.ReverseSpeed + b.ReverseSpeed,
                    TopSpeed = a.TopSpeed + b.TopSpeed,
                    Steer = a.Steer + b.Steer,
                };
            }
        }

        private PlayerState savedState;
        private float saveInterval = 5.0f; // Save state every 5 seconds
        private float timer = 0;

        // Define the structure used to save the player state
        private struct PlayerState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public Vector3 angularVelocity;
        }

        public Rigidbody Rigidbody { get; private set; }
        public InputData Input { get; private set; }
        public float AirPercent { get; private set; }
        public float GroundPercent { get; private set; }
        public FTMS_IndoorBike indoorBike { get; private set; }
        public GameObject FTMS_IndoorBike_GO;
        
        public string device_name = "";
        public string service_id = "";
        public string read_characteristic = "";
        public string service_id2 = "";
        public string read_characteristic2 = "";
        public string write_characteristic = "";
        //public string device_name = "KICKR BIKE 65E9";
        //public string service_id = "{00001826-0000-1000-8000-00805f9b34fb}";
        //public string read_characteristic = "{00002ad2-0000-1000-8000-00805f9b34fb}";
        //public string write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}";
        public bool connected = false;
        public Text info;
        public float resistance;
        public int grade;
        //public Vector3 COG;

        public ArcadeBike.Stats baseStats = new ArcadeBike.Stats
        {
            TopSpeed = 12f,
            Acceleration = 5f,
            AccelerationCurve = 4f,
            Braking = 10f,
            ReverseAcceleration = 5f,
            ReverseSpeed = 5f,
            Steer = 5f,
            CoastingDrag = 4f,
            Grip = .95f,
            AddedGravity = 1f,
        };

        [Header("Vehicle Visual")]
        public List<GameObject> m_VisualWheels;

        [Header("Vehicle Physics")]
        [Tooltip("The transform that determines the position of the kart's mass.")]
        public Transform CenterOfMass;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the kart in the air. The higher the number, the faster the kart will readjust itself along the horizontal plane.")]
        public float AirborneReorientationCoefficient = 3.0f;

        [Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        public float DriftGrip = 0.4f;
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the kart is drifting.")]
        public float DriftAdditionalSteer = 5.0f;
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        public float MinAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        public float MinSpeedPercentToFinishDrift = 0.5f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        public float DriftControl = 10.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        public float DriftDampening = 10.0f;

        [Header("VFX")]
        //[Tooltip("VFX that will be placed on the wheels when drifting.")]
        //public ParticleSystem DriftSparkVFX;
        [Range(0.0f, 0.2f), Tooltip("Offset to displace the VFX to the side.")]
        public float DriftSparkHorizontalOffset = 0.1f;
        [Range(0.0f, 90.0f), Tooltip("Angle to rotate the VFX.")]
        public float DriftSparkRotation = 17.0f;
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public GameObject DriftTrailPrefab;
        [Range(-0.1f, 0.1f), Tooltip("Vertical to move the trails up or down and ensure they are above the ground.")]
        public float DriftTrailVerticalOffset;
        //[Tooltip("VFX that will spawn upon landing, after a jump.")]
        //public GameObject JumpVFX;
        //[Tooltip("VFX that is spawn on the nozzles of the kart.")]
        //public GameObject NozzleVFX;
        //[Tooltip("List of the kart's nozzles.")]
        //public List<Transform> Nozzles;

        [Header("Suspensions")]
        [Tooltip("The maximum extension possible between the kart's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        public float SuspensionHeight = 0.0f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        public float SuspensionSpring = 20000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the kart will stabilize itself.")]
        public float SuspensionDamp = 500.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the kart's body.")]
        [Range(-1.0f, 1.0f)]
        public float WheelsPositionVerticalOffset = 0.0f;

        [Header("Physical Wheels")]
        [Tooltip("The physical representations of the Kart's wheels.")]
        public WheelCollider Wheel_B;
        public WheelCollider Wheel_F;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;


        // the input sources that can control the kart
        IInput[] m_Inputs;

        const float k_NullInput = 0.01f;
        const float k_NullSpeed = 0.01f;
        Vector3 m_VerticalReference = Vector3.up;

        // Drift params
        public bool WantsToDrift { get; private set; } = false;
        public bool IsDrifting { get; private set; } = false;
        float m_CurrentGrip = 1.0f;
        float m_DriftTurningPower = 0.0f;
        float m_PreviousGroundPercent = 1.0f;
        readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();
        readonly List<(WheelCollider wheel, float horizontalOffset, float rotation, ParticleSystem sparks)> m_DriftSparkInstances = new List<(WheelCollider, float, float, ParticleSystem)>();

        // can the kart move?
        bool m_CanMove = true;
        List<StatPowerup> m_ActivePowerupList = new List<StatPowerup>();
        ArcadeBike.Stats m_FinalStats;

        public Transform handle;
        public Transform frontWheeltransform;
        float currentSteeringAngle;
        public float maxSteeringAngle = 30f;
        public float steeringRate = 10f; 

        //public Transform backWheeltransform;

        Quaternion m_LastValidRotation;
        Vector3 m_LastValidPosition;
        Vector3 m_LastCollisionNormal;
        bool m_HasCollision;
        public bool m_InAir = false;


        public CycleGeometry cycleGeometry;


        public void AddPowerup(StatPowerup statPowerup) => m_ActivePowerupList.Add(statPowerup);
        public void SetCanMove(bool move) => m_CanMove = move;
        public float GetMaxSpeed() => Mathf.Max(m_FinalStats.TopSpeed, m_FinalStats.ReverseSpeed);

        private void ActivateDriftVFX(bool active)
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                if (active && vfx.wheel.GetGroundHit(out WheelHit hit))
                {
                    if (!vfx.sparks.isPlaying)
                        vfx.sparks.Play();
                }
                else
                {
                    if (vfx.sparks.isPlaying)
                        vfx.sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

            }

            foreach (var trail in m_DriftTrailInstances)
                trail.Item3.emitting = active && trail.wheel.GetGroundHit(out WheelHit hit);
        }

        private void UpdateDriftVFXOrientation()
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                vfx.sparks.transform.position = vfx.wheel.transform.position - (vfx.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up) + (transform.right * vfx.horizontalOffset);
                vfx.sparks.transform.rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, vfx.rotation);
            }

            foreach (var trail in m_DriftTrailInstances)
            {
                trail.trailRoot.transform.position = trail.wheel.transform.position - (trail.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up);
                trail.trailRoot.transform.rotation = transform.rotation;
            }
        }

        void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = SuspensionHeight;
            wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = SuspensionSpring;
            spring.damper = SuspensionDamp;
            wheel.suspensionSpring = spring;
        }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();

            //if (COG != null)
            //{
            //    Rigidbody.centerOfMass = COG;
            //}

            // 使用单例实例
            indoorBike = FTMS_IndoorBike.Instance;

            connect();
            // Check whether the component is successfully obtained
            if (indoorBike != null)
            {
                UnityEngine.Debug.Log("FTMS_IndoorBike component acquired successfully.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("FTMS_IndoorBike component is not assigned.");
            }

            m_Inputs = GetComponents<IInput>();

            // Update front and rear wheel suspension parameters
            UpdateSuspensionParams(Wheel_B);
            UpdateSuspensionParams(Wheel_F);

            m_CurrentGrip = baseStats.Grip;

            // Handling drifting spark effects
            //if (DriftSparkVFX != null)
            //{
            //    AddSparkToWheel(Wheel_F, -DriftSparkHorizontalOffset, -DriftSparkRotation);
            //}

            //// Handling drift trajectory effects
            //if (DriftTrailPrefab != null)
            //{
            //    AddTrailToWheel(Wheel_F);
            //}

            //// Dealing with nozzle effects
            //if (NozzleVFX != null)
            //{
            //    foreach (var nozzle in Nozzles)
            //    {
            //        Instantiate(NozzleVFX, nozzle, false);
            //    }
            //}

        }


        void AddTrailToWheel(WheelCollider wheel)
            {
                GameObject trailRoot = Instantiate(DriftTrailPrefab, gameObject.transform, false);
                TrailRenderer trail = trailRoot.GetComponentInChildren<TrailRenderer>();
                trail.emitting = false;
                m_DriftTrailInstances.Add((trailRoot, wheel, trail));
            }

        //void AddSparkToWheel(WheelCollider wheel, float horizontalOffset, float rotation)
        //{
        //    GameObject vfx = Instantiate(DriftSparkVFX.gameObject, wheel.transform, false);
        //    ParticleSystem spark = vfx.GetComponent<ParticleSystem>();
        //    spark.Stop();
        //    m_DriftSparkInstances.Add((wheel, horizontalOffset, -rotation, spark));
        //}

        void FixedUpdate()
        {
            UpdateSuspensionParams(Wheel_B);
            UpdateSuspensionParams(Wheel_F);
            //if (indoorBike != null)
            //{
            GatherInputs();

            // apply our powerups to create our finalStats
            TickPowerups();

            // apply our physics properties
            //Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);

            int groundedCount = 0;
            if (Wheel_B.isGrounded && Wheel_B.GetGroundHit(out WheelHit hit))
                groundedCount++;
            if (Wheel_F.isGrounded && Wheel_F.GetGroundHit(out hit))
                groundedCount++;


            // calculate how grounded and airborne we are
            GroundPercent = (float)groundedCount / 4.0f;
            AirPercent = 1 - GroundPercent;

            UpdateHandle();

            // apply vehicle physics
            if (m_CanMove)
            {
                MoveVehicle(Input.Accelerate, Input.Brake, Input.TurnInput);
            }
            GroundAirbourne();

            m_PreviousGroundPercent = GroundPercent;

            //UpdateDriftVFXOrientation();

            timer += Time.deltaTime;

            // Save the player's current status at regular intervals
            if (timer >= saveInterval)
            {
                SavePlayerState();
                timer = 0;
            }

            // Check if R key is pressed
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                //ResetToRoadCenter();
                ResetToNearestWaypoint();
            }

            //}
            //else
            //{
            //    UnityEngine.Debug.LogWarning("indoorBike is not assigned.");

            //}
        }

        public void connect()
        {
            if (device_name.Length > 0 && service_id.Length > 0 && read_characteristic.Length > 0 && write_characteristic.Length > 0)
            {
            //StartCoroutine(indoorBike.connect(device_name, service_id, read_characteristic, write_characteristic));
            StartCoroutine(indoorBike.connect(device_name, service_id, service_id2, read_characteristic, read_characteristic2, write_characteristic));
            connected = true;
            }
        }

        void GatherInputs()
        {
            // reset input
            Input = new InputData();
            WantsToDrift = false;

            // gather nonzero input from our sources
            for (int i = 0; i < m_Inputs.Length; i++)
            {
                Input = m_Inputs[i].GenerateInput();
                //UnityEngine.Debug.Log("Input type:" + m_Inputs[i]);
                //WantsToDrift = Input.Brake && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
            }
        }

        public void UpdateHandle()
        {
            float turnInput = Input.TurnInput;
            float targetSteeringAngle = turnInput * maxSteeringAngle; // 计算目标转向角度

            // Smoothly interpolate current steering angle to target angle using Lerp
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle, steeringRate * Time.deltaTime);

            // Apply rotation to handlebars and front wheel
            handle.localRotation = Quaternion.Euler(handle.localRotation.eulerAngles.x, currentSteeringAngle, handle.localRotation.eulerAngles.z);
            frontWheeltransform.localRotation = Quaternion.Euler(frontWheeltransform.localRotation.eulerAngles.x, currentSteeringAngle, frontWheeltransform.localRotation.eulerAngles.z);
        }

        void TickPowerups()
            {
                // remove all elapsed powerups
                m_ActivePowerupList.RemoveAll((p) => { return p.ElapsedTime > p.MaxTime; });

                // zero out powerups before we add them all up
                var powerups = new Stats();

                // add up all our powerups
                for (int i = 0; i < m_ActivePowerupList.Count; i++)
                {
                    var p = m_ActivePowerupList[i];

                    // add elapsed time
                    p.ElapsedTime += Time.fixedDeltaTime;

                    // add up the powerups
                    powerups += p.modifiers;
                }

                // add powerups to our final stats
                m_FinalStats = baseStats + powerups;

                // clamp values in finalstats
                m_FinalStats.Grip = Mathf.Clamp(m_FinalStats.Grip, 0, 1);
            }

        void GroundAirbourne()
        {
            // while in the air, fall faster
            if (AirPercent >= 1)
            {
                Rigidbody.velocity += Physics.gravity * Time.fixedDeltaTime * m_FinalStats.AddedGravity;
            }
        }

        public void Reset()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        public float LocalSpeed()
        {
            if (m_CanMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.velocity.magnitude;
                    return dot < 0 ? -(speed / m_FinalStats.ReverseSpeed) : (speed / m_FinalStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
                // use this value to play kart sound when it is waiting the race start countdown.
                return Input.Accelerate ? 1.0f : 0.0f;
            }
        }

        void OnCollisionEnter(Collision collision) => m_HasCollision = true;
        void OnCollisionExit(Collision collision) => m_HasCollision = false;

        void OnCollisionStay(Collision collision)
        {
            m_HasCollision = true;
            m_LastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    m_LastCollisionNormal = contact.normal;
            }
        }


        void MoveVehicle(bool accelerate, bool brake, float turnInput)
        {

            float accelInput = (accelerate ? 1.0f : 0.0f) - (brake ? 1.0f : 0.0f);

            // manual acceleration curve coefficient scalar
            float accelerationCurveCoeff = 5;

            Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

            bool accelDirectionIsFwd = accelInput >= 0;
            bool localVelDirectionIsFwd = localVel.z >= 0;

            // use the max speed for the direction we are going--forward or reverse.
            float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.ReverseSpeed;
            float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.ReverseAcceleration;

            float currentSpeed = Rigidbody.velocity.magnitude;
            float accelRampT = currentSpeed / maxSpeed;
            float multipliedAccelerationCurve = m_FinalStats.AccelerationCurve * accelerationCurveCoeff;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

            // if we are braking (moving reverse to where we are going)
            // use the braking accleration instead
            float finalAccelPower = isBraking ? m_FinalStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

            // apply inputs to forward/backward
            float turningPower = IsDrifting ? m_DriftTurningPower : turnInput * m_FinalStats.Steer;

            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
            Vector3 fwd = turnAngle * transform.forward;
            Vector3 movement = fwd * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

            // forward movement
            bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

            // if over max speed, cannot accelerate faster.
            if (wasOverMaxSpeed && !isBraking)
                movement *= 0.0f;

            //The vehicle moves 
            Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
            newVelocity.y = Rigidbody.velocity.y;

            //newVelocity.z = zVelocity;
            //UnityEngine.Debug.Log("newVelocity is " + newVelocity);


            //  clamp max speed if we are on ground
            if (GroundPercent > 0.0f && !wasOverMaxSpeed)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            }

            // coasting is when we aren't touching accelerate
            if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
            {
                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * m_FinalStats.CoastingDrag);
            }

            Rigidbody.velocity = newVelocity;

            // Drift
            if (GroundPercent > 0.0f)
            {
                //if (m_InAir)
                //{
                //    m_InAir = false;
                //    Instantiate(JumpVFX, transform.position, Quaternion.identity);
                //}

                // manual angular velocity coefficient
                float angularVelocitySteering = 0.4f;
                float angularVelocitySmoothSpeed = 20f;

                // turning is reversed if we're going in reverse and pressing reverse
                if (!localVelDirectionIsFwd && !accelDirectionIsFwd)
                    angularVelocitySteering *= -1.0f;

                var angularVel = Rigidbody.angularVelocity;

                // move the Y angular velocity towards our target
                angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

                // apply the angular velocity
                Rigidbody.angularVelocity = angularVel;

                // rotate rigidbody's velocity as well to generate immediate velocity redirection
                // manual velocity steering coefficient
                float velocitySteering = 25f;

                // If the karts lands with a forward not in the velocity direction, we start the drift
                if (GroundPercent >= 0.0f && m_PreviousGroundPercent < 0.1f)
                {
                    Vector3 flattenVelocity = Vector3.ProjectOnPlane(Rigidbody.velocity, m_VerticalReference).normalized;
                    if (Vector3.Dot(flattenVelocity, transform.forward * Mathf.Sign(accelInput)) < Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad))
                    {
                        IsDrifting = true;
                        m_CurrentGrip = DriftGrip;
                        m_DriftTurningPower = 0.0f;
                    }
                }

                // Drift Management
                if (!IsDrifting)
                {
                    if ((WantsToDrift || isBraking) && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                    {
                        IsDrifting = true;
                        m_DriftTurningPower = turningPower + (Mathf.Sign(turningPower) * DriftAdditionalSteer);
                        m_CurrentGrip = DriftGrip;

                        ActivateDriftVFX(true);
                    }
                }

                if (IsDrifting)
                {
                    float turnInputAbs = Mathf.Abs(turnInput);
                    if (turnInputAbs < k_NullInput)
                        m_DriftTurningPower = Mathf.MoveTowards(m_DriftTurningPower, 0.0f, Mathf.Clamp01(DriftDampening * Time.fixedDeltaTime));

                    // Update the turning power based on input
                    float driftMaxSteerValue = m_FinalStats.Steer + DriftAdditionalSteer;
                    m_DriftTurningPower = Mathf.Clamp(m_DriftTurningPower + (turnInput * Mathf.Clamp01(DriftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

                    bool facingVelocity = Vector3.Dot(Rigidbody.velocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad);

                    bool canEndDrift = true;
                    if (isBraking)
                        canEndDrift = false;
                    else if (!facingVelocity)
                        canEndDrift = false;
                    else if (turnInputAbs >= k_NullInput && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                        canEndDrift = false;

                    if (canEndDrift || currentSpeed < k_NullSpeed)
                    {
                        // No Input, and car aligned with speed direction => Stop the drift
                        IsDrifting = false;
                        m_CurrentGrip = m_FinalStats.Grip;
                    }

                }

                // rotate our velocity based on current steer value
                Rigidbody.velocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * m_CurrentGrip * Time.fixedDeltaTime, transform.up) * Rigidbody.velocity;
            }
            else
            {
                // Check that all wheels are not touching the ground
                bool allWheelsInAir = true;
                if (Wheel_B.isGrounded || Wheel_F.isGrounded)
                {
                    allWheelsInAir = false;
                }

                // If all wheels are in the air, set InAir to true
                m_InAir = allWheelsInAir;
            }

            bool validPosition = false;
            if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, 3.0f, 1 << 9 | 1 << 10 | 1 << 11)) // Layer: ground (9) / Environment(10) / Track (11)
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > hit.normal.y) ? m_LastCollisionNormal : hit.normal;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime * (GroundPercent > 0.0f ? 10.0f : 1.0f)));    // Blend faster if on ground
            }
            else
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > 0.0f) ? m_LastCollisionNormal : Vector3.up;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime));
            }

            validPosition = GroundPercent > 0.7f && !m_HasCollision && Vector3.Dot(m_VerticalReference, Vector3.up) > 0.9f;

            // Airborne / Half on ground management
            if (GroundPercent < 0.7f)
            {
                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
                finalOrientationDirection.Normalize();
                if (finalOrientationDirection.sqrMagnitude > 0.0f)
                {
                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime)));
                }
            }
            else if (validPosition)
            {
                m_LastValidPosition = transform.position;
                m_LastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
            }

            ActivateDriftVFX(IsDrifting && GroundPercent > 0.0f);
        }

        private void ResetToRoadCenter()
        {
            Transform roadTransform = GetCurrentRoadTransform(); // 获取当前道路的 Transform
            if (roadTransform != null)
            {
                Vector3 roadCenter = roadTransform.position; // 道路中心点
                Quaternion roadDirection = roadTransform.rotation; // 道路方向

                // 判断轨道方向是否为负
                if (Vector3.Dot(roadTransform.forward, Vector3.forward) < 0)
                {
                    // 如果方向为负，调整朝向180度
                    roadDirection *= Quaternion.Euler(0, 180f, 0);
                }

                // 重置玩家位置和方向
                transform.position = roadCenter;
                transform.rotation = roadDirection;
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                // 如果无法获取当前道路信息，则回退到之前的保存状态
                RestorePlayerState();
            }
        }

        private Transform GetCurrentRoadTransform()
        {
            // The LayerMask of "Track" Layer is 11
            int trackLayer = 11;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 10f))
            {
                // Check if hit.transform belongs to "Track" Layer
                if (hit.transform.gameObject.layer == trackLayer)
                {
                    return hit.transform;
                }
            }
            return null;
        }

        private void SavePlayerState()
        {
            savedState.position = transform.position;
            savedState.rotation = transform.rotation;
            savedState.velocity = Rigidbody.velocity;
            savedState.angularVelocity = Rigidbody.angularVelocity;
        }

        private void RestorePlayerState()
        {
            transform.position = savedState.position;
            transform.rotation = savedState.rotation;
            Rigidbody.velocity = savedState.velocity;
            Rigidbody.angularVelocity = savedState.angularVelocity;
        }

        void ResetToNearestWaypoint()
        {
            Waypoint nearestWaypoint = FindNearestWaypoint();
            if (nearestWaypoint != null && nearestWaypoint.nextWaypoint != null)
            {
                // Set the player's position to the position of the nearest path node
                transform.position = nearestWaypoint.transform.position;

                // Set the player's orientation to the direction of the current node pointing to the next node
                Vector3 directionToNext = nearestWaypoint.nextWaypoint.transform.position - nearestWaypoint.transform.position;
                transform.rotation = Quaternion.LookRotation(directionToNext);

                // Reset speed and spin speed
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }
        }

        Waypoint FindNearestWaypoint()
        {
            Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
            Waypoint nearestWaypoint = null;
            float minDistance = float.MaxValue;

            foreach (Waypoint waypoint in allWaypoints)
            {
                float distance = Vector3.Distance(transform.position, waypoint.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestWaypoint = waypoint;
                }
            }

            return nearestWaypoint;
        }


    }
}
