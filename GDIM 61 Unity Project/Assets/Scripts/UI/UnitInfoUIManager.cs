using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUIManager : MonoBehaviour {
    public static UnitInfoUIManager Instance { get; private set; }

    [SerializeField] GameObject hoverMessage;
    [SerializeField] TMP_Text hoverMessageText;

    [SerializeField] GameObject unitsInfoUI; public GameObject UnitInfoUI => UnitInfoUI;
    [SerializeField] TMP_Text unitName;
    [SerializeField] Image unitArt;
    [SerializeField] TMP_Text unitDescription;
    [SerializeField] AttributesUI unitAttributes;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
        }

        unitAttributes ??= GetComponentInChildren<AttributesUI>();
    }

    public void DisplayUnitInfo(Unit unit) {
        unitsInfoUI.SetActive(true);
        Card card = unit.BaseCard;
        if (card != null) {
            unitName.text = card.Name;
            unitArt.sprite = card.Art;
            unitDescription.text = card.Description;
        } else {
            unitName.text = unit.name;
            unitArt.sprite = unit.SpriteRenderer.sprite;
            unitDescription.text = "该单位无介绍";
        }
        unitArt.preserveAspect = true;
        unitAttributes.SetAttributesUI(unit);
    }

    public void ClearUnitInfo() {
        unitsInfoUI.SetActive(false);
    }
}
