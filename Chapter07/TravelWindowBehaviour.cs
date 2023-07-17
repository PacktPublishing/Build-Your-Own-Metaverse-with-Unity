using System.Collections; 
using System.Collections.Generic; 
using Firebase.Extensions; 
using Firebase.Firestore; 
using UnityEngine; 

public class TravelWindowBehaviour : MonoBehaviour 
{ 
    //Firestore SDK Instance 
    FirebaseFirestore db; 

    //In this variable we bind the GameObject Panel, that is the window 
    public GameObject travelWindow; 

    //In this variable we will bind the GameObject Content, which is the parent element of the scrollable items. 
    public GameObject scrollContent; 

    //In this variable we will bind the Prefab WorldItem 
    public GameObject worldItemPrefab; 

    //When the script is instantiated, we will call the GetWorlds method to get a query on the Worlds collection. 
    void Start() 
    { 
        GetWorlds(); 
    } 

    void GetWorlds() 
    { 
        if (db == null) 
        { 
            db = FirebaseFirestore.DefaultInstance; 
        } 

        //Create an instance to the Firestore Worlds Collection 
        CollectionReference docRef = db.Collection("Worlds"); 

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => 
        { 
            if (task.IsCanceled) 
            { 
                Debug.LogError("Obtaining worlds was canceled."); 
                return; 
            } 

            if (task.IsFaulted) 
            { 
                Debug.LogError("Obtaining worlds encountered an error: " + task.Exception); 
                return; 
            } 

            Debug.Log("Worlds obtanined succesfully"); 

            QuerySnapshot worldsRef = task.Result; 

            //For each document found in the Worlds collection, we convert the object to the World class and call the AttachWorldItemScroll method to add it to the scroll. 
            foreach (var worldDocument in worldsRef.Documents) 
            { 
                World world = worldDocument.ConvertTo<World>(); 
                AttachWorldItemToScroll(world); 
            } 
        }); 
    } 

    void AttachWorldItemToScroll(World world) 
    { 
        try 
        { 
            GameObject worldItemInstance = Instantiate(worldItemPrefab, scrollContent.transform) as GameObject; 

            worldItemInstance.transform.parent = scrollContent.transform; 
            worldItemInstance.transform.localPosition = Vector3.down; 

            //We configure the variables of the script associated to the Prefab 
            worldItemInstance.GetComponent<WorldItemBehaviour>().sceneName = world.Scene; 
            worldItemInstance.GetComponent<WorldItemBehaviour>().worldName = world.Name; 
        } 
        catch (System.Exception ex) 
        { 
            Debug.LogError(ex.Message); 
        } 
    } 

    public void CloseWindow() 
    { 
        travelWindow.SetActive(false); 

        //Hide mouse cursor 
        Cursor.lockState = CursorLockMode.Locked; 
    } 
}  