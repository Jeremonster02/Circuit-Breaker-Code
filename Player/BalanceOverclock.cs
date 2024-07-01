using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceOverclock : MonoBehaviour
{
    [SerializeField] private GameObject overclockPrefab;
    [SerializeField] private float overclockLength;
    [SerializeField] private float overclockCharge;
    private GameObject overclockBeam;
    private PlayerStats playerStats;
    private StateManager sm;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        sm = GetComponent<StateManager>();
    }

    public void LaserBeam()
    {
        if(playerStats.charge == 100 && sm.isBusy != true && sm.overclockCharge != true && !sm.isStunned && !sm.isDowned)
        {
            StartCoroutine("OverclockChargeUp");
        }
    }
    
    IEnumerator OverclockChargeUp()
    {
        sm.isBusy = true;
        sm.overclockCharge = true;
        yield return new WaitForSeconds(overclockCharge);
        sm.isOverclockActive = true;
        overclockBeam = Instantiate(overclockPrefab, GetComponent<WeaponsManager>().firePosition.position, GetComponent<WeaponsManager>().firePosition.rotation);
        overclockBeam.transform.parent = GetComponent<WeaponsManager>().firePosition;
        StartCoroutine("OverclockDuration");
        StartCoroutine("OverclockChargeDrain");
    }

    IEnumerator OverclockChargeDrain()
    {
        while(playerStats.charge > 0)
        {
            float chargeDrain = 100 / overclockLength;
            playerStats.charge -= chargeDrain/10;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator OverclockDuration()
    {
        yield return new WaitForSeconds(overclockLength);
        Destroy(overclockBeam);
        sm.isBusy = false;
        sm.isOverclockActive = false;
        sm.overclockCharge = false;
    }
}
