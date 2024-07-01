using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LROverclockTarget : MonoBehaviour
{
    StateManager sm;
    Vector3 move;
    CharacterController controller;
    public int targetSpeed;
    [Header("Explosion Settings")]
    //public bool explosion = false;
    [SerializeField] float explosionStartSize;
    [SerializeField] float explosionEndSize;
    [SerializeField] float explosionLength;
    [SerializeField] float explosionDamage;
    [SerializeField] float explosionStun;
    [SerializeField] GameObject explosionPrefab;

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

    public void Explode()
    {
        Debug.Log("Drone Strike Successful");
        GameObject pExplosion = Instantiate(explosionPrefab, this.gameObject.transform.position, this.gameObject.transform.rotation);
        Explosion explosionCS = pExplosion.GetComponent<Explosion>();
        explosionCS.StartExplosion(explosionStartSize, explosionEndSize, explosionLength, explosionDamage, explosionStun);
        Destroy(this.gameObject);
    }
}
