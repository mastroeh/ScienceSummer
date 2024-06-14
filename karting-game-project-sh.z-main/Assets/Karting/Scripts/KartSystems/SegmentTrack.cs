using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KartGame.KartSystems
{
    public class SegmentTrack : MonoBehaviour
    {
        public float resistance = 1.0f;
        public int grade; 
        public float WindSpeed = 0f;

        private void Update()
        {
            // Get the Transform component of the track object
            Transform trackTransform = transform;

            // Get the X-axis rotation value of the track
            float xRotation = trackTransform.rotation.eulerAngles.x;

            //Map the X-axis rotation value into the range of -20 to 20 and round it to an integer
            if (xRotation > 180)
            {
                xRotation -= 360; //Convert a negative number greater than 180 degrees to a negative number less than 180 degrees
            }
            grade = Mathf.RoundToInt(Mathf.Clamp(xRotation, -20f, 20f));
        }
    }
}
