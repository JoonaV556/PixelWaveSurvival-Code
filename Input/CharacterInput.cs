using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    // General input class for characters and players
    // Sends input data to other scripts

    // Movement input direction
    public Vector2 MoveInput;

    public Vector2 LookInput
    {
        get { return LookInput.normalized; }
        set { }
    }

}
