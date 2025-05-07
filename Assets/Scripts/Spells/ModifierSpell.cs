// using System.Collections;
// using UnityEngine;

// public class ModifierSpell : Spell
// {
//     private Spell baseSpell;

//     public ModifierSpell(SpellCaster owner) : base(owner) { }

//     public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
//     {
//         // Custom implementation for ModifierSpell
//         Debug.Log("ModifierSpell Cast called!");
//         yield return base.Cast(where, target, team); // Optionally call the base implementation
//     }
// }