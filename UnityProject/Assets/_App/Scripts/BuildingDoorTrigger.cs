using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using Models;
using Nethereum.Signer;
using Nethereum.Util;
using UnityEngine;
using Web3Unity.Scripts.Library.ETHEREUEM.Connect;
using Web3Unity.Scripts.Library.Ethers.Network;
using Web3Unity.Scripts.Library.IPFS;
using Web3Unity.Scripts.Library.Web3Wallet;
using static MyControls;
using static UnityEngine.InputSystem.InputAction;
using static Web3Unity.Scripts.Library.Ethers.Network.Chains;

public class BuildingDoorTrigger : MonoBehaviour, IPlayerActions
{
    private const string apiKey = "eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE2ODc0NTQwNTUsImNuZiI6eyJqa3UiOiIvY2VydHMiLCJraWQiOiI5aHE4bnlVUWdMb29ER2l6VnI5SEJtOFIxVEwxS0JKSFlNRUtTRXh4eGtLcCJ9LCJ0eXBlIjoiYXBpX3NlY3JldCIsImlkIjoxMjk0MiwidXVpZCI6IjBlODdmOThiLWFjNDktNDBlOC1hMWU2LTUzYjYzOGE4Y2NiZCIsInBlcm0iOnsiYmlsbGluZyI6IioiLCJzZWFyY2giOiIqIiwic3RvcmFnZSI6IioiLCJ1c2VyIjoiKiJ9LCJhcGlfa2V5IjoiUEZMSVlZQ1VKT1BDVlNZQkpGU1UiLCJzZXJ2aWNlIjoic3RvcmFnZSIsInByb3ZpZGVyIjoiIn0.qk6lPZqZpqc1hbp09OGKrWwQfJk1gYt5DkUeejKw1UXJzaSlSt9HwOL4jmsxd8FfMF0O3uVe1YYLKYiddi9iXQ";

    //Firestore SDK Instance
    FirebaseFirestore db;
    //Firebase Auth SDK Instance
    FirebaseAuth auth;

    //In these constants we define the different messages we want to display to the user.
    const string BUSY_BUILDING_TEXT = "Sorry\nThis building is occupied and cannot be adquired";
    const string ADQUIRE_BUILDING_TEXT = "Do you want to adquire this building?\nPress 'C' key to adquire it";
    const string CONFIRMATION_TEXT = "Congratulations, you are now the owner of this building!";
    const string YOU_ARE_THE_OWNER_TEXT = "You are the owner of this building ;)";

    //In this variable we will store the current message we want to display.
    string displayMessage = string.Empty;

    //To prevent the window from appearing if we press the E key without being in the area of influence, we will use this flag
    private bool canInteract = false;

    //Here we will store information about the building when we consult it in the database.
    private Building building;

    MyControls controls;

    private void Awake()
    {
        //We check that we have instantiated the variable "db" that allows access to the Firestore functions, otherwise we instantiate it
        if (db == null) db = FirebaseFirestore.DefaultInstance;

        if (auth == null) auth = FirebaseAuth.DefaultInstance;

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

    //If the Script is in a GameObject that has a Colllider with the Is Trigger property enabled, it will call this function when another GameObject comes into contact.
    private void OnTriggerEnter(Collider other)
    {
        //The player's Prefab comes with a default tag called Player, this is an excellent way to identify that it is a player and not another object.
        if (other.gameObject.tag == "Player")
        {
            //This function will perform the database query
            GetBuildingInfo();
        }
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


    void Update()
    {
        //We check if the user has pressed the C key and is also inside the collider.
        if (Input.GetKeyDown(KeyCode.C) && canInteract)
        {
            //This function will update the building in the database with our ID
            UpdateBuildingOwner();
        }
    }

    void OnGUI()
    {
        //We display a text on the screen
        GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200f, 200f), displayMessage);
    }

    void GetBuildingInfo()
    {
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        //We get building data from the BuildingInstanceManager component which is located in the parent of this object.
        var buildingInfo = GetComponentInParent<BuildingInstanceManager>();

        //Create an instance to the Firestore Buildings Collection
        DocumentReference docRef = db.Collection("Buildings").Document(buildingInfo.buildingId);

        docRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Obtaining building was canceled.");
                return;

            }
            if (task.IsFaulted)
            {
                Debug.LogError("Obtaining building encountered an error: " + task.Exception);
                return;
            }

            Debug.Log("Building obtanined succesfully");


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


        docRef.UpdateAsync(updatedProperties).ContinueWith(task =>
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

    public void OnMove(CallbackContext context)
    {

    }

    public void OnLook(CallbackContext context)
    {

    }

    public void OnJump(CallbackContext context)
    {

    }

    public void OnSprint(CallbackContext context)
    {

    }

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
        var timestamp = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // set expiration time
        var expirationTime = timestamp + 60;
        // set message
        var message = expirationTime.ToString();
        // sign message
        var signature = await Web3Wallet.Sign(message);
        // verify account
        var account = SignVerifySignature(signature, message);
        var now = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // validate
        if (account.Length == 42 && expirationTime >= now)
        {
            print("Account: " + account);
            await UploadIPFS();
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

        var cid = await ipfs.Upload(bucketId, folderName, fileName, bArray, "application/octet-stream");

        return $"{cid}";
    }

    public async void MintNft()
    {
        var voucherResponse721 = await EVM.Get721Voucher();
        CreateRedeemVoucherModel.CreateVoucher721 voucher721 = new CreateRedeemVoucherModel.CreateVoucher721();
        voucher721.tokenId = voucherResponse721.tokenId;
        voucher721.minPrice = voucherResponse721.minPrice;
        voucher721.signer = voucherResponse721.signer;
        voucher721.receiver = voucherResponse721.receiver;
        voucher721.signature = voucherResponse721.signature;

        // set chain: ethereum, moonbeam, polygon etc
        string chain = "ethereum";
        // chain id
        string chainId = "5";
        // set network mainnet, testnet
        string network = "goerli";
        // type
        string type = "721";

        string voucherArgs = JsonUtility.ToJson(voucher721);
        var nft = await UploadIPFS();
        // connects to user's browser wallet to call a transaction
        RedeemVoucherTxModel.Response voucherResponse = await EVM.CreateRedeemTransaction(chain, network, voucherArgs, type, nft, voucherResponse721.receiver);
        string response = await Web3Wallet.SendTransaction(chainId, voucherResponse.tx.to, voucherResponse.tx.value.ToString(), voucherResponse.tx.data, voucherResponse.tx.gasLimit, voucherResponse.tx.gasPrice);
        print("My NFT Address: " + response);
    }

}
