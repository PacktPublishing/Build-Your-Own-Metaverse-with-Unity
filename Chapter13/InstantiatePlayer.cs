using ExitGames.Client.Photon;

using Photon.Pun;

using Photon.Realtime;

using UnityEngine;

public class InstantiatePlayer : MonoBehaviourPun
{
    public GameObject playerPrefab;

    public GameObject playerPrefabVR;

    void Start()
    {
        var headsetType = OVRPlugin.GetSystemHeadsetType();

        // Using the headsetType variable, we can detect if we are running the project in virtual reality glasses.If it returns None, it means that we are not running in virtual reality.

        if (headsetType == OVRPlugin.SystemHeadset.None)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(playerPrefabVR.name, transform.position, Quaternion.identity);
        }
    }
}
