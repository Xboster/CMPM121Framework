using UnityEngine;
using System.Collections;

public class Spell
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    public string Name { get; set; }
    public int DamageAmount { get; set; }
    public Damage.Type DamageType { get; set; }
    public int ManaCost { get; set; }
    public float Cooldown { get; set; }
    public int IconIndex { get; set; }

    // Projectile properties
    public string ProjectileTrajectory { get; set; }
    public float ProjectileSpeed { get; set; }
    public int ProjectileSpriteIndex { get; set; }
    public float? ProjectileLifetime { get; set; }

    // Special behavior flags
    public int? SprayCount { get; set; }
    public float? SprayAngle { get; set; }
    public int? SecondaryCount { get; set; }
    public int SecondaryDamage { get; set; }
    public float? SecondarySpeed { get; set; }
    public float? SecondaryLifetime { get; set; }
    public float? DoubleCastDelay { get; set; }
    public float? SplitAngle { get; set; }

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public string GetName() => Name;
    public int GetManaCost() => ManaCost;
    public int GetDamage() => DamageAmount;
    public float GetCooldown() => Cooldown;
    public virtual int GetIcon() => IconIndex;

    public bool IsReady()
    {
        return (last_cast + Cooldown < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 origin, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        last_cast = Time.time;

        yield return CastOnce(origin, target);

        if (DoubleCastDelay.HasValue)
        {
            yield return new WaitForSeconds(DoubleCastDelay.Value);
            yield return CastOnce(origin, target);
        }
    }

    private IEnumerator CastOnce(Vector3 origin, Vector3 target)
    {
        Vector3 direction = (target - origin).normalized;
        int count = SprayCount ?? 1;
        float angleSpread = SprayAngle ?? 0f;

        for (int i = 0; i < count; i++)
        {
            float angleOffset = 0;
            if (count > 1)
            {
                angleOffset = -angleSpread + (2 * angleSpread) * i / (count - 1);
            }
            else if (SplitAngle.HasValue)
            {
                angleOffset = (i == 0) ? -SplitAngle.Value : SplitAngle.Value;
            }

            Vector3 rotatedDir = Quaternion.Euler(0, angleOffset, 0) * direction;
            SpawnProjectile(origin, rotatedDir, false);
        }

        yield return new WaitForEndOfFrame();
    }

    private void SpawnProjectile(Vector3 origin, Vector3 direction, bool isSecondary)
    {
        var callback = isSecondary ? new System.Action<Hittable, Vector3>(OnSecondaryHit)
                                   : new System.Action<Hittable, Vector3>(OnHit);

        GameManager.Instance.projectileManager.CreateProjectile(
            ProjectileSpriteIndex,
            ProjectileTrajectory,
            origin,
            direction,
            ProjectileSpeed,
            callback,
            ProjectileLifetime
        );
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), DamageType));

            if (SecondaryCount.HasValue && SecondaryCount.Value > 0 && SecondaryDamage > 0)
            {
                int count = SecondaryCount.Value;
                for (int i = 0; i < count; i++)
                {
                    float angle = (360f / count) * i;
                    Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    GameManager.Instance.projectileManager.CreateProjectile(
                        ProjectileSpriteIndex,
                        "straight",
                        impact,
                        dir,
                        SecondarySpeed ?? ProjectileSpeed,
                        OnSecondaryHit,
                        SecondaryLifetime
                    );
                }
            }
        }
    }

    void OnSecondaryHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(SecondaryDamage, DamageType));
        }
    }
}
