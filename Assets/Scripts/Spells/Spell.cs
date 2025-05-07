using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Mono.Cecil.Cil;
using UnityEngine.Rendering;

public class Spell
{
    public string name;
    public string description;
    public int icon;
    public float spray;
    public int shots;
    public Damage damage;
    public int mana_cost;
    public float cooldown;
    public Dictionary<string, string> projectile;
    public float last_cast;

    public SpellCaster owner;
    public Hittable.Team team;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
        damage = new Damage(0, Damage.Type.PHYSICAL); // Initialize the damage field with default values
    }

    public virtual void SetProperties(JObject attributes)
    {
        var variables = new Dictionary<string, float>
        {
            { "power", owner.spell_power },
            { "wave", GameManager.Instance.wave }
        };

        if (attributes["name"] != null)
            name = attributes["name"].ToString();
        if (attributes["description"] != null)
            description = attributes["description"].ToString();
        if (attributes["icon"] != null)
            icon = attributes["icon"].ToObject<int>();
        if (attributes["spray"] != null)
            spray = attributes["spray"].ToObject<float>();
        if (attributes["N"] != null)
            shots = (int)RPNEvaluator.Evaluate(attributes["N"].ToString(), variables);
        else
            shots = 1;
        if (attributes["damage"] != null)
        {
            if (attributes["damage"]["type"] != null)
                damage.type = (Damage.Type)System.Enum.Parse(typeof(Damage.Type), attributes["damage"]["type"].ToString(), true);
            if (attributes["damage"]["amount"] != null)
                damage.amount = (int)RPNEvaluator.Evaluate(attributes["damage"]["amount"].ToString(), variables);
        }
        if (attributes["mana_cost"] != null)
            mana_cost = (int)RPNEvaluator.Evaluate(attributes["mana_cost"].ToString(), variables);
        if (attributes["cooldown"] != null)
            cooldown = float.Parse(attributes["cooldown"].ToString());
        if (attributes["projectile"] != null)
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
        return damage.amount;
    }

    public float GetCooldown()
    {
        return cooldown;
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

        var variables = new Dictionary<string, float>
        {
            { "power", owner.spell_power }
        };

        Debug.Log("Power: " + owner.spell_power);
        int speed = (int)RPNEvaluator.Evaluate(projectile["speed"].ToString(), variables);
        Vector3 direction = target - where;

        for (int i = 0; i < shots; i++)
        {
            direction = target - where;
            if (spray > 0)
            {
                // Debug.Log("Spray: " + spray);
                // Randomize the shots direction    
                float angle = Mathf.Atan2(direction.y, direction.x);
                angle += Random.value * spray - spray / 2;
                direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), direction.z);
            }
            if (projectile.ContainsKey("lifetime"))
            {
                float lifetime = (float)RPNEvaluator.Evaluate(projectile["lifetime"].ToString(), variables);
                GameManager.Instance.projectileManager.CreateProjectile(int.Parse(projectile["sprite"]), projectile["trajectory"], where, direction, speed, OnHit, lifetime);
            }
            else
            {
                GameManager.Instance.projectileManager.CreateProjectile(int.Parse(projectile["sprite"]), projectile["trajectory"], where, direction, speed, OnHit);
            }
        }
        yield return new WaitForEndOfFrame();
    }

    public virtual void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), damage.type));
        }

    }

}
