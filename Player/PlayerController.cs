using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    public Vector3 playerVelocity;
    public StateManager stateManager;
    private int airDashCount = 0;
    public int airShotCount = 0;
    [SerializeField] Vector3 move;
    public bool airYStopper;
    public bool isPostDash;
    public int playerIdx;
    public bool isMoving;

    [Header("Model Settings")]
    public Transform headTransform;
    public animController animC;
    public animController animCH;
    [SerializeField] SkinnedMeshRenderer modelMain;
    [SerializeField] SkinnedMeshRenderer modelHead;

    [Header("Run Settings")]
    [SerializeField] public float curPlayerSpeed;
    [SerializeField] private float playerAcceleration;
    [SerializeField] private float playerDeceleration;
    public float maxPlayerSpeed = 10.0f; // Used in place of base player speed for now
    public float minPlayerSpeed = 1f;

    [Header("Dash Settings")]
    [SerializeField] private float airDashSpeed = 5f;
    [SerializeField] private float airDashVert = 0f;
    [SerializeField] private float dashTime;
    [SerializeField] int maxAirDash = 2;

    [Header("Misc Settings")]
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float jumpWait = .1f;
    [SerializeField] private float baseGravityValue;
    [SerializeField] private float landingLag;
    private float curGravityValue;
    public float playerDashMod;

    private AudioMgrRobo audioMgrRobo;
    public void Init(int _Idx)
    {
        playerIdx = _Idx;
        CameraManager.instance.players[playerIdx] = this.gameObject;

    }

    private void Awake()
    {
        controller = gameObject.GetComponent<CharacterController>();
        stateManager = GetComponent<StateManager>();
        curGravityValue = baseGravityValue;
        audioMgrRobo = GetComponentInChildren<AudioMgrRobo>();
        //playerStats = GetComponent<PlayerStats>();
    }

    void FixedUpdate()
    {

        if (stateManager.isGrounded && playerVelocity.y < 0) // Landing Lag Check
        {
            playerVelocity.y = 0f;
            airDashCount = 0;
            if (airShotCount > 0)
            {
                StartCoroutine("LandingLag");
            }
        }

        if (stateManager.isGrounded && !stateManager.isStunned && !stateManager.isBusy && !stateManager.isDowned) //Grounded movement handler
        {
            if (!stateManager.movePU) 
            {
                controller.Move(move * Time.deltaTime * curPlayerSpeed);
            } else
            {
                controller.Move(move * Time.deltaTime * curPlayerSpeed * PowerUpEffects.instance.movementPUMult);
            }
            
        }

        if (move != Vector3.zero && !stateManager.isOverclockActive) // Player acceleration script
        {
            gameObject.transform.forward = move;
            stateManager.isMoving = true;

            if (curPlayerSpeed != maxPlayerSpeed)
            {
                if (curPlayerSpeed < maxPlayerSpeed)
                {
                    curPlayerSpeed = curPlayerSpeed + (playerAcceleration * Time.deltaTime);
                    if (curPlayerSpeed > maxPlayerSpeed)
                    { curPlayerSpeed = maxPlayerSpeed; }
                }
            }
        }
        else
        {
            stateManager.isMoving = false;
            if (curPlayerSpeed > 0)
            { curPlayerSpeed = curPlayerSpeed + (playerDeceleration * Time.deltaTime); }
        }

        // Changes the height position of the player..

        if (airYStopper == false) // Handles Gravity
        {
            if (stateManager.isGrounded) 
            {
                playerVelocity.y += curGravityValue * 10 * Time.deltaTime; // Increase gravity if player is grounded
            } else
            {
                playerVelocity.y += curGravityValue * Time.deltaTime;
            }
            
        }
        else // turn off player's y movement
        {
            playerVelocity.y = 0f;
        }

        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 movement = ctx.ReadValue<Vector2>();
        move = new Vector3(movement.x, 0, movement.y);
        //Debug.Log("Move Magnatude = " + move.magnitude);

        if (move.magnitude > .05f /*move.x >= .05f || move.z <= -.05f*/)
        {
           if (stateManager.isGrounded == true)
            {
                //if (GameManager.instance.audioMgr.walk.isPlaying == false && GameManager.instance.audioMgr.walk != null)
                //{
                //    GameManager.instance.audioMgr.PlayWalk();
                //}
            }
            
            
        }
    }

    public void OnJump()
    {
        if (stateManager.isGrounded && !stateManager.isBusy && !stateManager.isStunned && !stateManager.isDowned&& !stateManager.overclockCharge && !stateManager.isOverclockActive)
        {
            StartCoroutine("JumpStart");
        }
        else if(stateManager.overclockCharge && GetComponent<PlayerStats>().playerType == PlayerType.LR)
        {
            GetComponent<LROverclock>().StartOverclock();
        }
    }
    private IEnumerator JumpStart()
    {
        //Debug.Log("Jump Started");
        yield return new WaitForSeconds(jumpWait);
        //stateManager.isGrounded = false;
        audioMgrRobo.PlayJump();
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * curGravityValue);
        StartCoroutine("JumpMomentum");
    }

    private IEnumerator JumpMomentum()
    {
        if(move != Vector3.zero)
        {         
            float momentum = move.magnitude;
            Vector3 moveDirection = transform.forward * momentum;
            yield return new WaitForSeconds(.03f);
            while (!stateManager.isGrounded && !airYStopper)
            {
                controller.Move(moveDirection * Time.deltaTime * momentum*(curPlayerSpeed/2f));
                yield return null;
            }
            
        }
    }

    public void AirDash()
    {
        if (airDashCount < maxAirDash && !stateManager.isGrounded && !stateManager.isDowned && !stateManager.isStunned && stateManager.isBusy == false 
         || airDashCount < maxAirDash && !stateManager.isGrounded && !stateManager.isDowned && !stateManager.isStunned && stateManager.movePU == true)
        {
            if (airYStopper || isPostDash)
            {
                DashStopper();
            }
            StartCoroutine("DashCoroutine");
            airDashCount++;
            GameManager.instance.audioMgr.PlayAirDash();
            
        }
    }

    private IEnumerator DashCoroutine()
    {
        //Debug.Log("Dash Started");
        airYStopper = true;
        animC.AirdashStart();

        float starttime = Time.time;
        Vector3 moveDirection = transform.forward * airDashSpeed;
        Vector3 vertMoveDirection = transform.up * airDashVert;
        Vector3 totalDirection = (moveDirection /*+ vertMoveDirection*/) * move.magnitude;

        
        while (starttime + dashTime > Time.time)
        {
            if (stateManager.isGrounded)
            {
                airYStopper = false;
                yield break;
            }
            controller.Move(totalDirection * Time.deltaTime * (airDashSpeed / 2));
            //controller.Move(vertMoveDirection * Time.deltaTime);
            yield return null;
        }
        airYStopper = false;
        isPostDash = true;
        playerVelocity.y += Mathf.Sqrt(airDashVert * -3.0f * curGravityValue);
        while (!stateManager.isGrounded)
        {
            if (stateManager.isGrounded)
            { 
                yield break;
            }
            controller.Move(totalDirection * Time.deltaTime * (airDashSpeed / 3.3f));
            //controller.Move(vertMoveDirection * Time.deltaTime);
            yield return null;
        }
        isPostDash = false;
        //Debug.Log("Dash Finished");
    }

    private IEnumerator LandingLag()
    {
        stateManager.isBusy= true;
        airShotCount = 0;
        yield return new WaitForSeconds(landingLag);
        stateManager.isBusy = false;
    }

    public void DashStopper()
    {
        //Debug.Log("Dash Stopped");
        animC.AirdashEnd();
        StopCoroutine("DashCoroutine");
    }

    public void JumpStopper()
    {
        //Debug.Log("Jump Stopped");

        StopCoroutine("JumpMomentum");
        airYStopper= true;
    }

    public void Overclock()
    {
        StartCoroutine("OverclockCoroutine");
    }

    private IEnumerator OverclockCoroutine()
    {
        Debug.Log("Overclock Started");
        maxPlayerSpeed *= 2;
        yield return new WaitForSeconds(5);
        maxPlayerSpeed /= 2;
        Debug.Log("Overclock Ended");
    }

    public void SwitchModelFacing(string _modelName)
    {
        if (modelMain)
        {
            if (_modelName == "Head")
            {
                modelMain.enabled = false;
                modelHead.enabled = true;
            }
            else if (_modelName == "Main")
            {
                modelMain.enabled = true;
                modelHead.enabled = false;
            }
        }

    }

    public Vector3 GetGroundPos()
    {
        int layerMask = 1 << 8;

        RaycastHit hit;

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask);
        return hit.point;
    }
}