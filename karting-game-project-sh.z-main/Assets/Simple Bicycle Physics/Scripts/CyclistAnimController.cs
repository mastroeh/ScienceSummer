using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using KartGame.KartSystems;

namespace SBPScripts
{
public class CyclistAnimController : MonoBehaviour
{
    ArcadeBike bicycleController;
    Rigidbody bicycleControllerRb;
    Animator anim;
    string clipInfoCurrent, clipInfoLast;
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public bool isAirborne;

    public GameObject hipIK, chestIK, leftFootIK, leftFootIdleIK, headIK;
    void Start()
    {
        bicycleController = FindObjectOfType<ArcadeBike>();
        bicycleControllerRb = bicycleController.GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        leftFootIK.GetComponent<TwoBoneIKConstraint>().weight = 0;
        chestIK.GetComponent<TwoBoneIKConstraint>().weight = 0;
        hipIK.GetComponent<MultiParentConstraint>().weight = 0;
        headIK.GetComponent<MultiAimConstraint>().weight = 0;
    }

    void Update()
    {
        speed = bicycleController.transform.InverseTransformDirection(bicycleControllerRb.velocity).z;
        isAirborne = bicycleController.m_InAir;
        anim.SetFloat("Speed", speed);
        anim.SetBool("isAirborne", isAirborne);
        if(!bicycleController.m_InAir)
        {clipInfoCurrent = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        if (clipInfoCurrent == "IdleToStart" && clipInfoLast == "Idle")
            StartCoroutine(LeftFootIK(0));
        if (clipInfoCurrent == "Idle" && clipInfoLast == "IdleToStart")
            StartCoroutine(LeftFootIK(1));
        if(clipInfoCurrent == "Idle" && clipInfoLast == "Reverse")
            StartCoroutine(LeftFootIdleIK(0));
        if(clipInfoCurrent == "Reverse" && clipInfoLast == "Idle")
            StartCoroutine(LeftFootIdleIK(1));

        clipInfoLast = clipInfoCurrent;}
    }

    IEnumerator LeftFootIK(int offset)
    {
        float t1 = 0f;
        while (t1 <= 1f)
        {
            t1 += Time.fixedDeltaTime;
            leftFootIK.GetComponent<TwoBoneIKConstraint>().weight = Mathf.Lerp(-0.05f,1.05f, Mathf.Abs(offset - t1));
            leftFootIdleIK.GetComponent<TwoBoneIKConstraint>().weight = 1 - leftFootIK.GetComponent<TwoBoneIKConstraint>().weight;         
            yield return null;
        }

    }
    IEnumerator LeftFootIdleIK(int offset)
    {
        float t1 = 0f;
        while (t1 <= 1f)
        {
            t1 += Time.fixedDeltaTime;
            leftFootIdleIK.GetComponent<TwoBoneIKConstraint>().weight = Mathf.Lerp(-0.05f,1.05f, Mathf.Abs(offset - t1));
            yield return null;
        }

    }
}
}
