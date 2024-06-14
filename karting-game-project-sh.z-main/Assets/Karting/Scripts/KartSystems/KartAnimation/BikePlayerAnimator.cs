using UnityEngine;
using UnityEngine.Assertions;

namespace KartGame.KartSystems 
{
    public class BikePlayerAnimator : MonoBehaviour
    {
        public Animator PlayerAnimator;
        public ArcadeBike Bike;

        public string SteeringParam = "Steering";
        public string GroundedParam = "Grounded";

        int m_SteerHash, m_GroundHash;
        float steeringSmoother;

        void Awake()
        {
            Assert.IsNotNull(Bike, "No ArcadeBike found!");
            Assert.IsNotNull(PlayerAnimator, "No PlayerAnimator found!");
            m_SteerHash  = Animator.StringToHash(SteeringParam);
            m_GroundHash = Animator.StringToHash(GroundedParam);
        }

        void Update()
        {
            SegmentTrack segmentTrack = GetComponent<SegmentTrack>();
            if (segmentTrack != null)
            {
                float resistance = segmentTrack.resistance;
                int grade = segmentTrack.grade;

            }

            steeringSmoother = Mathf.Lerp(steeringSmoother, Bike.Input.TurnInput, Time.deltaTime * 5f);
            PlayerAnimator.SetFloat(m_SteerHash, steeringSmoother);
            // If 2 wheels are above the ground then we consider that the kart is airbourne.
            PlayerAnimator.SetBool(m_GroundHash, Bike.GroundPercent >= 0.5f);
        }
    }
}
