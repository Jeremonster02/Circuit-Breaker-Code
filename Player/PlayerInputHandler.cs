using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputHandler : MonoBehaviour
{
    public GameObject[] playerPrefab;
    public int playerRobot1;
    public int playerIndex;
    public int playerRobot2;
    public PlayerController playerController;
    public PlayerHeadSwivel headAim;
    WeaponsManager weaponsManager;
    public PlayerStats playerStats;
    public GameObject player;
    public soRobotTypes selectedRobot;
    Robot1Plate characterSelector;
    public bool fireSpecial = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        int idx = GetComponent<PlayerInput>().playerIndex;
        playerIndex = idx;
        Debug.Log("Player Index = " + idx);
        if (!characterSelector && GameManager.instance.sceneMgr.GetScene() == eScene.frontEnd)
        {
            Transform t = GameObject.FindGameObjectWithTag("SpawnPoint").transform;
            characterSelector = Instantiate(Resources.Load("Widgets/" + "Robot1Plate") as GameObject, t).GetComponent<Robot1Plate>();
            characterSelector.Init(this);
        }
        else if(GameManager.instance.sceneMgr.GetScene() != eScene.frontEnd)
        {
            GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        }
        if(playerIndex == 1)
        {
            GameManager.instance.ToggleJoin();
        }
        

    }

    public void OnSelectorNext(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (GameManager.instance.player1Ready && playerIndex == 0) return;
            if (GameManager.instance.player2Ready && playerIndex == 1) return;
            characterSelector.onSelectorNext();
        }
    }
    public void OnSelectorPrevious(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (GameManager.instance.player1Ready && playerIndex == 0) return;
            if (GameManager.instance.player2Ready && playerIndex == 1) return;
            characterSelector.onSelectorPrevious();
        }
    }
    public void ReadyUp(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (playerIndex == 0)
            {
                characterSelector.PlayPlayer1Ready();
                GameManager.instance.player1Ready = !GameManager.instance.player1Ready;
            }
            if (playerIndex == 1)
            {
                characterSelector.PlayPlayer2Ready();
                GameManager.instance.player2Ready = !GameManager.instance.player2Ready;
            }
            if (GameManager.instance.player1Ready && GameManager.instance.player2Ready)
            {
                gameObject.GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
            }
        }
    }

    public void SpawnPlayer()
    {
        if (!player)
        {
            Debug.Log("Spawning Player");
            player = GameObject.Instantiate(selectedRobot.robotPrefab, GameManager.instance.spawnPoints[playerIndex].transform.position, GameManager.instance.spawnPoints[playerIndex].transform.rotation);
            playerController = player.GetComponent<PlayerController>();
            weaponsManager = player.GetComponent<WeaponsManager>();
            playerStats = player.GetComponent<PlayerStats>();
            headAim = player.GetComponentInChildren<PlayerHeadSwivel>();
            playerStats.InitUI(this);
            //transform.parent = playerController.transform;
            //transform.position = transform.parent.position;
            //headAim.Init();
            playerController.Init(playerIndex);
        }

    }

    public void DestoryPlayer()
    {
        Destroy(player);
    }
    public void Movement(InputAction.CallbackContext ctx)
    {
        if (!playerController.stateManager.overclockCharge)
        {
            if (playerController.stateManager.tankNukeAiming)
            {
                player.GetComponent<TankMiniNuke>().target.OnMove(ctx);
            }
            else
            {
                playerController.OnMove(ctx);
            } 
        }
        else if (playerStats.playerType == PlayerType.LR)
        {
            player.GetComponent<LROverclock>().target.OnMove(ctx);
        }                
    }

    public void Aim(InputAction.CallbackContext ctx)
    {
        headAim.Aim(ctx);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!ctx.canceled)
        {
            playerController.OnJump();
            if (GameManager.instance.audioMgr.jump.isPlaying == false)
            {
                GameManager.instance.audioMgr.PlayJump();
            }
        }
       
        
    }

    public void AirDash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            playerController.AirDash();
            
        }
        
    }

    public void Sprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && GetComponentInParent<StateManager>().isGrounded)
        {
            playerController.maxPlayerSpeed *= playerController.playerDashMod;
            /*while (ctx.performed)
            {
                if (GameManager.instance.audioMgr.run.isPlaying == false)
                {
                    GameManager.instance.audioMgr.PlayRun();
                }
            }*/
        }
        else if (ctx.canceled && GetComponentInParent<StateManager>().isGrounded)
        {
            playerController.maxPlayerSpeed /= playerController.playerDashMod;
        }
    }

    public void PrimaryFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            weaponsManager.PrimaryFire();
        }
        
        
    }
    public void SecondaryFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //weaponsManager.SecondaryFire();   make specific(ish)
            switch (playerStats.playerType)
            {
                case PlayerType.Balance:
                    weaponsManager.SecondaryFire();
                    break;
                case PlayerType.LR:
                    weaponsManager.SecondaryFire();
                    break;
                case PlayerType.Tank:
                    player.GetComponent<TankMiniNuke>().InitNuke(playerController);
                    break;
                default:
                    break;
            }
        }
        if (ctx.canceled)
        {
            if(playerStats.playerType == PlayerType.Tank)
            {
                player.GetComponent<TankMiniNuke>().StartNuke();
            }
        }
    }

    public void MeleeAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //weaponsManager.MeleeAttack();  Make Specific To Character
            switch (playerStats.playerType)
            {
                case PlayerType.Balance:
                    player.GetComponent<BMelee>().MeleeAttack();
                    //Debug.Log("Player Type: Balance");
                    //player.GetComponent<ErrorDischarge>().StartErrorDischarge();
                    break;
                case PlayerType.LR:
                    player.GetComponent<LRMelee>().MeleeAttack();
                    break;
                case PlayerType.Tank:
                    player.GetComponent<TMelee>().MeleeAttack();
                    break;
                default:
                    break;
            }
        }
    }
    public void Special(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            switch (playerStats.playerType)
            {
                case PlayerType.Balance:
                    player.GetComponent<PlayerProxMine>().LaunchProxMine();
                    fireSpecial = true;
                    //Debug.Log("Player Type: Balance");
                    //player.GetComponent<ErrorDischarge>().StartErrorDischarge();
                    break;
                case PlayerType.LR:
                    player.GetComponent<PlayerAirrest>().LaunchAirrest();
                    fireSpecial = true;
                    break;
                default:
                    break;
            }
            
        }
    }

    public void Overclock(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            switch (playerStats.playerType)
            {
                case PlayerType.Balance:
                    player.GetComponent<BalanceOverclock>().LaserBeam();
                    break;
                case PlayerType.LR:
                    player.GetComponent<LROverclock>().Init(playerController);
                    break;
                default:
                    break;
            }
            
        }
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameManager.instance.canvasMgr.ShowOptionsMenu();
            GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
        }
    }
}
