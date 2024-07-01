using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.ProBuilder;

public class WeaponsManager : MonoBehaviour
{
    public List<GameObject> firedPrimaryProjectiles = new List<GameObject>();

    [Header("Primary Settings")]
    [SerializeField] GameObject primaryProjectile;
    [SerializeField] bool pHasCooldown;
    public float pCoolDown;
    public float primaryStartup;
    public float primaryEndlag;
    [SerializeField] int primaryProjectileForce;
    public int primaryProjectileCount;
    [SerializeField] private float primaryTimeBetweenShots;
    [SerializeField] float pChargeCost;
    [SerializeField] float pChargeGain = 10f;

    public bool isShotgun;
    [SerializeField] float shotgunSpread;

    [Header("Secondary Settings")]
    [SerializeField]GameObject secondaryProjectile;
    [SerializeField] bool sHasCooldown;
    public float sCoolDown;
    public float secondaryStartup;
    public float secondaryEndlag;
    [SerializeField] int secondaryProjectileForce;
    public int secondaryProjectileCount;
    [SerializeField] private float secondaryTimeBetweenShots;
    [SerializeField] float sChargeCost;
    [SerializeField] float sChargeGain = 10f;

    [Header("Misc Settings")]
    public Transform firePosition;
    private StateManager stateManager;
    private CharacterController controller;
    private PlayerStats playerStats;
    private PlayerController pController;
    public float stopTime;
    private float pCurTime;
    public float sCurTime;
    public float continueMovementSpeed;

    Transform opponentT;
    IEnumerator lastAttackCR = null;

    public bool checkSecondaryFire;
    bool isShooting;

    private AudioMgrRobo audioMgrRobo;
    private void Awake()
    {
        stateManager = GetComponent<StateManager>();
        controller = GetComponent<CharacterController>();
        playerStats = GetComponentInParent<PlayerStats>();
        pController = GetComponentInParent<PlayerController>();
        isShooting = false;
        audioMgrRobo = GetComponentInChildren<AudioMgrRobo>();
    }

    private void FixedUpdate()
    {
        pCurTime = pCurTime + Time.deltaTime;
        sCurTime =  sCurTime + Time.deltaTime;
    }

    public void PrimaryFire()
    {
        if (!stateManager.isStunned && !stateManager.isBusy && !stateManager.isDowned && isShooting == false)
        {
            isShooting= true;
            stateManager.isBusy = true;
            playerStats.ChangeCharge(pChargeGain);
            pController.DashStopper();
            pController.JumpStopper();
            audioMgrRobo.PlayPrimaryAttack();
            IEnumerator coroutine = BasicAttackCoroutine
                (primaryProjectile, primaryProjectileForce, primaryStartup, primaryEndlag, primaryProjectileCount, primaryTimeBetweenShots, pChargeCost, false);
            StartCoroutine(coroutine);
            lastAttackCR = coroutine;

            /*if (GameManager.instance.audioMgr.primaryAttack.isPlaying == false)
            {
                GameManager.instance.audioMgr.PlayPrimary();
            }*/
        }
    }

    public void SecondaryFire()
    {
        if (!stateManager.isStunned && !stateManager.isBusy && sCurTime > sCoolDown && !stateManager.isDowned && isShooting == false)
        {
            isShooting= true;
            stateManager.isBusy = true;
            playerStats.ChangeCharge(sChargeGain);
            sCurTime = 0;
            audioMgrRobo.PlaySecondaryAttack();
            IEnumerator coroutine = BasicAttackCoroutine
                (secondaryProjectile, secondaryProjectileForce, secondaryStartup, secondaryEndlag, secondaryProjectileCount, secondaryTimeBetweenShots, sChargeCost, true);
            StartCoroutine(coroutine);
            lastAttackCR = coroutine;

            checkSecondaryFire = true;
            /*if (GameManager.instance.audioMgr.primaryAttack.isPlaying == false)
            {
                GameManager.instance.audioMgr.PlaySecondary();
            }*/
        }
    }

    private IEnumerator BasicAttackCoroutine(GameObject _projectile, int _force, float _startup, float _endlag, int _projCount, float _timeBetweenShots, float _chargeCost, bool _secondary)
    {
        Debug.Log("Attack Started");
        pController.animC.ShootingStart();
        pController.animCH.ShootingStart();
        
        if (stateManager.isGrounded != true) 
            { pController.airShotCount++; }
        
        playerStats.charge = playerStats.charge - _chargeCost;
        float movetime = _startup + (_projCount * _timeBetweenShots);
        if (stateManager.isGrounded && stateManager.isMoving)
        {
            IEnumerator coroutine = ContinueMovement(movetime);
            StartCoroutine(coroutine);
        } 
        yield return new WaitForSeconds(_startup); // Startup time of the weapon
        for (int i = 0; i < _projCount; i++)
        {
            
            if (!isShotgun)
            {
                FireProjectile(_projectile, _force, _secondary);
                yield return new WaitForSeconds(_timeBetweenShots);
            }
            else
            {
                FireShotgun(_projectile, _force, _secondary);
            }

            
        }

        if (!stateManager.isGrounded)
        {
            pController.airYStopper = false;
        }

        if (stateManager.isGrounded != true)
        {
            yield return new WaitForSeconds((_endlag + stopTime) / 2); //Set 'end lag' on attack, making the player stop moving and not giving back control until the end lag is over

        } else
        {
            yield return new WaitForSeconds(_endlag + stopTime); //Set 'end lag' on attack, making the player stop moving and not giving back control until the end lag is over
        }

        pController.curPlayerSpeed = 0;
        pController.SwitchModelFacing("Main");
        stateManager.isBusy = false;
        isShooting= false;
        //Debug.Log("Attack Ended");
    }

    public void StopAttackCoroutine()
    {
        if (lastAttackCR != null)
        {
            StopCoroutine(lastAttackCR); // This isnt working?
            pController.SwitchModelFacing("Main");

            stateManager.isBusy = false;
            isShooting = false;
            Debug.Log("Basic Attack Stopped");
        }

    }

    private void FireProjectile(GameObject _projectile, float _force, bool _secondary)
    {
        opponentT = GetComponentInChildren<PlayerHeadSwivel>().targetTransform;
        GameObject curProj = Instantiate(_projectile, firePosition.position, firePosition.rotation);
        Rigidbody rb = curProj.GetComponent<Rigidbody>();
        if (_projectile == primaryProjectile ) 
        {
            firedPrimaryProjectiles.Add(curProj);
        }
        if (!_secondary)
        {
            rb.GetComponent<Projectile>().Init(this.gameObject.GetComponentInParent<PlayerStats>(), opponentT);
        }
        //Debug.Log("Projectile Fired");
        rb.AddForce(firePosition.forward * _force, ForceMode.Impulse);
    }

    private void FireShotgun(GameObject _projectile, float _force, bool _secondary)
    {
        opponentT = GetComponentInChildren<PlayerHeadSwivel>().targetTransform;
        GameObject curProj = Instantiate(_projectile, firePosition.position, firePosition.rotation);
        Rigidbody rb = curProj.GetComponent<Rigidbody>();

        float rz = UnityEngine.Random.Range(-shotgunSpread, shotgunSpread);
        float rx = UnityEngine.Random.Range(-shotgunSpread, shotgunSpread);
        if (isShotgun) rb.AddForce(new Vector3(transform.forward.x + rx, 0, transform.forward.z + rz) * primaryProjectileForce);
        if (_projectile == primaryProjectile)
        {
            firedPrimaryProjectiles.Add(curProj);
        }
        if (!_secondary)
        {
            rb.GetComponent<Projectile>().Init(this.gameObject.GetComponentInParent<PlayerStats>(), opponentT);
        }
        //Debug.Log("Projectile Fired");
        rb.AddForce(firePosition.forward * _force, ForceMode.Impulse);
    }

    private IEnumerator ContinueMovement(float moveTime)
    {
        float starttime = Time.time;
        Vector3 moveDerection = transform.forward * 3;
        while (starttime + moveTime > Time.time && !stateManager.isStunned)
        {
            if (!stateManager.isGrounded)
            {
                StopCoroutine("ContinueMovement");
                yield return null;
            }
            controller.Move(moveDerection * Time.deltaTime * ((continueMovementSpeed / 2) * (pController.curPlayerSpeed/pController.maxPlayerSpeed)));
            yield return null;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        //PlayerStats targetStats = collision.collider.gameObject.GetComponentInParent<PlayerStats>();
        ////Debug.Log(targetStats);
        //if (targetStats)
        //{
        //    if (isMelee)
        //    {
        //        Debug.Log("Melee Hit");
        //        StopCoroutine("MeleeCoroutine");
        //        Vector2 knockbackVector = new Vector2(targetStats.transform.position.x - this.transform.position.x, targetStats.transform.position.z - this.transform.position.z);
        //        targetStats.TakeDamage(meleeDamage, true, knockbackVector);
        //        sword.SetActive(false);
        //        stateManager.isBusy = false;
        //        isMelee = false;
        //    }

        //}
    }
}
