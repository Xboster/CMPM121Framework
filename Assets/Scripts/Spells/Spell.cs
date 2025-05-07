using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Spell
{
    public string name;
    public string description;
    public int icon;
    public Dictionary<string, string> damage;
    public int mana_cost;
    public string cooldown;
    public Dictionary<string, string> projectile;
    public float last_cast;

    public SpellCaster owner;
    public Hittable.Team team;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public void SetProperties(JObject attributes)
    {
        name = attributes["name"].ToString();
        description = attributes["description"].ToString();
        icon = attributes["icon"].ToObject<int>();
        damage = attributes["damage"].ToObject<Dictionary<string, string>>();
        mana_cost = attributes["mana_cost"].ToObject<int>();
        cooldown = attributes["cooldown"].ToString();
        projectile = attributes["projectile"].ToObject<Dictionary<string, string>>();
    }

    public string GetName()
    {
        return name;
    }

    public int GetManaCost()
    {
        return mana_cost;
    }

    public int GetDamage()
    {
        var variables = new Dictionary<string, float>
        {
            { "power", owner.spell_power }
        };
        return (int)RPNEvaluator.Evaluate(damage["amount"], variables);
    }

    public float GetCooldown()
    {
        return float.Parse(cooldown);
    }

    public virtual int GetIcon()
    {
        return icon;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }

    }

}
