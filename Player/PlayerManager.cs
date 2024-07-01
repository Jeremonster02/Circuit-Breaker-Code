using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    private void Awake()
    {
        //spawnPoint1 = GameManager.instance.spawnPoints[0].transform;
        //Instantiate(player1, spawnPoint1);
    }
}
