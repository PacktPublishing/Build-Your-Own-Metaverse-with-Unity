using StarterAssets; 
using TMPro; 
using Unity.VisualScripting; 
using UnityEngine; 
using UnityEngine.EventSystems; 
using UnityEngine.InputSystem; 
using UnityEngine.SceneManagement; 

using static MyControls; 
using static UnityEngine.InputSystem.InputAction; 

public class TravelNPCTrigger : MonoBehaviour, IPlayerActions 
{ 
    //This constant will store the message we want to display when the player approaches, we will use it to create a line break. 
    const string NPC_TEXT = "Do you want to travel?\nPress 'E' key to interact"; 

    //In this variable we will bind the GameObject Message to access its properties in the code. 
    public TextMeshPro messageGameObject; 

    //In this variable we will bind the GameObject TravelWindow to be able to hide or show it. 
    public GameObject travelWindow; 

    //To prevent the window from appearing if we press the E key without being in the area of influence, we will use this flag 
    private bool canInteract; 

    MyControls controls; 

    private void Awake() 
    { 
        travelWindow.SetActive(false); 

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

    void Start() 
    { 
        //When the script starts up, we will set a default blank text 
        messageGameObject.text = string.Empty; 
    } 

    //If the Script is in a GameObject that has a Colllider with the Is Trigger property enabled, it will call this function when another GameObject comes into contact. 
    private void OnTriggerEnter(Collider other) 
    { 
        print(other.name); 

        //The player's Prefab comes with a default tag called Player, this is an excellent way to identify that it is a player and not another object. 
        if (other.gameObject.tag == "Player") 
        { 
            messageGameObject.text = NPC_TEXT; 
            canInteract = true; 
        } 
    } 

    //When the player leaves the area of influence, we will put a blank text back in.  
    private void OnTriggerExit(Collider other) 
    { 
        if (other.gameObject.tag == "Player") 
        { 
            messageGameObject.text = string.Empty; 
            canInteract = false; 
            travelWindow.SetActive(false); 
        } 
    }

    void Update() 
    { 
        //We check if the user has pressed the E key and is also inside the collider. 
        if (Input.GetKeyDown(KeyCode.E)  && canInteract) 
        { 
            travelWindow.SetActive(true); 

            //Allows you to see the mouse cursor 
            Cursor.lockState = CursorLockMode.None; 
        } 
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
            travelWindow.SetActive(true); 
        } 
    } 
} 