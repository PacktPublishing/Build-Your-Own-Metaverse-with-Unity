using System.Collections;

using System.Collections.Generic;

using Photon.Pun;

using StarterAssets;

using UnityEngine;

public class LoadAvatar : MonoBehaviour
{
    public GameObject Player;

    void Awake()
    {
        if (Player.GetComponent<PhotonView>().IsMine)
        {
            ChangeAvatar();
        }
    }

    //Pressing the Join button will call this function and change the scene.

    public void ChangeAvatar()
    {
        string avatarName = PlayerPrefs.GetString("Avatar") ?? "Exo Gray";

        //We look in the player's hierarchy for the GameObject Geometry which is the parent of where the FBX prefab is located.

        var content = Player.transform.Find("Geometry");

        //We get the reference to the old FBX in order to access to its properties

        var oldAvatar = content.GetChild(0);

        //Instantiate a copy of the Prefab we have stored in the avatarPrefab variable.

        GameObject avatarInstance =
            Instantiate(
                Resources.Load("Prefabs/Avatars/" + avatarName),
                oldAvatar.transform.position,
                oldAvatar.transform.rotation
            ) as GameObject;

        //We destroyed the old FBX model

        Destroy(oldAvatar.gameObject);

        //We make the new avatar a child of the GameObject Geometry

        avatarInstance.transform.parent = content;

        //We reset the player so that the Animator Component refreshes the new model.

        StartCoroutine(ResetAnimator());
    }

    IEnumerator ResetAnimator()
    {
        Player.SetActive(false);

        yield return new WaitForSeconds(1f);

        Player.SetActive(true);
    }
}
