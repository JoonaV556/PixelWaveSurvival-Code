public class EnemyHealth : Health
{
    protected override void Die()
    {
        // Inform about death
        GameEvents.OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }
}
