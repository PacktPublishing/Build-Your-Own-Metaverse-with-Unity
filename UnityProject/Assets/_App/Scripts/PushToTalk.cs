using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;

public class PushToTalk : MonoBehaviourPun
{
    //We create a variable that stores the key we want it to recognise to transmit the voice.
    public KeyCode PushButton = KeyCode.V;
    public Recorder VoiceRecorder;
    private PhotonView view;
    private bool pushtotalk = true;
    public GameObject localspeaker;

    void Start()
    {
        view = photonView;
        VoiceRecorder.TransmitEnabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(PushButton))
        {
            if (view.IsMine)
            {
                Debug.Log("Transmiting...");

                VoiceRecorder.TransmitEnabled = true;
            }
        }
        else if (Input.GetKeyUp(PushButton))
        {
            if (view.IsMine)
            {
                Debug.Log("Stop transmition...");
                VoiceRecorder.TransmitEnabled = false;
            }
        }
    }
}