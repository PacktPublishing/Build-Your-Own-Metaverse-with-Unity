using System; 
using System.Collections; 
using System.Collections.Generic; 
using Firebase.Extensions; 
using Firebase.Firestore; 
using UnityEngine; 

[ExecuteAlways] 
public class BuildingInstanceManager : MonoBehaviour 
{ 
    //Firestore SDK Instance 
    FirebaseFirestore db; 

    //In these variables we will store the "unique" ID of the building, the name of the prefab we will use, 
    //the ID of the world where we will instantiate this building, and finally we will dedicate a variable to store 
    //the ID of the user who will be the owner of this building in the future. 
    public string buildingId; 
    public string prefabName; 
    public string worldId; 
    public string ownerUserId; 

    void Start() 
    { 
        //We launch here a method that will check if this building currently exists in the database, if so, it will be instantiated in the scene. 
        GetExistingBuilding(); 
    } 

    // Update is called once per frame 
    void Update() 
    { 

    } 

    //This function allows you to create a building in the Firestore database, if it already exists, we update it. 
    public void CreateOrUpdate() 
    { 
        //We check that we have instantiated the variable "db" that allows access to the Firestore functions, otherwise we instantiate it 
        if (db == null) 
        { 
            db = FirebaseFirestore.DefaultInstance; 
        } 

        //If we have forgotten to assign an ID to the building, we cannot continue. 
        if (string.IsNullOrEmpty(buildingId)) 
        { 
            Debug.LogError("Please, Click on Initialize Building button first"); 
            return; 
        } 

        //If we have forgotten to enter an ID of the world where this building will go or the Prefab name we want to use, we cannot continue. 
        if (string.IsNullOrEmpty(worldId) || string.IsNullOrEmpty(prefabName)) 
        { 
            Debug.LogError("Please check Prefab or WorldId properties before update this on Firebase"); 
            return; 
        } 

        //We create a reference to the "Buildings" collection with a new document that will have as ID the one we have generated for the building. 
        DocumentReference docRef = db.Collection("Buildings").Document(buildingId); 

        //We call the SetAsync method to write the changes to the database, we use the SetOptions.MergeAll 
        //option not to replace all the properties in the database, but the ones that have been modified. 
        docRef.SetAsync(GetBuilding(), SetOptions.MergeAll).ContinueWithOnMainThread(task => 
        { 
            if (task.IsCanceled) 
            { 
                Debug.LogError("Creating building was canceled."); 
                return; 
            } 

            if (task.IsFaulted) 
            { 
                Debug.LogError("Creating building encountered an error: " + task.Exception); 
                return; 
            } 

            Debug.Log("Building created or updated succesfully"); 
        }); 
    } 

    public void InitializeBuilding() 
    { 
        //We get a new unique ID for this building and assign it to the object in the scene 
        string uniqueId = Guid.NewGuid().ToString(); 
        gameObject.name = uniqueId; 
        gameObject.isStatic = true; 
        buildingId = uniqueId; 
    } 

    //When this script is started, we obtain from the database the building if it exists and instantiate it in the scene 
    public void GetExistingBuilding() 
    {
        if (db == null) 
        { 
            db = FirebaseFirestore.DefaultInstance; 
        } 

        //If there is already an instance of the building, We clean the previous Prefab instance, as it is possible that it has been changed in the database and we need a recent version.. 
        if (transform.childCount > 0) 
        { 
            Debug.Log("Building is already instantiated, cleaning."); 
            CleanPrefab(); 
        } 

        string buildingId = gameObject.name; 
        DocumentReference docRef = db.Collection("Buildings").Document(buildingId); 

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => 
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

            if (!buildingRef.Exists) 
            { 
                Debug.LogError("The Building with ID " + buildingId + " not exists in the Database, please create it first"); 
                return; 
            } 

            //We convert the Firestore document to Building class 
            Building building = buildingRef.ConvertTo<Building>(); 

            buildingId = building.Id; 
            prefabName = building.Prefab; 
            worldId = building.WorldId; 
            ownerUserId = building.OwnerUserId; 

            PlacePrefab(prefabName); 
        }); 
    } 

    private void CleanPrefab() 
    { 
        int childCount = transform.childCount; 

        for (int i = childCount - 1; i >= 0; i--) 
        { 
            DestroyImmediate(transform.GetChild(i).gameObject); 
        } 

        Debug.Log("Building prefab cleaned"); 
    } 

    //Using the name Prefab, we look for it in our Resources folder and instantiate it in the scene 
    private void PlacePrefab(string prefabName) 
    { 
        try 
        { 
            GameObject buildingInstance = Instantiate(Resources.Load("Prefabs/Buildings/" + prefabName), transform) as GameObject; 

            buildingInstance.transform.parent = transform; 
            buildingInstance.transform.localPosition = Vector3.down; 

            //Disabling Cube mesh 
            gameObject.GetComponent<MeshRenderer>().enabled = false; 
            Debug.Log("Placed building prefab."); 
        } 
        catch (Exception e) 
        { 
            Debug.LogError(e.Message); 
        } 
    } 

    //We create a Building class with the local data of the object 
    private Building GetBuilding() 
    { 
        return new Building() 
        { 
            Id = buildingId, 
            PosX = transform.position.x, 
            PosY = transform.position.y, 
            PosZ = transform.position.z, 
            Prefab = prefabName, 
            WorldId = worldId, 
            OwnerUserId = ownerUserId 
        }; 
    } 
} 