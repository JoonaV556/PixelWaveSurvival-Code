/// <summary>
/// Tracks player stats such as experience and cash.
/// </summary>
public class PlayerStats
{
    public float Experience { get; private set; }
    public float Cash { get; private set; }


    public PlayerStats()
    {
        Experience = 0f;
        Cash = 0f;
        Initialize();
    }

    /// <summary>
    /// Override to add custom functionality
    /// </summary>
    protected virtual void Initialize()
    {
        // Subscribe to events to react to enemy kills etc.
        // Enemy.OnEnemykilled += AddExperience;
        // Enemy.OnEnemykilled += AddCash;
    }

    public void AddExperience(float amount)
    {
        Experience += amount;
    }

    public void AddCash(float amount)
    {
        Cash += amount;
    }
}
