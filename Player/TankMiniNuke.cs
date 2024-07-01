using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankMiniNuke : MonoBehaviour
{

    public MiniNukeRet target;
    [SerializeField]private GameObject targetPrefab;

    public void InitNuke(PlayerController _pc)
    {
        PlayerStats playerStats = GetComponentInParent<PlayerStats>();
        StateManager sm = GetComponentInParent<StateManager>();

        if (sm.isBusy != true && sm.tankNukeAiming != true && !sm.isStunned && !sm.isDowned)
        {
            WeaponsManager wm = GetComponent<WeaponsManager>();
            //weaponsMgr.isShooting = true;
            sm.isBusy = true;
            wm.sCurTime = 0;
            foreach (PlayerInput p in GameManager.instance.players)
            {
                if (p.playerIndex != _pc.playerIdx)
                {
                    GameObject t = p.gameObject.GetComponent<PlayerInputHandler>().player.gameObject;
                    target = Instantiate(targetPrefab as GameObject, t.transform.position, t.transform.rotation).GetComponent<MiniNukeRet>();
                    _pc.stateManager.tankNukeAiming = true;
                    _pc.stateManager.isBusy = true;
                }
            }
        }
    }

    public void StartNuke()
    {
        //gameObject.GetComponent<StateManager>().isOverclockActive = true;
        // Turn retical red????
        gameObject.GetComponent<StateManager>().isBusy = false;
        //gameObject.GetComponent<StateManager>().isOverclockActive = false;
        gameObject.GetComponent<StateManager>().tankNukeAiming = false;
        target.LaunchNuke();
    }
}
