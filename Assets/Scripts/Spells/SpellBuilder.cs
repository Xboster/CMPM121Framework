using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder
{
    public Spell Build(SpellCaster owner, JToken attributes)
    {
        Spell spell = new Spell(owner);
        spell.SetProperties((JObject)attributes);

        return spell;
    }


    public SpellBuilder()
    {
    }

}
