namespace Photon.Voice.Unity.Demos.DemoVoiceUI
{
    using ExitGames.Client.Photon;
    using Unity;
    using UnityEngine;
    using UnityEngine.UI;
    using Realtime;

    [RequireComponent(typeof(Speaker))]
    public class RemoteSpeakerUI : MonoBehaviour, IInRoomCallbacks
    {
#pragma warning disable 649
        [SerializeField]
        private Text nameText;
        [SerializeField]
        protected Image remoteIsMuting;
        [SerializeField]
        private Image remoteIsTalking;
        [SerializeField]
        private InputField playDelayInputField;
        [SerializeField]
        private Text bufferLagText;
        [SerializeField]
        private Slider volumeSlider;
#pragma warning restore 649
        protected Speaker speaker;
        private AudioSource audioSource;

        protected Player Actor { get { return this.loadBalancingClient.CurrentRoom != null ? this.loadBalancingClient.CurrentRoom.GetPlayer(this.speaker.RemoteVoice.PlayerId) : null; } }

        protected VoiceConnection voiceConnection;
        protected LoadBalancingClient loadBalancingClient;

        protected virtual void Start()
        {
            this.speaker = this.GetComponent<Speaker>();
            this.audioSource = this.GetComponent<AudioSource>();
            this.playDelayInputField.text = this.speaker.PlayDelay.ToString();
            this.playDelayInputField.SetSingleOnEndEditCallback(this.OnPlayDelayChanged);
            this.SetNickname();
            this.SetMutedState();

            this.volumeSlider.minValue = 0f;
            this.volumeSlider.maxValue = 1f;
            this.volumeSlider.SetSingleOnValueChangedCallback(this.OnVolumeChanged);
            this.volumeSlider.value = 1;
            this.OnVolumeChanged(1);

        }
        private void OnVolumeChanged(float newValue)
        {
            this.audioSource.volume = newValue;
        }

        private void OnPlayDelayChanged(string str)
        {
            if (int.TryParse(str, out int x))
            {
                this.speaker.PlayDelay = x;
            }
            else
            {
                Debug.LogErrorFormat("Failed to parse {0}", str);
            }
        }

        private void Update()
        {
            // TODO: It would be nice, if we could show if a user is actually talking right now (Voice Detection)
            this.remoteIsTalking.enabled = this.speaker.IsPlaying;
            this.bufferLagText.text = string.Concat("Buffer Lag: ", this.speaker.Lag);
        }

        private void OnDestroy()
        {
            if (this.loadBalancingClient != null)
            {
                this.loadBalancingClient.RemoveCallbackTarget(this);
            }
        }

        private void SetNickname()
        {
            string nick = this.speaker.name;
            if (this.Actor != null)
            {
                nick = this.Actor.NickName;
                if (string.IsNullOrEmpty(nick))
                {
                    nick = string.Concat("user ", this.Actor.ActorNumber);
                }
            }
            this.nameText.text = nick;
        }

        private void SetMutedState()
        {
            this.SetMutedState(this.Actor.IsMuted());
        }

        protected virtual void SetMutedState(bool isMuted)
        {
            this.remoteIsMuting.enabled = isMuted;
        }

        protected virtual void OnActorPropertiesChanged(Player targetPlayer, Hashtable changedProps)
        {
            if (targetPlayer.ActorNumber == this.Actor.ActorNumber)
            {
                this.SetMutedState();
                this.SetNickname();
            }
        }

        public virtual void Init(VoiceConnection vC)
        {
            this.voiceConnection = vC;
            this.loadBalancingClient = this.voiceConnection.Client;
            this.loadBalancingClient.AddCallbackTarget(this);
        }

        #region IInRoomCallbacks

        void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
        {
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            this.OnActorPropertiesChanged(targetPlayer, changedProps);
        }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
        }

        #endregion
    }
}