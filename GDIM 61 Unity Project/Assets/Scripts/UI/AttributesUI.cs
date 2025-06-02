using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttributesUI : MonoBehaviour { 
    Dictionary<string, TMP_Text> attributes;
    public Dictionary<string, TMP_Text> Attributes => attributes;
    Unit unit;

    public void SetAttributesUI(Unit unit) {
        if (attributes == null || attributes == default) {
            attributes = new Dictionary<string, TMP_Text>();

            foreach (var attribute in GetComponentsInChildren<Attribute>()) {
                attributes.Add(attribute.Name, attribute.Value);
            }
        }

        this.unit = unit;
    }

    public void SetAttributeUI(string name, string value) {
        attributes[name].text = value;
    }

    private void Update() {
        SetAttributeUI("Hitpoint", $"{unit.Hitpoint:0} / {unit.MaxHitPoint:0}");
        SetAttributeUI("Damage", $"{unit.Damage:0.0}");
        SetAttributeUI("Knockback", $"{unit.Knockback:0.0}");
        SetAttributeUI("Cooldown", $"{unit.Cooldown:0.00} sec / hit");
        SetAttributeUI("Range", $"{unit.Range:0.00} m");
        SetAttributeUI("Speed", $"{unit.Speed:0.00} m / sec");
    }
}
