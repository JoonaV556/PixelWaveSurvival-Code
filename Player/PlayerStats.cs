using UnityEngine;

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
        GameEvents.OnEnemyDroppedXpAndExperience += HandleXpAndCashDropped;
    }

    // Destructor
    ~PlayerStats()
    {
        // Cleanup code here
        GameEvents.OnEnemyDroppedXpAndExperience -= HandleXpAndCashDropped;
    }

    private void HandleXpAndCashDropped(float xp, int cash)
    {
        AddExperience(xp);
        AddCash(cash);
        Debug.Log("PlayerStats: Player received " + xp + " experience and " + cash + " cash");
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
