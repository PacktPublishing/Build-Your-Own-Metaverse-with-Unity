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
        OVRPlayerController controllerVR = GetComponent<OVRPlayerController>();
        Transform camera = transform.parent.Find("Main Camera");
        Transform ovrCameraRig = transform.Find("OVRCameraRigWithControllers");
        Transform playerFollowCamera = transform.parent.Find("PlayerFollowCamera");
        Transform geometryParent = transform.Find("Geometry");
        Transform mobileController = transform.parent.Find("UI_Canvas_StarterAssetsInputs_Joysticks");


        if (!photonView.IsMine)
        {
            gameObject.tag = "Untagged";

            //If not us, we delete all GameObjects and Components that we have searched for before.

            if(mobileController)
            {
                Destroy(mobileController);
            }

            if(controller != null)
            {
                Destroy(controller);
            }

            if (controllerVR != null)
            {
                Destroy(controllerVR);
            }

            if (character != null)
            {
                Destroy(character);
            }
            
            if(camera != null)
            {
                Destroy(camera.gameObject);
            }

            if (ovrCameraRig != null)
            {
                Destroy(ovrCameraRig.gameObject);
            }

            if (playerFollowCamera != null)
            {
                Destroy(playerFollowCamera.gameObject);
            }
            
        }
        else{
            
            gameObject.tag = "Player";

            // We add the GameObject Geometry and its descendants to a specific layer that will allow us to filter it in the Camera.

            var transparentLayer = LayerMask.NameToLayer("TransparentFX");

            SetLayerAllChildren(geometryParent, transparentLayer);
        }

        void SetLayerAllChildren(Transform root, int layer)
        {
            var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                //            Debug.Log(child.name);
                child.gameObject.layer = layer;
            }
        }
    }
}
