using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniNukeRet : MonoBehaviour
{
    StateManager sm;
    Vector3 move;
    CharacterController controller;
    public int targetSpeed;
    [SerializeField] GameObject nukePrefab;
    [SerializeField] float spawnHeight;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        controller.Move(move * Time.deltaTime * targetSpeed);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 movement = ctx.ReadValue<Vector2>();
        move = new Vector3(movement.x, 0, movement.y);
    }

    public void LaunchNuke()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + spawnHeight, transform.position.z);
        Instantiate(nukePrefab as GameObject, pos, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
