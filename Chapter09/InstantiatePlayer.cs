using Photon.Pun;

using UnityEngine;

public class InstantiatePlayer : MonoBehaviourPun
{
    public GameObject playerPrefab;

    void Start()
    {
        //We call Photon's Instantiate method which will create the player object in the scene and transmit the data to other connected users.

        PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
    }
}
