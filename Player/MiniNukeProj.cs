using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniNukeProj : MonoBehaviour
{
    [SerializeField] GameObject distanceIndicatorPrefab;
    GameObject distanceIndicator;
    [SerializeField] Transform raycastOrigin;

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
        int layerMask = 1 << 8;

        RaycastHit hit;

        Physics.Raycast(raycastOrigin.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask);
        Vector3 t = hit.point;
        Vector3 pos = new Vector3(t.x, t.y + .01f, t.z);
        distanceIndicator = Instantiate(distanceIndicatorPrefab as GameObject, pos, Quaternion.identity);
    }

    float GetDistance()
    {
        int layerMask = 1 << 8;

        RaycastHit hit;

        Physics.Raycast(raycastOrigin.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask);
        float dis = Vector3.Distance(raycastOrigin.position, hit.point);
        return dis;
    }

    private void FixedUpdate()
    {
        distanceIndicator.transform.localScale = new Vector3(GetDistance(), 0, GetDistance());
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(distanceIndicator);
        GameObject pExplosion = Instantiate(explosionPrefab, this.gameObject.transform.position, this.gameObject.transform.rotation);
        Explosion explosionCS = pExplosion.GetComponent<Explosion>();
        explosionCS.StartExplosion(explosionStartSize, explosionEndSize, explosionLength, explosionDamage, explosionStun);
        Destroy(this.gameObject);
    }
}
