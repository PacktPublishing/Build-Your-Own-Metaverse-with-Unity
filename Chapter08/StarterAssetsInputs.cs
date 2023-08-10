using UnityEngine; 

#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem; 
#endif 

namespace StarterAssets 
{ 
    public class StarterAssetsInputs : MonoBehaviour 
    { 
        [Header("Character Input Values")] 
        public Vector2 move; 
        public Vector2 look; 
        public bool jump; 
        public bool sprint; 

        //Add this new variable 
        public bool interact; 

        [Header("Movement Settings")] 
        public bool analogMovement; 

        [Header("Mouse Cursor Settings")] 
        public bool cursorLocked = true; 
        public bool cursorInputForLook = true; 

        #if ENABLE_INPUT_SYSTEM 
        public void OnMove(InputValue value) 
        { 
            MoveInput(value.Get<Vector2>()); 
        } 

        public void OnLook(InputValue value) 
        { 
            if(cursorInputForLook) 
            { 
                LookInput(value.Get<Vector2>()); 
            } 
        } 

        public void OnJump(InputValue value) 
        { 
            JumpInput(value.isPressed); 
        } 

        public void OnSprint(InputValue value) 
        { 
            SprintInput(value.isPressed); 
        } 

        public void OnInteract(InputValue value) 
        { 
            InteractInput(value.isPressed); 
        } 
        #endif 


        public void InteractInput(bool newInteractState) 
        { 
            interact = newInteractState; 
        } 
...
    }
}