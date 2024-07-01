using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerAirrest : MonoBehaviour
{
    [SerializeField]GameObject airRestPrefab;
    [SerializeField] Transform airRestSpawnPoint;
    PlayerStats playerStats;
    StateManager stateManager;
    //public bool fireDrone;

    public void Awake()
    {
        stateManager = GetComponent<StateManager>();
        playerStats = GetComponent<PlayerStats>();
        
    }

    public void LaunchAirrest()
    {
        if (playerStats.charge >= 50)
        {
            if (!stateManager.isStunned && !stateManager.isBusy && !stateManager.isDowned)
            {
                playerStats.charge -= 50;
                Debug.Log("Launching Airrest");
                airRestSpawnPoint.position = new Vector3 (airRestSpawnPoint.position.x - 1, 0, airRestSpawnPoint.position.z);
                Airrest airRest = Instantiate(airRestPrefab, airRestSpawnPoint.position, airRestSpawnPoint.rotation).GetComponent<Airrest>();
                airRest.Init(GetComponent<PlayerController>());
                GameManager.instance.audioMgr.PlaySpecial();
                //fireDrone = true;
            }
        }
        Debug.Log("InputRecieved");
        
    } 
}
