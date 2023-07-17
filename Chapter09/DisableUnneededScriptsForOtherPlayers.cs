using System.Collections;

using System.Collections.Generic;

using Photon.Pun;

using StarterAssets;

using UnityEngine;

public class DisableUnneededScriptsForOtherPlayers : MonoBehaviourPun
{
    void Start()
    {
        //We search for all components or GameObjects that can create interference with our player.

        PhotonView photonView = GetComponent<PhotonView>();

        CharacterController character = GetComponent<CharacterController>();

        ThirdPersonController controller = GetComponent<ThirdPersonController>();

        Transform camera = transform.parent.Find("Main Camera");

        Transform playerFollowCamera = transform.parent.Find("PlayerFollowCamera");

        Transform mobileController = transform.parent.Find(
            "UI_Canvas_StarterAssetsInputs_Joysticks"
        );

        if (!photonView.IsMine)
        {
            gameObject.tag = "Untagged";

            //If not us, we delete all GameObjects and Components that we have searched for before.

            Destroy(mobileController);

            Destroy(controller);

            Destroy(character);

            Destroy(camera.gameObject);

            Destroy(playerFollowCamera.gameObject);

            Destroy(this);
        }
        else
        {
            gameObject.tag = "Player";
        }
    }
}
