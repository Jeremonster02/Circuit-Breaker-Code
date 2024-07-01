using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHeadTilt : MonoBehaviour
{
    PlayerInput targetPlayer;
    Transform targetParent;
    public Transform targetTransform;
    Vector3 aim;
    [SerializeField] PlayerInput self;

    public void Init()
    {
        /*GameObject parent = transform.parent.gameObject;
        self = parent.GetComponentInChildren<PlayerInput>();
        if (PlayerInputManager.instance.playerCount > 1 && !targetPlayer)
        {
            foreach (PlayerInput p in GameManager.instance.players)
            {
                if (p != self)
                {
                    targetPlayer = p;
                    targetParent = p.transform.parent;
                    targetTransform = targetParent.GetComponentInChildren<PlayerHeadSwivel>().transform;
                    targetParent.GetComponentInChildren<PlayerHeadTilt>().Init();
                }
            }
        }*/
    }

    private void FixedUpdate()
    {
        //Transform t = this.transform;
        if (!targetTransform)
        {
            //targetTransform = GetComponentInChildren<PlayerHeadSwivel>().targetTransform;
            
            return;
        }
        transform.LookAt(targetTransform);
        targetTransform.localEulerAngles = new Vector3(targetTransform.localRotation.x, 0f, 0f);

    }

    public void Aim(InputAction.CallbackContext ctx)
    {
        Vector2 aiming = ctx.ReadValue<Vector2>();
        aim = new Vector3(aiming.x, 0, aiming.y);
    }
}
