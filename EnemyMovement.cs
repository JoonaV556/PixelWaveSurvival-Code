public class EnemyMovement : CharacterMovement
{
    public CharacterInput input;

    protected override void FetchInput()
    {
        MoveInput = input.MoveInput;
    }
}