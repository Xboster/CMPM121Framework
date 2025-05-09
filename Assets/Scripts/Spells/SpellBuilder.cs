using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SpellBuilder
{
    private JObject spellData;
    private List<string> baseSpellKeys;
    private List<string> modifierKeys;

    public SpellBuilder()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "spells.json");
        string jsonText = File.ReadAllText(jsonPath);
        spellData = JsonConvert.DeserializeObject<JObject>(jsonText);

        baseSpellKeys = new List<string>();
        modifierKeys = new List<string>();

        foreach (var entry in spellData)
        {
            JObject obj = (JObject)entry.Value;
            if (obj["damage"] != null && obj["mana_cost"] != null)
                baseSpellKeys.Add(entry.Key);
            else
                modifierKeys.Add(entry.Key);
        }
    }

    public Spell Build(SpellCaster owner)
    {
        var variables = new Dictionary<string, float> {
            {"power", owner.spell_power},
            {"wave", GameManager.Instance.wave}
        };

        // Choose random base spell
        string baseKey = baseSpellKeys[Random.Range(0, baseSpellKeys.Count)];
        JObject baseSpell = (JObject)spellData[baseKey];

        // Choose 0–2 random modifiers
        List<JObject> chosenMods = new List<JObject>();
        int modCount = Random.Range(0, 3);
        List<string> pickedKeys = new List<string>();

        while (chosenMods.Count < modCount && modifierKeys.Count > 0)
        {
            string modKey = modifierKeys[Random.Range(0, modifierKeys.Count)];
            if (pickedKeys.Contains(modKey)) continue;
            pickedKeys.Add(modKey);
            chosenMods.Add((JObject)spellData[modKey]);
        }

        float damage = RPNEvaluator.Evaluate(baseSpell["damage"]["amount"].ToString(), variables);
        int mana = Mathf.CeilToInt(RPNEvaluator.Evaluate(baseSpell["mana_cost"].ToString(), variables));
        float cooldown = RPNEvaluator.Evaluate(baseSpell["cooldown"].ToString(), variables);
        float speed = RPNEvaluator.Evaluate(baseSpell["projectile"]["speed"].ToString(), variables);

        string trajectory = baseSpell["projectile"]["trajectory"].ToString();
        int icon = baseSpell["icon"].ToObject<int>();
        string name = baseSpell["name"].ToString();

        foreach (var mod in chosenMods)
        {
            name = mod["name"] + " " + name;

            if (mod["damage_multiplier"] != null)
                damage *= RPNEvaluator.Evaluate(mod["damage_multiplier"].ToString(), variables);

            if (mod["mana_multiplier"] != null)
                mana = Mathf.CeilToInt(mana * RPNEvaluator.Evaluate(mod["mana_multiplier"].ToString(), variables));

            if (mod["mana_adder"] != null)
                mana += Mathf.CeilToInt(RPNEvaluator.Evaluate(mod["mana_adder"].ToString(), variables));

            if (mod["cooldown_multiplier"] != null)
                cooldown *= RPNEvaluator.Evaluate(mod["cooldown_multiplier"].ToString(), variables);

            if (mod["speed_multiplier"] != null)
                speed *= RPNEvaluator.Evaluate(mod["speed_multiplier"].ToString(), variables);

            if (mod["projectile_trajectory"] != null)
                trajectory = mod["projectile_trajectory"].ToString();
        }

        Spell spell = new Spell(owner);
        spell.Name = name;
        spell.IconIndex = icon;
        spell.DamageAmount = Mathf.RoundToInt(damage);
        spell.ManaCost = mana;
        spell.Cooldown = cooldown;
        spell.ProjectileTrajectory = trajectory;
        spell.ProjectileSpeed = speed;

        return spell;
    }
}
