namespace Photon.Voice.Unity.Demos.DemoVoiceUI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    public struct MicRef
    {
        public readonly Recorder.MicType MicType;
        public readonly DeviceInfo Device;

        public MicRef(Recorder.MicType micType, DeviceInfo device)
        {
            this.MicType = micType;
            this.Device = device;
        }

        public override string ToString()
        {
            return string.Format("Mic reference: {0}", this.Device.Name);
        }
    }

    public class MicrophoneDropdownFiller : VoiceComponent
    {
        private List<MicRef> micOptions;

#pragma warning disable 649
        [SerializeField]
        private Dropdown micDropdown;

        [SerializeField]
        private Recorder recorder;

        [SerializeField]
        [FormerlySerializedAs("RefreshButton")]
        private GameObject refreshButton;

#pragma warning restore 649

        private IDeviceEnumerator unityMicEnum;
        private IDeviceEnumerator photonMicEnum;

        protected override void Awake()
        {
            base.Awake();
            unityMicEnum = new AudioInEnumerator(this.Logger);
            photonMicEnum = Platform.CreateAudioInEnumerator(this.Logger);
            this.RefreshMicrophones();
            this.refreshButton.GetComponentInChildren<Button>().onClick.AddListener(RefreshMicrophones);
        }

        private void OnEnable()
        {
            UtilityScripts.MicrophonePermission.MicrophonePermissionCallback += this.OnMicrophonePermissionCallback;
        }

        private void OnMicrophonePermissionCallback(bool granted)
        {
            this.RefreshMicrophones();
        }

        private void OnDisable()
        {
            UtilityScripts.MicrophonePermission.MicrophonePermissionCallback -= this.OnMicrophonePermissionCallback;
        }

        private void SetupMicDropdown()
        {
            this.micDropdown.ClearOptions();

            this.micOptions = new List<MicRef>();
            List<string> micOptionsStrings = new List<string>();

            this.micOptions.Add(new MicRef(Recorder.MicType.Unity, DeviceInfo.Default));
            micOptionsStrings.Add(string.Format("[Unity] [Default]"));

            foreach (var d in this.unityMicEnum)
            {
                this.micOptions.Add(new MicRef(Recorder.MicType.Unity, d));
                micOptionsStrings.Add(string.Format("[Unity] {0}", d));
            }

            this.micOptions.Add(new MicRef(Recorder.MicType.Photon, DeviceInfo.Default));
            micOptionsStrings.Add(string.Format("[Photon] [Default]"));

            foreach (var d in this.photonMicEnum)
            {
                this.micOptions.Add(new MicRef(Recorder.MicType.Photon, d));
                micOptionsStrings.Add(string.Format("[Photon] {0}", d));
            }

            this.micDropdown.AddOptions(micOptionsStrings);
            this.micDropdown.onValueChanged.RemoveAllListeners();
            this.micDropdown.onValueChanged.AddListener(delegate { this.MicDropdownValueChanged(this.micOptions[this.micDropdown.value]); });
        }

        private void MicDropdownValueChanged(MicRef mic)
        {
            this.recorder.MicrophoneType = mic.MicType;
            this.recorder.MicrophoneDevice = mic.Device;
        }

        private void SetCurrentValue()
        {
            if (this.micOptions == null)
            {
                Debug.LogWarning("micOptions list is null");
                return;
            }
            this.micDropdown.gameObject.SetActive(true);
            this.refreshButton.SetActive(true);
            for (int valueIndex = 0; valueIndex < this.micOptions.Count; valueIndex++)
            {
                MicRef val = this.micOptions[valueIndex];
                if (this.recorder.MicrophoneType == val.MicType)
                {
                    if (this.recorder.MicrophoneType == val.MicType &&
                        this.recorder.MicrophoneDevice == val.Device)
                    {
                        this.micDropdown.value = valueIndex;
                        return;
                    }
                }
            }
        }

        public void PhotonMicToggled(bool on)
        {
            this.micDropdown.gameObject.SetActive(!on);
            this.refreshButton.SetActive(!on);
            if (on)
            {
                this.recorder.MicrophoneType = Recorder.MicType.Photon;
            }
            else
            {
                this.recorder.MicrophoneType = Recorder.MicType.Unity;
            }
        }

        public void RefreshMicrophones()
        {
            this.unityMicEnum.Refresh();
            this.photonMicEnum.Refresh();
            this.SetupMicDropdown();
            this.SetCurrentValue();
        }

        // sync. UI in case a change happens from the Unity Editor Inspector
        private void PhotonVoiceCreated()
        {
            this.RefreshMicrophones();
        }
    }
}