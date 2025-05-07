using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;


public class ArcaneBlast : Spell
{
    Damage secondary_damage;
    Dictionary<string, string> secondary_projectile;
    public ArcaneBlast(SpellCaster owner, JToken attributes) : base(owner)
    {
        this.owner = owner;
        damage = new Damage(0, Damage.Type.PHYSICAL); // Initialize the damage field with default values
        secondary_damage = new Damage(0, Damage.Type.PHYSICAL); // Initialize the secondary_damage field with default values

        SetProperties((JObject)attributes);
    }

    public override void SetProperties(JObject attributes)
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
        if (attributes["secondary_damage"] != null)
        {
            if (attributes["damage"]["type"] != null)
                secondary_damage.type = (Damage.Type)System.Enum.Parse(typeof(Damage.Type), attributes["damage"]["type"].ToString(), true);
            if (attributes["secondary_damage"] != null)
                secondary_damage.amount = (int)RPNEvaluator.Evaluate(attributes["secondary_damage"].ToString(), variables);
        }
        if (attributes["mana_cost"] != null)
            mana_cost = (int)RPNEvaluator.Evaluate(attributes["mana_cost"].ToString(), variables);
        if (attributes["cooldown"] != null)
            cooldown = float.Parse(attributes["cooldown"].ToString());
        if (attributes["projectile"] != null)
            projectile = attributes["projectile"].ToObject<Dictionary<string, string>>();
        if (attributes["secondary_projectile"] != null)
            secondary_projectile = attributes["secondary_projectile"].ToObject<Dictionary<string, string>>();
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;

        var variables = new Dictionary<string, float>
        {
            { "power", owner.spell_power }
        };

        Debug.Log("Power: " + owner.spell_power);
        int speed = (int)RPNEvaluator.Evaluate(projectile["speed"].ToString(), variables);
        Vector3 direction = target - where;

        GameManager.Instance.projectileManager.CreateProjectile(int.Parse(projectile["sprite"]), projectile["trajectory"], where, direction, speed, OnHit);

        yield return new WaitForEndOfFrame();
    }
    public override void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), damage.type));


            float angleStep = 360f / shots; // Angle between each projectile

            for (int i = 0; i < shots; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;

                var variables = new Dictionary<string, float>
                {
                    { "power", owner.spell_power }
                };

                int speed = (int)RPNEvaluator.Evaluate(secondary_projectile["speed"].ToString(), variables);
                float lifetime = (float)RPNEvaluator.Evaluate(secondary_projectile["lifetime"].ToString(), variables);
                GameManager.Instance.projectileManager.CreateProjectile(
                    int.Parse(secondary_projectile["sprite"]),
                    secondary_projectile["trajectory"],
                    impact,
                    direction,
                    speed,
                    (childOther, childImpactPoint) =>
                    {
                        if (childOther.team != team)
                        {
                            childOther.Damage(new Damage(secondary_damage.amount, secondary_damage.type));
                        }
                    },
                    lifetime);
            }


        }
    }
}