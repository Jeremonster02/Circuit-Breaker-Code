using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class BMelee : MeleeBasic
{
    private AudioMgrRobo audioMgrRobo;

    private void Awake()
    {
        stateManager = GetComponent<StateManager>();
        controller = GetComponent<CharacterController>();
        playerStats = GetComponentInParent<PlayerStats>();
        pController = GetComponentInParent<PlayerController>();
        audioMgrRobo = GetComponentInChildren<AudioMgrRobo>();
    }

    public void MeleeAttack()
    {
        if (!stateManager.isStunned && !stateManager.isBusy && !stateManager.isDowned)
        {
            pController.animC.MeleeStart();
            audioMgrRobo.PlayMelee();
            StartCoroutine("MeleeCoroutine");
            /*if (GameManager.instance.audioMgr.melee.isPlaying == false)
            {
                GameManager.instance.audioMgr.PlayMelee();
            }*/
        }
    }

    private IEnumerator MeleeCoroutine()
    {
        if (playerStats.charge >= chargeCost)
        {
            //sword.SetActive(true);
            stateManager.isBusy = true;
            isMelee = true;
            playerStats.charge -= chargeCost;
            float starttime = Time.time;
            Vector3 moveDerection = transform.forward * 3;
            yield return new WaitForSeconds(meleeStartup);
            
            while (starttime + meleeAttackTime > Time.time)
            {
                controller.Move(moveDerection * Time.deltaTime * meleeDashSpeed);
                yield return null;
            }

            //sword.SetActive(false);
            yield return new WaitForSeconds(meleeEndLag);
            stateManager.isBusy = false;
            isMelee = false;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        PlayerStats targetStats = collision.collider.gameObject.GetComponentInParent<PlayerStats>();
        //Debug.Log(targetStats);
        if (targetStats)
        {
            if (isMelee)
            {
                Debug.Log("Melee Hit");
                StopCoroutine("MeleeCoroutine");
                Vector2 knockbackVector = new Vector2(targetStats.transform.position.x - this.transform.position.x, targetStats.transform.position.z - this.transform.position.z);
                targetStats.TakeDamage(meleeDamage, meleeStun, true, knockbackVector);
                sword.SetActive(false);
                stateManager.isBusy = false;
                isMelee = false;
            }

        }
    }
}
