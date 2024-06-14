import cv2
import mediapipe as mp
# import pyautogui

import pyrealsense2 as rs
import numpy as np
import socket

pose = None
mp_pose = None
mp_drawing = None

# Two potential input sources for steering
#distance_to_center = True
#shoulder_angle = False
distance_to_center = False
shoulder_angle = True
gradient_threshold = 0.05
# Create a socket object
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Connect to Unity application server
try:
    client_socket.connect(('127.0.0.1', 8080))
except WindowsError as win_error:
    print(f"{win_error}")
def initialize_mediapipe():
    global pose, mp_pose, mp_drawing
    mp_pose = mp.solutions.pose
    pose = mp_pose.Pose()
    mp_drawing = mp.solutions.drawing_utils
def get_video():
    # Initialize the webcam (0 is the default camera)
    #cap = cv2.VideoCapture(0)

    # Configure and initialize the RealSense Cam
    pipeline = rs.pipeline()
    config = rs.config()
    config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)
    pipeline.start(config)

    try:
        while True:
            # Get frames from RealSense camera
            frames = pipeline.wait_for_frames()
            color_frame = frames.get_color_frame()
            if not color_frame:
                continue

            # Convert frames to NumPy array
            frame = np.asanyarray(color_frame.get_data())

            # Track human posture
            frame = track_human(frame)

            # Show results
            cv2.imshow('RealSense', frame)

            # Press the 'q' key to exit the loop
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    finally:
        # Stop camera stream
        pipeline.stop()
        cv2.destroyAllWindows()

def track_human(frame):
    frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # Process the frame for pose detection
    results = pose.process(frame_rgb)

    # Draw the pose annotations on the frame
    if results.pose_landmarks:
        # mp_drawing.draw_landmarks(frame, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)

        if distance_to_center:
            nose = results.pose_landmarks.landmark[mp_pose.PoseLandmark.NOSE]
            nose_x = int(nose.x * frame.shape[1])
            cv2.circle(frame, (nose_x, 20), 10, (0, 0, 255), -1)
            get_distance_to_center(nose_x, frame.shape[1])

        if shoulder_angle:
            left_shoulder = results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_SHOULDER]
            left_shoulder_x, left_shoulder_y = int(left_shoulder.x * frame.shape[1]), int(left_shoulder.y * frame.shape[0])
            cv2.circle(frame, (left_shoulder_x, left_shoulder_y), 10, (0, 0, 255), -1)
            right_shoulder = results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_SHOULDER]
            right_shoulder_x, right_shoulder_y = int(right_shoulder.x * frame.shape[1]), int(right_shoulder.y * frame.shape[0])
            cv2.circle(frame, (right_shoulder_x, right_shoulder_y), 10, (0, 0, 255), -1)
            cv2.line(frame,(left_shoulder_x, left_shoulder_y),(right_shoulder_x, right_shoulder_y),(255,255,255),5,-1)
            get_shoulder_angle(left_shoulder_x, left_shoulder_y, right_shoulder_x, right_shoulder_y)

    return frame

def get_distance_to_center(x, width):
    center = int(width/2)
    distance = int(((x-center)/center)*5)
    print(distance)

    # TODO: transmit value to Unity and process it there to steer
    #print("Attempting to send data...")
    send_data_to_unity(distance)

    # pyautogui.moveRel(distance, 0)

def get_shoulder_angle(x1, y1, x2, y2):
    try:
        angle = ((y2-y1)/(x2-x1))
        print(angle)
    except ZeroDivisionError as zerodiv_error:
        print(f"{zerodiv_error}")
        angle = 0
    # TODO: transmit value to Unity and process it there to steer
    #print("Attempting to send data...")
    if abs(angle)>=gradient_threshold:
        send_data_to_unity(angle)
    else:
        send_data_to_unity(0)

def send_data_to_unity(data):
    try:
        # 准备发送的数据
        data_to_send = str(data).encode()
        print("Data to send:", data_to_send)  # 打印字节数据

        # 发送数据
        client_socket.sendall(data_to_send)
        print("Data is sent")
    except socket.error as socket_error:
        print(f"Socket error: {socket_error}")
    except Exception as error:
        print(f"{error}")


if __name__ == '__main__':
    initialize_mediapipe()
    get_video()


"""
    # Check if the webcam is opened correctly
    if not cap.isOpened():
        raise IOError("Cannot open webcam")

    while True:
        # Capture frame-by-frame
        ret, frame = cap.read()

        # If frame is read correctly, ret is True
        if not ret:
            break

        frame = track_human(frame)

        # Display the resulting frame
        cv2.imshow('Webcam Feed', frame)

        # Break the loop with the 'ESC' key
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    # Release the capture and close any open windows
    cap.release()
    cv2.destroyAllWindows() 
"""