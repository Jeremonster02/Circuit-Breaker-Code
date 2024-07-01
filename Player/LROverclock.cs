using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LROverclock : MonoBehaviour
{

    public LROverclockTarget target;
    [SerializeField]private GameObject targetPrefab;
    [SerializeField] float timeToExplosion;

    public void Init(PlayerController _pc)
    {
        PlayerStats playerStats = GetComponentInParent<PlayerStats>();
        StateManager sm = GetComponentInParent<StateManager>();

        if (playerStats.charge == 100 && sm.isBusy != true && sm.overclockCharge != true && !sm.isStunned && !sm.isDowned)
        {
            GetComponent<PlayerStats>().charge -= 100;
            foreach (PlayerInput p in GameManager.instance.players)
            {
                if (p.playerIndex != _pc.playerIdx)
                {
                    GameObject t = p.gameObject.GetComponent<PlayerInputHandler>().player.gameObject;
                    target = Instantiate(targetPrefab as GameObject, t.transform.position, t.transform.rotation).GetComponent<LROverclockTarget>();
                    _pc.stateManager.overclockCharge = true;
                    _pc.stateManager.isBusy = true;
                }
            }
        }
    }

    public void StartOverclock()
    {
        gameObject.GetComponent<StateManager>().isOverclockActive = true;
        // Turn retical red????
        gameObject.GetComponent<StateManager>().isBusy = false;
        StartCoroutine("explosionDelayer");
        gameObject.GetComponent<StateManager>().isOverclockActive = false;
        gameObject.GetComponent<StateManager>().overclockCharge = false;
        
    }

    public IEnumerator explosionDelayer()
    {
        yield return new WaitForSeconds(timeToExplosion);
        target.Explode();
    }
}
