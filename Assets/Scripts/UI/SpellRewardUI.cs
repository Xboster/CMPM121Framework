using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpellRewardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI spellNameText;
    private Spell storedSpell;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI descriptionText;


    public void Setup(Spell spell)
    {
        storedSpell = spell;
        spellNameText.text = spell.GetName();
        spellNameText.text = spell.GetName();
        damageText.text = $"Damage: {spell.GetDamage()}";
        manaCostText.text = $"Mana Cost: {spell.GetManaCost()}";
        cooldownText.text = $"Cooldown: {spell.GetCooldown():0.##}s";
        descriptionText.text = spell is not null ? spell.owner : "";

    }

    public void OnClickChoose()
    {
        var caster = GameManager.Instance.player.GetComponent<SpellCaster>();
        caster.spell = storedSpell;

        // Update HUD display
        var spellUI = FindObjectOfType<SpellUI>();
        if (spellUI != null) spellUI.SetSpell(storedSpell);


        Debug.Log($"Player selected spell: {storedSpell.GetName()}");

        // Optionally close the UI
        transform.parent.parent.gameObject.SetActive(false);
    }
}
