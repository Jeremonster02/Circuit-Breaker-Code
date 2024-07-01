using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Video;

public class StateManager : MonoBehaviour
{
    public bool isGrounded;
    public bool isStunned;
    public bool isBusy;
    public bool isMoving;
    public bool isDowned;
    public bool isPostDowned;
    public bool isOverclockActive;
    public bool overclockCharge;
    public bool tankNukeAiming;
    [SerializeField] float downedLength;
    [SerializeField] float postDownedLength;
    public float downedDamage;
    [SerializeField] float downedHealthMod;
    [SerializeField] float hitstunLength;

    CharacterController controller;
    public PlayerController pc;
    PlayerStats playerStats;

    [Header("Placeholder Effects")]
    [SerializeField] GameObject downedStars;
    [SerializeField] GameObject hitstunMarker;
    //[SerializeField] VideoPlayer stunPortrait;
    [SerializeField] Material playerArmsMat;
    [SerializeField] Material playerLegsMat;
    [SerializeField] Material playerTorsoMat;
    [SerializeField] Material playerPostDownedArms;
    [SerializeField] Material playerPostDownedLegs;
    [SerializeField] Material playerPostDownedTorso;
    [SerializeField] GameObject playerBody;
    private SkinnedMeshRenderer playerSMR;

    [Header("Power Up Bools")]
    public bool attackPU;
    public bool defencePU;
    public bool movePU;
    public bool chargePU;
    public bool healthPU;

    // Start is called before the first frame update
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        playerStats = GetComponentInParent<PlayerStats>();
        playerSMR = playerBody.GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isGrounded = controller.isGrounded;

        //if (downedDamage >= (GetComponent<PlayerStats>().maxHealth * .2f) && !isDowned && !isPostDowned)
        //{
        //    Debug.Log("You Are Downed!");
        //    StartCoroutine("DownedState");
        //}
        //downedDamage -= .015f;
        //if (downedDamage < 0)
        //{
        //    downedDamage = 0;
        //}
    }

    public void StartHitStun()
    {
        StartCoroutine("HitStun");
    }

    IEnumerator HitStun()
    {
        if (!isDowned && !isPostDowned && playerStats.meleeScr.isMelee != true)
        {
            isStunned = true;
            pc.animC.HitstunStart();
            hitstunMarker.SetActive(true);
            //stunPortrait.Play();
            yield return new WaitForSeconds(hitstunLength);
            pc.airYStopper = false;
            isStunned = false;
            hitstunMarker.SetActive(false);
        }
    }

    public void startDownedState()
    {
        if (!isDowned)
        {
            Debug.Log("You Are Downed!");
            StartCoroutine("DownedState");
        }
    }

    IEnumerator DownedState()
    {
        isDowned = true;
        downedStars.SetActive(true);
        
        playerStats.downedHealthModifier = downedHealthMod;
        yield return new WaitForSeconds(downedLength);
        downedStars.SetActive(false);
        playerStats.downedMeter = 0;
        isDowned = false;

        StartCoroutine("PostDownedState");
    }

    IEnumerator PostDownedState()
    {
        isPostDowned = true;
        SetPlayerAlpha();
        playerStats.downedHealthModifier = 0f;
        yield return new WaitForSeconds(postDownedLength);
        playerStats.downedHealthModifier = 1f;
        isPostDowned = false;
        playerStats.downedMeter = 0;
        SetPlayerAlpha();
    }

    public void SetPlayerAlpha()
    {
        // Temp array for SMR
        Material[] materials = playerSMR.materials;

        if (isPostDowned == true)
        {
            //Debug.Log("Player Alpha Changed to post downed");
            materials[0] = playerPostDownedArms;
            materials[1] = playerPostDownedLegs;
            materials[2] = playerPostDownedTorso;
        }
        else
        {
            //Debug.Log("Player Alpha Changed to normal");
            materials[0] = playerArmsMat;
            materials[1] = playerLegsMat;
            materials[2] = playerTorsoMat;
        }
        playerSMR.materials = materials;
    }

    public void StartPowerup(PowerUpType _puType, float _puLength)
    {
        IEnumerator puCoRout = PowerupCoroutine(_puType, _puLength);
        StartCoroutine(puCoRout);
    }

    public IEnumerator PowerupCoroutine(PowerUpType _puType, float _puLength)
    {
        switch(_puType)
        {
            case PowerUpType.Attack:
                attackPU = true;
                yield return new WaitForSeconds(_puLength);
                EndPowerup(_puType);
                break;
            case PowerUpType.Defense:
                defencePU = true;
                yield return new WaitForSeconds(_puLength);
                EndPowerup(_puType);
                break;
            case PowerUpType.Movement:
                movePU = true;
                yield return new WaitForSeconds(_puLength);
                EndPowerup(_puType);
                break;
        }
    }

    public void EndPowerup(PowerUpType _puType)
    {
        switch (_puType)
        {
            case PowerUpType.Attack:
                attackPU = false;
                break;
            case PowerUpType.Defense:
                defencePU = false ;
                break;
            case PowerUpType.Movement:
                movePU = false;
                break;
        }
    }
}
