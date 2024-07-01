using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class LRMelee : MeleeBasic
{
    [Header("Melee Settings")]
    [SerializeField] private float melee1AttackTime;
    [SerializeField] private float melee2AttackTime;
    [SerializeField] private float meleeJumpHeight;

    public void MeleeAttack()
    {
        if (!stateManager.isStunned && !stateManager.isBusy && !stateManager.isDowned && playerStats.charge >= chargeCost)
        {
            pController.airYStopper = true;
            StartCoroutine("MeleeCoroutine");
            if (GameManager.instance.audioMgr.melee.isPlaying == false)
            {
                GameManager.instance.audioMgr.PlayMelee();
            }
        }
    }

    private IEnumerator MeleeCoroutine()
    {
        if (playerStats.charge >= chargeCost)
        {
            sword.SetActive(true);
            stateManager.isBusy = true;
            isMelee = true;
            playerStats.charge -= chargeCost;
            float starttime = Time.time;
            Vector3 moveDirection = transform.forward * 3;
            //float momentum = melee.magnitude;
            Vector3 upDirection = transform.up * 3;
            yield return new WaitForSeconds(meleeStartup);

            while (starttime + melee1AttackTime > Time.time)
            {
                controller.Move(moveDirection * Time.deltaTime * meleeDashSpeed);
                yield return null;
            }
            while (starttime + melee1AttackTime + melee2AttackTime > Time.time)
            {
                // Jump upwards
                controller.Move((upDirection + (moveDirection / 2)) * Time.deltaTime * (meleeJumpHeight / 2f));
                yield return null;
            }

            sword.SetActive(false);
            yield return new WaitForSeconds(meleeEndLag);
            stateManager.isBusy = false;
            isMelee = false;
            pController.airYStopper = false;
        }
    }
    //private IEnumerator JumpStart()
    //{
    //    yield return new WaitForSeconds(jumpWait);
    //    playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * curGravityValue);
    //    StartCoroutine("JumpMomentum");
    //}

    //private IEnumerator JumpMomentum()
    //{
    //    if (move != Vector3.zero)
    //    {
    //        float momentum = move.magnitude;
    //        Vector3 moveDerection = transform.forward * momentum;
    //        yield return new WaitForSeconds(.03f);
    //        while (!stateManager.isGrounded && !isDashing)
    //        {
    //            controller.Move(moveDerection * Time.deltaTime * momentum * (curPlayerSpeed / 2f));
    //            yield return null;
    //        }
    //    }
    //}

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
