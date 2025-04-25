using UnityEngine;

public class DamageTracker : MonoBehaviour
{
    public int totalDamageTaken = 0;

    void Start()
    {
        EventBus.Instance.OnDamage += TrackPlayerDamage;
    }

    void OnDestroy()
    {
        EventBus.Instance.OnDamage -= TrackPlayerDamage;
    }

    void TrackPlayerDamage(Vector3 where, Damage dmg, Hittable target)
    {
        // Check if the damaged entity is the player
        if (target == GameManager.Instance.player.GetComponent<Hittable>())
        {
            totalDamageTaken += dmg.amount;
            Debug.Log($"Player took {dmg.amount} damage! Total: {totalDamageTaken}");
        }
    }

    public int GetTotalDamageTaken()
    {
        return totalDamageTaken;
    }

    public void ResetDamage()
    {
        totalDamageTaken = 0;
    }
}
