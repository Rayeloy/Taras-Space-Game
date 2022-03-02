using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJetpack : MonoBehaviour
{
    /// <summary>
    /// Exist the possibilty than in some levels we can't use the jetpack
    /// </summary>
    public bool stoleJetpackBool;

    public float totalFuel = 100;
    [Range(0, 1)]
    public float jetpackMinFuelToStartPercent = 0.3f;
    public float initialConsumption = 10;

    public float jetpackForce = 10;
    public float jetpackMaxSpeed = 20;
    public float jetpackConsumptionPerTick = 5;
    public float jetpackConsumptionFrequency=0.1f;
    private float jetpackConsumptionCurrentTime = 0;

    public float jetpackRefillPerTick = 5;
    public float jetpackRefillFrequency = 0.5f;

    [HideInInspector]
    public float currentFuel = 100;

    private bool isJetpackIgnitiatedBool = false;

    public ParticleSystem jetpackParticles;
    public GameObject jetpackLight;


    private PlayerMovementCMF myPlayerMov;

    public void KonoAwake()
    {
        myPlayerMov = GetComponent<PlayerMovementCMF>();
        currentFuel = totalFuel;
        jetpackLight.SetActive(false);

    }

    public void KonoStart()
    {
        JetpackAvailable(stoleJetpackBool);
    }

    public void KonoUpdate()
    {
        ProcessJetpack();
    }

    public bool canUseJetpack
    {
        get
        {
            return (!myPlayerMov.jumpInsurance && (!myPlayerMov.collCheck.below || myPlayerMov.collCheck.sliping)) && currentFuel >= totalFuel * jetpackMinFuelToStartPercent && !stoleJetpackBool;
        }
    }



    public void JetpackAvailable(bool isNotAvailableBool)
    {
        if(isNotAvailableBool)
            myPlayerMov.myPlayerAnimations.ActivateAttatchment("jetpack_png", null);
        else
            myPlayerMov.myPlayerAnimations.ActivateAttatchment("jetpack_png", "jetpack_png");


    }


    bool AddFuel(float consumption)
    {
        currentFuel += consumption;
        currentFuel = Mathf.Clamp(currentFuel, 0, totalFuel);
        //Debug.Log("Adding fuel: " + consumption + "; currentFuel = " + currentFuel);
        return currentFuel > 0;
    }

    public void StartJetpack()
    {
        if(!isJetpackIgnitiatedBool && canUseJetpack)
        {
            isJetpackIgnitiatedBool = true;
            AddFuel(-initialConsumption);
            myPlayerMov.StopJump();
            myPlayerMov.vertMovSt = VerticalMovementState.Jetpack;
            jetpackConsumptionCurrentTime = 0;
            //Debug.Log("START JETPACK");
            jetpackParticles.Play(true);
            jetpackLight.SetActive(true);

        }
    }

    void ProcessJetpack()
    {
        if (isJetpackIgnitiatedBool)
        {
            if (currentFuel == 0 || (myPlayerMov.collCheck.below && !myPlayerMov.collCheck.sliping))
            {
                EndJetpack();
            }
            else
            {
                if (jetpackConsumptionCurrentTime >= jetpackConsumptionFrequency)
                {
                    AddFuel(-jetpackConsumptionPerTick);
                    jetpackConsumptionCurrentTime = 0;
                }
                jetpackConsumptionCurrentTime += Time.deltaTime;
            }
        }
        else//refilling
        {
            if (currentFuel < totalFuel && myPlayerMov.collCheck.below && !myPlayerMov.collCheck.sliping)
            {
                if (jetpackConsumptionCurrentTime >= jetpackRefillFrequency)
                {
                    jetpackConsumptionCurrentTime = 0;
                    AddFuel(jetpackRefillPerTick);
                }
                jetpackConsumptionCurrentTime += Time.deltaTime;
            }
        }
    }

    public void EndJetpack()
    {
        if (isJetpackIgnitiatedBool)
        {
            //Debug.Log("END JETPACK");
            isJetpackIgnitiatedBool = false;
            myPlayerMov.vertMovSt = VerticalMovementState.None;//Is this correct?
            jetpackConsumptionCurrentTime = 0;
            jetpackParticles.Stop(true);
            jetpackLight.SetActive(false);

        }
    }


    public void Enter_JetpackCollision(ColliderBridge brCol)
    {
        Debug.Log("entro");
        if (stoleJetpackBool)
        {
            stoleJetpackBool = false;
            JetpackAvailable(stoleJetpackBool);
            brCol.gameObject.SetActive(false);
        }
            
    }
}
