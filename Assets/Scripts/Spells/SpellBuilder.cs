using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder
{

    public Spell Build(JObject attributes, SpellCaster owner)
    {
        Spell spell = new Spell(owner);

        // TODO



        return spell;

    }


    public SpellBuilder(JObject attributes)
    {
    }

}
