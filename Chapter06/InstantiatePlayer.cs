using UnityEngine; 

public class InstantiatePlayer : MonoBehaviour 
{ 
    public GameObject playerPrefab; 

    // Start is called before the first frame update 
    void Start() 
    { 
        Instantiate(playerPrefab, transform); 
    } 
} 