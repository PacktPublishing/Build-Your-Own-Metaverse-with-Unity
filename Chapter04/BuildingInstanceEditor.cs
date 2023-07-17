#if UNITY_EDITOR 
using UnityEngine; 
using System.Collections; 
using UnityEditor; 

[CustomEditor(typeof(BuildingInstanceManager))] 
public class BuildingInstanceEditor : Editor 
{ 
    public override void OnInspectorGUI() 
    { 
        DrawDefaultInspector(); 

        BuildingInstanceManager myScript = (BuildingInstanceManager)target; 

        if (GUILayout.Button("Initialize Building")) 
        { 
            myScript.InitializeBuilding(); 
        } 

        if (GUILayout.Button("Create Or Update in Firestore")) 
        { 
            myScript.CreateOrUpdate(); 
        } 
    } 
} 
#endif 