// ----------------------------------------------------------------------------
// <copyright file="PunVoiceClient.cs" company="Exit Games GmbH">
// Photon Voice - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// This class can be used to automatically join/leave Voice rooms when
// Photon Unity Networking (PUN) joins or leaves its rooms. The Voice room
// will use the same name as PUN, but with a "_voice_" postfix.
// It also finds the Speaker component for a character's voice. For this to work,
// the voice's UserData must be set to the character's PhotonView ID.
// (see "PhotonVoiceView.cs")
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if PUN_2_OR_NEWER

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;

namespace Photon.Voice.PUN
{

    /// <summary>
    /// This class can be used to automatically sync client states between PUN and Voice.
    /// It also finds the Speaker component for a character's voice.
    /// For this to work attach a <see cref="PhotonVoiceView"/> next to the <see cref="PhotonView"/> of your player's prefab.
    /// </summary>
    [AddComponentMenu("Photon Voice/PUN/Pun Voice Client")]
    [HelpURL("https://doc.photonengine.com/en-us/voice/v2/getting-started/voice-for-pun")]
    public class PunVoiceClient : VoiceConnection
    {
        #region Public Fields

        /// <summary> Suffix for voice room names appended to PUN room names. </summary>
        public const string VoiceRoomNameSuffix = "_voice_";
        /// <summary> Auto connect voice client and join a voice room when PUN client is joined to a PUN room </summary>
        public bool AutoConnectAndJoin = true;
        /// <summary> Auto disconnect voice client when PUN client is not joined to a PUN room </summary>
        public bool AutoLeaveAndDisconnect = true;
        #endregion

        #region Private Fields

        private EnterRoomParams voiceRoomParams = new EnterRoomParams
        {
            RoomOptions = new RoomOptions { IsVisible = false }
        };
        private bool clientCalledConnectAndJoin;
        private bool clientCalledDisconnect;
        private bool clientCalledConnectOnly;
        private bool internalDisconnect;
        private bool internalConnect;
        private static PunVoiceClient instance;

        [SerializeField]
        private bool usePunAppSettings = true;

        [SerializeField]
        private bool usePunAuthValues = true;

        #endregion

        #region Properties

        /// <summary>
        /// Singleton instance for PunVoiceClient
        /// </summary>
        public static PunVoiceClient Instance
        {
            get
            {
                if (instance == null)
                {
                    PunVoiceClient[] objects = FindObjectsOfType<PunVoiceClient>();
                    if (objects == null || objects.Length < 1)
                    {
                        GameObject singleton = new GameObject();
                        singleton.name = "PunVoiceClient";
                        instance = singleton.AddComponent<PunVoiceClient>();
                        instance.Logger.LogError("PunVoiceClient component was not found in the scene. Creating PunVoiceClient object.");
                    }
                    else if (objects.Length >= 1)
                    {
                        instance = objects[0];
                        instance.Logger.LogInfo("An instance of PunVoiceClient is found in the scene.");
                        if (objects.Length > 1)
                        {
                            instance.Logger.LogError("{0} instances of PunVoiceClient found in the scene. Using a random instance.", objects.Length);
                        }
                    }
                    instance.Logger.LogInfo("PunVoiceClient singleton instance is now set.");
                }
                return instance;
            }
        }

        /// <summary>
        /// Whether or not to use the Voice AppId and all the other AppSettings from PUN's PhotonServerSettings ScriptableObject singleton in the Voice client/app.
        /// </summary>
        public bool UsePunAppSettings
        {
            get
            {
                return this.usePunAppSettings;
            }
            set
            {
                this.usePunAppSettings = value;
            }
        }

        /// <summary>
        /// Whether or not to use the same PhotonNetwork.AuthValues in PunVoiceClient.Instance.Client.AuthValues.
        /// This means that the same UserID will be used in both clients.
        /// If custom authentication is used and setup in PUN app, the same configuration should be done for the Voice app.
        /// </summary>
        public bool UsePunAuthValues
        {
            get
            {
                return this.usePunAuthValues;
            }
            set
            {
                this.usePunAuthValues = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connect voice client to Photon servers and join a Voice room
        /// </summary>
        /// <returns>If true, connection command send from client</returns>
        public bool ConnectAndJoinRoom()
        {
            if (!PhotonNetwork.InRoom)
            {
                this.Logger.LogError("Cannot connect and join if PUN is not joined.");
                return false;
            }
            if (this.Connect())
            {
                this.clientCalledConnectAndJoin = true;
                this.clientCalledDisconnect = false;
                return true;
            }
            this.Logger.LogError("Connecting to server failed.");
            return false;
        }

        /// <summary>
        /// Disconnect voice client from all Photon servers
        /// </summary>
        public void Disconnect()
        {
            if (!this.Client.IsConnected)
            {
                this.Logger.LogError("Cannot Disconnect if not connected.");
                return;
            }
            this.clientCalledDisconnect = true;
            this.clientCalledConnectAndJoin = false;
            this.clientCalledConnectOnly = false;
            this.Client.Disconnect();
        }

        #endregion

        #region Private Methods

        protected void Start()
        {
            if (Instance == this)
            {
                if (this.UsePrimaryRecorder)
                {
                    if (this.PrimaryRecorder != null)
                    {
                        AddRecorder(this.PrimaryRecorder);
                    }
                    else
                    {
                        this.Logger.LogError("Primary Recorder is not set.");
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (Instance == this)
            {
                PhotonNetwork.NetworkingClient.StateChanged += this.OnPunStateChanged;
                this.FollowPun(); // in case this is enabled or activated late
                this.clientCalledConnectAndJoin = false;
                this.clientCalledConnectOnly = false;
                this.clientCalledDisconnect = false;
                this.internalDisconnect = false;
            }
        }

        protected override void OnDisable()
        {
        	base.OnDisable();
            if (Instance == this)
            {
                PhotonNetwork.NetworkingClient.StateChanged -= this.OnPunStateChanged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (instance == this)
            {
                instance.Logger.LogInfo("PunVoiceClient singleton instance is being reset because destroyed.");
                instance = null;
            }
        }

        private void OnPunStateChanged(ClientState fromState, ClientState toState)
        {
            this.Logger.LogInfo("OnPunStateChanged from {0} to {1}", fromState, toState);
            this.FollowPun(toState);
        }

        protected override void OnVoiceStateChanged(ClientState fromState, ClientState toState)
        {
            base.OnVoiceStateChanged(fromState, toState);
            if (toState == ClientState.Disconnected)
            {
                if (this.internalDisconnect)
                {
                    this.internalDisconnect = false;
                }
                else if (!this.clientCalledDisconnect)
                {
                    this.clientCalledDisconnect = this.Client.DisconnectedCause == DisconnectCause.DisconnectByClientLogic;
                }
            }
            else if (toState == ClientState.ConnectedToMasterServer)
            {
                if (this.internalConnect)
                {
                    this.internalConnect = false;
                }
                else if (!this.clientCalledConnectOnly && !this.clientCalledConnectAndJoin)
                {
                    this.clientCalledConnectOnly = true;
                    this.clientCalledDisconnect = false;
                }
            }
            this.FollowPun(toState);
        }

        private void FollowPun(ClientState toState)
        {
            switch (toState)
            {
                case ClientState.Joined:
                case ClientState.Disconnected:
                case ClientState.ConnectedToMasterServer:
                    this.FollowPun();
                    break;
            }
        }

        protected override Speaker InstantiateSpeakerForRemoteVoice(int playerId, byte voiceId, object userData)
        {
            if (userData == null) // Recorder w/o PhotonVoiceView: probably created due to this.UsePrimaryRecorder = true
            {
                this.Logger.LogInfo("Creating Speaker for remote voice p#{0} v#{1} PunVoiceClient Primary Recorder (userData == null).", playerId, voiceId);
                return this.InstantiateSpeakerPrefab(this.gameObject, true);
            }

            if (!(userData is int))
            {
                this.Logger.LogWarning("UserData ({0}) does not contain PhotonViewId. Remote voice p#{1} v#{2} not linked. Do you have a Recorder not used with a PhotonVoiceView? is this expected?",
                    userData == null ? "null" : userData.ToString(), playerId, voiceId);
                return null;
            }

            int photonViewId = (int)userData;
            PhotonView photonView = PhotonView.Find(photonViewId);
            if (null == photonView || !photonView)
            {
                this.Logger.LogWarning("No PhotonView with ID {0} found. Remote voice p#{1} v#{2} not linked.", userData, playerId, voiceId);
                return null;
            }

            PhotonVoiceView photonVoiceView = photonView.GetComponent<PhotonVoiceView>();
            if (null == photonVoiceView || !photonVoiceView)
            {
                this.Logger.LogWarning("No PhotonVoiceView attached to the PhotonView with ID {0}. Remote voice p#{1} v#{2} not linked.", userData, playerId, voiceId);
                return null;
            }
            this.Logger.LogInfo("Using PhotonVoiceView {0} Speaker for remote voice p#{1} v#{2}.", userData, playerId, voiceId);
            return photonVoiceView.SpeakerInUse;
        }

        internal static string GetVoiceRoomName()
        {
            if (PhotonNetwork.InRoom)
            {
                return string.Format("{0}{1}", PhotonNetwork.CurrentRoom.Name, VoiceRoomNameSuffix);
            }
            return null;
        }

        private void ConnectOrJoin()
        {
            switch (this.ClientState)
            {
                case ClientState.PeerCreated:
                case ClientState.Disconnected:
                    this.Logger.LogInfo("PUN joined room, now connecting Voice client");
                    if (!this.Connect())
                    {
                        this.Logger.LogError("Connecting to server failed.");
                    }
                    else
                    {
                        this.internalConnect = this.AutoConnectAndJoin && !this.clientCalledConnectOnly && !this.clientCalledConnectAndJoin;
                    }
                    break;
                case ClientState.ConnectedToMasterServer:
                    this.Logger.LogInfo("PUN joined room, now joining Voice room");
                    if (!this.JoinRoom(GetVoiceRoomName()))
                    {
                        this.Logger.LogError("Joining a voice room failed.");
                    }
                    break;
                default:
                    this.Logger.LogWarning("PUN joined room, Voice client is busy ({0}). Is this expected?", this.ClientState);
                    break;
            }
        }

        private bool Connect()
        {
            AppSettings settings = null;

            if (this.usePunAppSettings)
            {
                settings = new AppSettings();
                settings = PhotonNetwork.PhotonServerSettings.AppSettings.CopyTo(settings); // creates an independent copy (cause we need to modify it slightly)
                if (!string.IsNullOrEmpty(PhotonNetwork.CloudRegion))
                {
                    settings.FixedRegion = PhotonNetwork.CloudRegion; // makes sure the voice connection follows into the same cloud region (as PUN uses now).
                }

                this.Client.SerializationProtocol = PhotonNetwork.NetworkingClient.SerializationProtocol;
            }

            // use the same user, authentication, auth-mode and encryption as PUN
            if (this.UsePunAuthValues)
            {
                if (PhotonNetwork.AuthValues != null)
                {
                    if (this.Client.AuthValues == null)
                    {
                        this.Client.AuthValues = new AuthenticationValues();
                    }
                    this.Client.AuthValues = PhotonNetwork.AuthValues.CopyTo(this.Client.AuthValues);
                }
                this.Client.AuthMode = PhotonNetwork.NetworkingClient.AuthMode;
                this.Client.EncryptionMode = PhotonNetwork.NetworkingClient.EncryptionMode;
            }

            return this.ConnectUsingSettings(settings);
        }

        private bool JoinRoom(string voiceRoomName)
        {
            if (string.IsNullOrEmpty(voiceRoomName))
            {
                this.Logger.LogError("Voice room name is null or empty.");
                return false;
            }
            this.voiceRoomParams.RoomName = voiceRoomName;
            return this.Client.OpJoinOrCreateRoom(this.voiceRoomParams);
        }

        // Follow PUN client state
        // In case Voice client disconnects unexpectedly try to reconnect to the same room
        // In case Voice client is connected to the wrong room switch to the correct one
        private void FollowPun()
        {
            if (PhotonNetwork.OfflineMode)
            {
                return;
            }
            if (PhotonNetwork.NetworkClientState == this.ClientState)
            {
                if (PhotonNetwork.InRoom && this.AutoConnectAndJoin)
                {
                    string expectedRoomName = GetVoiceRoomName();
                    string currentRoomName = this.Client.CurrentRoom.Name;
                    if (!currentRoomName.Equals(expectedRoomName))
                    {
                        this.Logger.LogWarning(
                            "Voice room mismatch: Expected:\"{0}\" Current:\"{1}\", leaving the second to join the first.",
                            expectedRoomName, currentRoomName);
                        if (!this.Client.OpLeaveRoom(false))
                        {
                            this.Logger.LogError("Leaving the current voice room failed.");
                        }
                    }
                }
                else if (this.ClientState == ClientState.ConnectedToMasterServer && this.AutoLeaveAndDisconnect && !this.clientCalledConnectAndJoin && !this.clientCalledConnectOnly)
                {
                    this.Logger.LogWarning("Unexpected: PUN and Voice clients have the same client state: ConnectedToMasterServer, Disconnecting Voice client.");
                    this.internalDisconnect = true;
                    this.Client.Disconnect();
                }
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                if (this.clientCalledConnectAndJoin || this.AutoConnectAndJoin && !this.clientCalledDisconnect)
                {
                    this.ConnectOrJoin();
                }
            }
            else if (this.Client.InRoom && this.AutoLeaveAndDisconnect && !this.clientCalledConnectAndJoin && !this.clientCalledConnectOnly)
            {
                this.Logger.LogInfo("PUN left room, disconnecting Voice");
                this.internalDisconnect = true;
                this.Client.Disconnect();
            }
        }

        #endregion
    }
}
#endif