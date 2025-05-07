using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder
{
    public Spell Build(SpellCaster owner, string spellName)
    {
        if (!GameManager.Instance.spells.ContainsKey(spellName))
        {
            Debug.LogError($"Spell '{spellName}' not found in GameManager spells.");
            return null;
        }

        JObject spellAttributes = (JObject)GameManager.Instance.spells[spellName];
        return Build(owner, spellAttributes);
    }

    public Spell Build(SpellCaster owner, JToken attributes)
    {
        string spellName = attributes["name"]?.ToString();

        if (spellName == "Arcane Bolt")
        {
            return new ArcaneBolt(owner, attributes);
        }
        else if (spellName == "Magic Missile")
        {
            return new MagicMissile(owner, attributes);
        }
        else if (spellName == "Arcane Blast")
        {
            return new ArcaneBlast(owner, attributes);
        }
        else if (spellName == "Arcane Spray")
        {
            return new ArcaneSpray(owner, attributes);
        }

        // Spell spell = new Spell(owner);
        // spell.SetProperties((JObject)attributes);

        // return spell;

        Debug.LogError($"Unknown spell name: {spellName}");
        return null;
    }

    public SpellBuilder()
    {
    }

}
