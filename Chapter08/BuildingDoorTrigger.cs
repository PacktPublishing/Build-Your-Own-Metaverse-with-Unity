using UnityEngine;
using static MyControls;
using static UnityEngine.InputSystem.InputAction;
using Firebase.Auth;
using Firebase.Firestore;

public class BuildingDoorTrigger : MonoBehaviour, IPlayerActions
{
    private const string apiKey =
        "eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE2ODc0NTQwNTUsmNuZiI6eyJqa3UiOiIvY2VydHMiLCJraWQiOiI5aHE4bnlVUWdMb29ER2l6VnI5SEJtOFIxVEwxS0JKSFlNRUtTRXh4eGtLcCJ9LCJ0eXBlIjoiYXBpX3NlY3JldCIsImlkIjoxMjk0MiwidXVpZCI6IjBlODdmOThiLWFjNDktNDBlOC1hMWU2LTUzYjYzOGE4Y2NiZCIsInBlcm0iOnsiYmlsbGluZyI6IioiLCJzZWFyY2giOiIqIiwic3RvcmFnZSI6IioiLCJ1c2VyIjoiKiJ9LCJhcGlfa2V5IjoiUEZMSVlZQ1VKT1BDVlNZQkpGU1UiLCJzZXJ2aWNlIjoic3RvcmFnZSIsInByb3ZpZGVyIjoiIn0.qk6lPZqZpqc1hbp09OGKrWwQfJk1gYt5DkUeejKw1UXJzaSlSt9HwOL4jmsxd8FfMF0O3uVe1YYLKYiddi9";

    FirebaseFirestore db;

    FirebaseAuth auth;

    const string BUSY_BUILDING_TEXT = "Sorry\nThis building is occupied and cannot be adquired";

    const string YOU_ARE_THE_OWNER_TEXT = "You are the owner of this building ;)";

    const string ADQUIRE_BUILDING_TEXT =
        "Do you want to adquire this building?\nPress 'C' key to adquire it";

    string displayMessage = string.Empty;

    private bool canInteract = false;

    private Building building;
    MyControls controls;

    private void Awake()
    {
        if (db == null)
            db = FirebaseFirestore.DefaultInstance;

        if (auth == null)
            auth = FirebaseAuth.DefaultInstance;
        //We link the Interact action and enable it for detection in the code.

        if (controls == null)
        {
            controls = new MyControls();

            // Tell the "gameplay" action map that we want to get told about

            // when actions get triggered.

            controls.Player.SetCallbacks(this);
        }

        controls.Player.Enable();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && canInteract)
        {
            //This function will update the building in the database with our ID

            UpdateBuildingOwner();
        }
    }

    public void OnInteract(CallbackContext context)
    {
        if (context.action.triggered && canInteract) { }
    }

    //If the Script is in a GameObject that has a Colllider with the Is Trigger property enabled, it will call this function when another GameObject comes into contact.

    private void OnTriggerEnter(Collider other)
    {
        //The player's Prefab comes with a default tag called Player, this is an excellent way to identify that it is a player and not another object.

        if (other.gameObject.tag == "Player")
            GetBuildingInfo();
    }

    //When the player leaves the area of influence, we will put a blank text back in.

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            displayMessage = string.Empty;

            canInteract = false;
        }
    }

    void GetBuildingInfo()
    {
        if (db == null)
            db = FirebaseFirestore.DefaultInstance;

        var buildingInfo = GetComponentInParent<BuildingInstanceManager>();

        DocumentReference docRef = db.Collection("Buildings").Document(buildingInfo.buildingId);

        docRef
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Obtaining building encountered an error");

                    return;
                }

                DocumentSnapshot buildingRef = task.Result;

                building = buildingRef.ConvertTo<Building>();

                //No current owner

                if (string.IsNullOrEmpty(building.OwnerUserId))
                {
                    displayMessage = ADQUIRE_BUILDING_TEXT;

                    canInteract = true;
                }
                //I am the owner

                else if (auth.CurrentUser.UserId == building.OwnerUserId)
                {
                    displayMessage = YOU_ARE_THE_OWNER_TEXT;

                    canInteract = false;
                }
                //Another user is the owner

                else
                {
                    displayMessage = BUSY_BUILDING_TEXT;

                    canInteract = false;
                }
            });
    }

    public void UpdateBuildingOwner()
    {
        //If we have forgotten to assign the building, we cannot continue.

        if (building == null)
        {
            Debug.LogError("Building object is null, cannot continue");

            return;
        }

        //We create a reference to the "Buildings" collection with a new document that will have as ID the one we have generated for the building.

        DocumentReference docRef = db.Collection("Buildings").Document(building.Id);

        //We call the UpdateAsync method to write the changes to the database, we use the SetOptions.MergeAll

        //option not to replace all the properties in the database, but the ones that have been modified.



        Dictionary<string, object> updatedProperties = new Dictionary<string, object>();

        updatedProperties["OwnerUserId"] = auth?.CurrentUser.UserId;

        docRef
            .UpdateAsync(updatedProperties)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Update building encountered an error");

                    return;
                }

                Debug.Log("Building created or updated succesfully");

                displayMessage = CONFIRMATION_TEXT;

                canInteract = false;

                LoginNft();
            });
    }

    public void OnMove(CallbackContext context) { }

    public void OnLook(CallbackContext context) { }

    public void OnJump(CallbackContext context) { }

    public void OnSprint(CallbackContext context) { }

    public void OnInteract(CallbackContext context)
    {
        if (context.action.triggered && canInteract)
        {
            //This function will update the building in the database with our ID

            UpdateBuildingOwner();
        }
    }

    public async void LoginNft()
    {
        Web3Wallet.url = "https://chainsafe.github.io/game-web3wallet/";
        // get current timestamp

        var timestamp = (int)
            System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;

        // set expiration time

        var expirationTime = timestamp + 60;

        // set message

        var message = expirationTime.ToString();

        // sign message

        var signature = await Web3Wallet.Sign(message);

        // verify account

        var account = SignVerifySignature(signature, message);

        var now = (int)
            System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;

        // validate

        if (account.Length == 42 && expirationTime >= now)
        {
            PlayerPrefs.SetString("Account", account);
            MintNft();
        }
    }

    public string SignVerifySignature(string signatureString, string originalMessage)
    {
        var msg = "Ethereum Signed Message:\n" + originalMessage.Length + originalMessage;

        var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));

        var signature = MessageSigner.ExtractEcdsaSignature(signatureString);

        var key = EthECKey.RecoverFromSignature(signature, msgHash);

        return key.GetPublicAddress();
    }

    private async Task<string> UploadIPFS()
    {
        var capture = ScreenCapture.CaptureScreenshotAsTexture();

        byte[] bArray = capture.GetRawTextureData();

        var ipfs = new IPFS(apiKey);

        var bucketId = "5d9c59c9-be7a-4fbc-9f1e-b107209437be";

        var folderName = "/MyFolder";

        var fileName = "MyCapture.jpg";

        var cid = await ipfs.Upload(
            bucketId,
            folderName,
            fileName,
            bArray,
            "application/octet-stream"
        );

        return $"{cid}";
    }

    public async void MintNft()
    {
        var account = PlayerPrefs.GetString("Account");
        // set chain: ethereum, moonbeam, polygon etc
        string chain = "ethereum";
        // chain id
        string chainId = "5";
        // set network mainnet, testnet
        string network = "goerli";
        // type
        string type = "721";

        var response = await EVM.CreateApproveTransaction(chain, network, account, type);
        Debug.Log("Response: " + response.connection.chain);

        string responseNft = await Web3Wallet.SendTransaction(chainId, response.tx.to, "0",
            response.tx.data, response.tx.gasLimit, response.tx.gasPrice);
        if (responseNft == null)
        {
            Debug.Log("Empty Response Object:");
        }

        print("My NFT Address: " + responseNft);
    }
}
