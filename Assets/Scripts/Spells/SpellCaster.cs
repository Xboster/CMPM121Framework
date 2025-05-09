using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster
{
    public float mana;
    public int max_mana;
    public float mana_reg;
    public int spell_power;
    public Hittable.Team team;
    public Spell spell;

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, Hittable.Team team)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.team = team;
        spell = new SpellBuilder().Build(this);
    }

    public IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        if (mana >= spell.GetManaCost() && spell.IsReady())
        {
            mana -= spell.GetManaCost();
            yield return spell.Cast(origin, target, team);
        }
        yield break;
    }
}
