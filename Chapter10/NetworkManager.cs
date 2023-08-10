using Photon.Pun;

using Photon.Realtime;

using Debug = UnityEngine.Debug;

using Firebase.Auth;

using UnityEngine.SceneManagement;

using UnityEngine;

using System.Collections;

using Firebase.Firestore;

using Photon.Pun.Demo.PunBasics;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    //Firebase Authentication SDK Instance

    FirebaseAuth auth;

    //We will use this flag variable to know the status of the connection.

    private bool isConnecting = false;

    //Photon, allows us to separate users by game versions, if there are users who have an older version of our game,

    //it will use this variable to group them, this way a player with an older version will not

    //enter a room with users who have the updated game.

    private const string GameVersion = "0.1";

    private const int MaxPlayersPerRoom = 100;

    //In this variable we will store the current scene we are in, this is very useful to know at any time where we are.

    public string actualRoom;

    // In the Awake method we will set up a Singleton instance of this script, to ensure that only one instance exists during the whole game.

    void Awake()
    {
        //Prevents Firebase from blocking the instance when testing with the Unity Editor and making it inaccessible from the cloned project.

#       if UNITY_EDITOR

        FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = false;

#endif
        FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = false;

        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        //Basic configuration recommended by Photon

        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.SendRate = 15;

        PhotonNetwork.SerializationRate = 15;

        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = "0.1";

        PhotonNetwork.GameVersion = "0.1";

        PhotonNetwork.GameVersion = GameVersion;

        //Launch the connection

        PhotonNetwork.ConnectUsingSettings();
    }

    private void Start()
    {
        Instance = this;
    }

    //We manage the change to another scene here

    public void JoinRoom(string sceneName)
    {
        isConnecting = true;

        actualRoom = sceneName;

        if (PhotonNetwork.IsConnected)
        {
            //We need to change scenes with Delay, with a Coroutine, as Photon has to change states and we cannot change scenes until the service is Ready.

            StartCoroutine(ChangeSceneWithDelay(sceneName));
        }
        else
        {
            Debug.LogError("Not connected to Photon");
        }
    }

    IEnumerator ChangeSceneWithDelay(string sceneName)
    {
        var roomOptions = new RoomOptions();

        var typedLobby = new TypedLobby(sceneName, LobbyType.Default);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        while (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log(PhotonNetwork.IsConnectedAndReady);

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log(PhotonNetwork.IsConnectedAndReady);

        yield return new WaitForSeconds(1f);

        PhotonNetwork.JoinOrCreateRoom(sceneName, roomOptions, typedLobby);
    }

    //If we decide to disconnect, we return to the Identification scene.

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();

        SceneManager.LoadScene("Welcome");
    }

    //When we have successfully connected to Photon, we set up the player's user name

    public override void OnConnectedToMaster()
    {
        Debug.Log("Has been connected");

        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        PhotonNetwork.LocalPlayer.NickName = currentUser.DisplayName;

        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconected due to: {cause}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients have made any rooms");
    }

    //Once we have successfully made the room change in Photon, we can change the scene to the lugger.

    public override void OnJoinedRoom()
    {
        Debug.Log("Client succesfully joined room");

        SceneManager.LoadSceneAsync(actualRoom);
    }

    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();

        Application.Quit();
    }
}
