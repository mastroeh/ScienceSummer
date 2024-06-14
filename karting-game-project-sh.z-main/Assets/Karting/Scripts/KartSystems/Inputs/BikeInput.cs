using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KartGame.KartSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class BikeInput : BaseInput
{
    public FTMS_IndoorBike indoorBike;
    public SocketReceiver dataReceiver; // Using SocketReceiver
    public XRController leftHandController;
    public XRController rightHandController;

    // Define the turn button press count counter
    public int leftTurnPressCount = 0;
    public int rightTurnPressCount = 0;

    // Variable used to detect button state changes
    private bool wasLeftButtonPressed = false;
    private bool wasRightButtonPressed = false;

    // Variable used to track when the turn button is pressed
    private float turnLeftDuration = 0f;
    private float turnRightDuration = 0f;
    private float maxTurnRate = 5f; // 最大转向率
    private float turnAcceleration = 1.5f; // 转向加速度

    // Record the longest and shortest turn button duration
    public float maxLeftTurnDuration = 0f;
    public float minLeftTurnDuration = float.MaxValue;
    public float maxRightTurnDuration = 0f;
    public float minRightTurnDuration = float.MaxValue;

    // Count the body turns
    public int bodyTurnCount = 0;
    public float maxBodyTurnInput = float.MinValue;
    public float minBodyTurnInput = float.MaxValue;

    //private InputDevice rightHandDevice;
    //private InputDevice leftHandDevice;

    public string TurnInputName = "Horizontal";
    public string AccelerateButtonName = "Accelerate";
    public string BrakeButtonName = "Brake";

    public float GetSteeringInput()
    {
        return GenerateInput().TurnInput;
    }

    //private void Awake()
    //{
    //    // 获取右手和左手手柄设备
    //    rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    //    leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    //}

    //public InputData BikeInput;

    //public override InputData GenerateInput()
    //{
    //    float bikeSpeed = indoorBike.speed;
    //    bool turnLeft = indoorBike.is_left_clicked;
    //    bool turnRight = indoorBike.is_right_clicked;
    //    //UnityEngine.Debug.Log("indoorBike.Speed in Bike Input.cs: " + indoorBike.speed);
    //    InputData inputData = new InputData
    //    {
    //        Brake = false,
    //        TurnInput = turnRight ? 1 : (turnLeft ? -1 : 0)
    //    };

    //    /* if (bikeSpeed > 0)
    //    {
    //        inputData.Accelerate = true;
    //    }
    //    else
    //    {
    //        inputData.Accelerate = false;
    //    } */

    //    // 为速度设置一个阈值
    //    float speedThreshold = 3f; // 举例，具体值根据需要调整

    //    // 只有速度超过阈值时才认为是有效的加速
    //    inputData.Accelerate = bikeSpeed > speedThreshold;

    //    //UnityEngine.Debug.Log("IndoorBike: " + value);
    //    return inputData;
    //    //return BikeInput;
    //}

    public override InputData GenerateInput()
    {
        indoorBike = FTMS_IndoorBike.Instance;
        float speedThreshold = 2f; // acceleration threshold

        InputData inputData = new InputData
        {
            Brake = false,
            Accelerate = false,
            TurnInput = 0
        };

        // Check current game mode
        GameModeManager.GameMode currentMode = GameModeManager.Instance.currentGameMode;

        // Game Mode Controller: Input controls using the Unity XR Interaction Toolkit
        if (currentMode == GameModeManager.GameMode.Controller)
        {
            // 读取右手摇杆输入
            Vector2 rightJoystick = rightHandController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickValue) ? joystickValue : Vector2.zero;

            // 读取左手Grip按钮状态作为刹车
            bool isLeftGripPressed = leftHandController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed) && gripPressed;

            // 设置转向和加速
            inputData.TurnInput = rightJoystick.x; // 使用摇杆左右移动来转向
            inputData.Accelerate = rightJoystick.y > 0.1f; // 摇杆向前移动来加速

            // 设置刹车
            inputData.Brake = isLeftGripPressed;
        }

        //// Game Mode 1: Use the Quest 2 Touch controller to control steering
        //if (currentMode == GameModeManager.GameMode.Mode1)
        //{
        //    //Input using Oculus Touch controllers
        //    //Vector2 rightJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);

        //    //Read the right-hand primary index trigger status
        //    bool isRightTriggerPressed = rightHandController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerPressed) && rightTriggerPressed;

        //    //Read the left-hand primary index trigger status
        //    bool isLeftTriggerPressed = leftHandController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerPressed) && leftTriggerPressed;

        //    //inputData.Accelerate = rightJoystick.y > 0.1f;
        //    inputData.TurnInput = isRightTriggerPressed ? 1 : (isLeftTriggerPressed ? -1 : 0);

        //    //Set acceleration, use bikeSpeed variable
        //    inputData.Accelerate = indoorBike.speed > speedThreshold;
        //}

        // Game Mode 2: Use the buttons on the bike to control
        else if (currentMode == GameModeManager.GameMode.Mode2 || currentMode == GameModeManager.GameMode.Mode1)
        {
            bool turnLeft = indoorBike.is_left_clicked;
            bool turnRight = indoorBike.is_right_clicked;

            // Update turn button press time
            if (turnLeft) {
                turnLeftDuration += Time.deltaTime;
                maxLeftTurnDuration = Mathf.Max(maxLeftTurnDuration, turnLeftDuration);
                minLeftTurnDuration = Mathf.Min(minLeftTurnDuration, turnLeftDuration);
                
                if (!wasLeftButtonPressed)
                {
                    leftTurnPressCount++;
                    wasLeftButtonPressed = true;
                }
            }
            else
            {
                wasLeftButtonPressed = false;
                turnLeftDuration = 0f;
            }

            if (turnRight)
            {
                turnRightDuration += Time.deltaTime;
                maxRightTurnDuration = Mathf.Max(maxRightTurnDuration, turnRightDuration);
                minRightTurnDuration = Mathf.Min(minRightTurnDuration, turnRightDuration);
                
                if (!wasRightButtonPressed)
                {
                    rightTurnPressCount++;
                    wasRightButtonPressed = true;
                }
            }
            else
            {
                wasRightButtonPressed = false;
                turnRightDuration = 0f;
            }

            // Calculate steering input
            float turnInput = 0f;
            if (turnLeft)
                turnInput -= Mathf.Min(turnLeftDuration * turnAcceleration, maxTurnRate);
            if (turnRight)
                turnInput += Mathf.Min(turnRightDuration * turnAcceleration, maxTurnRate);

            inputData.TurnInput = turnInput;

            //inputData.TurnInput = turnRight ? 1 : (turnLeft ? -1 : 0);

            // Set acceleration, use bikeSpeed variable
            inputData.Accelerate = indoorBike.speed > speedThreshold;
        }
        // Game Mode 3 and 4
        else if (currentMode == GameModeManager.GameMode.Mode3 || currentMode == GameModeManager.GameMode.Mode4)
        {
            // Data received from Python script
            float receivedData = dataReceiver != null ? -dataReceiver.GetReceivedData() : 0f;

            if (receivedData != 0)
            {
                bodyTurnCount++;
                maxBodyTurnInput = Mathf.Max(maxBodyTurnInput, Mathf.Abs(receivedData));
                minBodyTurnInput = Mathf.Min(minBodyTurnInput, Mathf.Abs(receivedData));
            }

            // Calculate the increment and adjust the sign according to the steering direction
            float turnIncrement = 0.05f * Time.deltaTime;
            if (receivedData < 0)
            {
                // Turn left
                turnIncrement = -turnIncrement;
            }

            // Update steering input, combining Python input value and time delta
            inputData.TurnInput = receivedData + turnIncrement;

            // Set acceleration, use bikeSpeed variable
            inputData.Accelerate = indoorBike.speed > speedThreshold;
        }

        // Game Mode Test: Use keyboard control
        else if (currentMode == GameModeManager.GameMode.Test)
        {
            inputData.Accelerate = Input.GetButton(AccelerateButtonName);
            inputData.Brake = Input.GetButton(BrakeButtonName);
            inputData.TurnInput = Input.GetAxis("Horizontal");
        }

        return inputData;
    }


}