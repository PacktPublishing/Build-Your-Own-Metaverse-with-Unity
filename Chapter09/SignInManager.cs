using TMPro;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class SignInManager : MonoBehaviour
{
    //In these variables we will bind the scene inputs UsernameInput and PasswordInput in order to access their values later on
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    //Firebase Authentication SDK Instance
    FirebaseAuth auth;

    // Start is called before the first frame update
    void Start()
    {
        //When starting the script when the scene starts, we make sure that we get the Firebase SDK instances so that we can access its methods
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }
    }

    public void LoginUser()
    {
        //At this point, if the user has not completely filled out the registration form, we cannot continue.

        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            Debug.Log("Missing information in the login form");

            return;
        }

        //We call the SignInWithEmailAndPasswordAsync method of the SDK to sign in the user with the user and password we have entered.

        auth.SignInWithEmailAndPasswordAsync(usernameInput.text, passwordInput.text)
            .ContinueWithOnMainThread(async task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception
                    );

                    return;
                }

                //At this point, the user is correctly created.

                FirebaseUser existingUser = task.Result;

                Debug.LogFormat("User signed successfully: {0}", existingUser.UserId);

                //Once we have signed the user, we can change scene and go to our Meeting Point, the initial scene MainScene


                NetworkManager.Instance.JoinRoom("MainScene");
            });
    }
}
