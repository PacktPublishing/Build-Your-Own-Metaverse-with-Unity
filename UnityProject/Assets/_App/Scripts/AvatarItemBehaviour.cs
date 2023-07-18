using System.Collections;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class AvatarItemBehaviour : MonoBehaviour
{
    //In this variable we will store the prefab we want to use to replace the user's prefab.
    public GameObject avatarPrefab;


    //Pressing the Join button will call this function and change the scene.
    public void ChangeAvatar()
    {
        //We look for the GameObject that represents our player, only our player has the Player tag.
        var player = GameObject.FindWithTag("Player");

        //We look in the player's hierarchy for the GameObject Geometry which is the parent of where the FBX prefab is located.
        var content = player.transform.Find("Geometry");

        //We get the reference to the old FBX in order to access to its properties
        var oldAvatar = content.GetChild(0);

        //Instantiate a copy of the Prefab we have stored in the avatarPrefab variable.
        var newAvatar = Instantiate(avatarPrefab, oldAvatar.transform.position, oldAvatar.transform.rotation);

        PlayerPrefs.SetString("Avatar", avatarPrefab.name);

        //We destroyed the old FBX model
        Destroy(oldAvatar.gameObject);

        //We make the new avatar a child of the GameObject Geometry
        newAvatar.transform.parent = content;

        //We reset the player so that the Animator Component refreshes the new model.
        StartCoroutine(ResetAnimator(player));
    }

    IEnumerator ResetAnimator(GameObject player)
    {
        player.SetActive(false);

        yield return new WaitForSeconds(1f);

        player.SetActive(true);

    }
}
