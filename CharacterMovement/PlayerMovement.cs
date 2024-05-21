using UnityEngine;

public class PlayerMovement : CharacterMovement
{
    protected override void FetchInput()
    {
        // Fetch input 
        MoveInput = PlayerInput.Instance.MoveInput;
    }
}