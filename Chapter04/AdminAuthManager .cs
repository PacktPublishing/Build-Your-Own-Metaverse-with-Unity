using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using Firebase.Auth; 

[ExecuteInEditMode] 
public class AdminAuthManager : MonoBehaviour 
{ 
    public string username; 
    public string password; 

    FirebaseAuth auth; 

    // Start is called before the first frame update 
    void Start() 
    { 

    } 

    // Update is called once per frame 
    void Update() 
    { 
        InitializeFirebase(); 
    } 

    void InitializeFirebase() 
    { 
        //If the auth variable is not instantiated or the user's session has expired, we re-identify ourselves. 
        if (auth == null || auth?.CurrentUser == null) 
        { 
            auth = FirebaseAuth.DefaultInstance; 
            Login(); 
        } 
    } 

    void Login() 
    { 
        //We call the SignInWithEmailAndPasswordAsync method of the SDK to identify ourselves with the user and password that we will store in the previously declared variables.  
        auth.SignInWithEmailAndPasswordAsync(username, password).ContinueWith(task => 
        { 
            if (task.IsCanceled) 
            { 
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled."); 
                return; 
            } 

            if (task.IsFaulted) 
            { 
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception); 
                return; 
            } 

            //At this point, the user is correctly identified. 
            FirebaseUser newUser = task.Result; 

            Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId); 
        }); 
    } 
} 