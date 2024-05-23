using System;

// Contains core game events
// Invoke these events form anywhere
// Listen from anywhere
public class GameEvents
{
    public static Action<EnemyHealth> OnEnemyDied;
    public static Action<float, int> OnEnemyDroppedXpAndExperience;
}