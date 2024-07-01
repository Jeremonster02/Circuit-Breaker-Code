using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerHeadSwivel : MonoBehaviour
{
    PlayerInput targetPlayer;
    Transform targetParent;
    public Transform targetTransform;
    public Transform headAimer;

    public Vector3 aim;
    [SerializeField]Transform self;

    public void Init()
    {
        self = this.transform;
        foreach(PlayerInput p in GameManager.instance.players)
        {
            if (p.gameObject.GetComponent<PlayerInputHandler>().playerController.headTransform != self)
            {
                targetTransform = p.gameObject.GetComponent<PlayerInputHandler>().playerController.headTransform;
            }
        }
    }

    private void FixedUpdate()
    {
        headAimer.LookAt(targetTransform);
        if (aim != Vector3.zero && !GetComponentInParent<StateManager>().isOverclockActive)
        {
            transform.forward = aim;
            Vector3 rot = transform.localRotation.eulerAngles;
            rot.Set(headAimer.localRotation.eulerAngles.x, rot.y, rot.z);
            transform.localRotation = Quaternion.Euler(rot);
        }
        else if (targetTransform && !GetComponentInParent<StateManager>().overclockCharge)
        {
            transform.LookAt(targetTransform);
        }
        
    }

    public void Aim(InputAction.CallbackContext ctx)
    {
        Vector2 aiming = ctx.ReadValue<Vector2>();       
        aim = new Vector3(aiming.x, 0, aiming.y);
    }
}
