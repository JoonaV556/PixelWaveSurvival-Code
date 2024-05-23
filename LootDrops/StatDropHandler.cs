using UnityEngine;


// Abstract class for handling various types of drops???
// public abstract class DropHandler : MonoBehaviour
// {
// 
// }

public class StatDropHandler : MonoBehaviour
{
    // Throws event to drop loot when enemy dies

    // TODO - Add enemy drop database integration to retrieve drop data from

    public float ExperienceToDrop = 50f;
    public int CashToDrop = 100;

    private void OnEnable()
    {
        GameEvents.OnEnemyDied += DropLoot;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= DropLoot;
    }
    public void DropLoot(EnemyHealth enemyHealth)
    {
        // Check if the enemy that died is the same as the one this script is attached to
        if (enemyHealth == gameObject.GetComponent<EnemyHealth>())
        {
            GameEvents.OnEnemyDroppedXpAndExperience?.Invoke(ExperienceToDrop, CashToDrop);
        }
    }
}