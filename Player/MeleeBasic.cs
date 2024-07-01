using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBasic : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] public float chargeCost;
    [SerializeField] public float meleeStartup;
    [SerializeField] public float meleeEndLag;
    [SerializeField] public float meleeAttackTime;
    [SerializeField] public float meleeDashSpeed;
    public bool isMelee;
    public float meleeDamage;
    public float meleeStun;
    public GameObject sword;

    [Header("Misc Settings")]
    //public Transform firePosition;
    public StateManager stateManager;
    public CharacterController controller;
    public PlayerStats playerStats;
    public PlayerController pController;

    private void Awake()
    {
        stateManager = GetComponent<StateManager>();
        controller = GetComponent<CharacterController>();
        playerStats = GetComponentInParent<PlayerStats>();
        pController = GetComponentInParent<PlayerController>();
    }

}
