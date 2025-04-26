using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    [field:SerializeField] public CardUI[] CardUIArray { get; private set; } = new CardUI[10];
    [field:SerializeField] public RectTransform CardSlotTransform { get; private set; }
    public float CardUIVerticalOffset = -20;
    public int SelectedCardIndex;

    [field:SerializeField] public TMP_Text CostValue { get; private set; }

    [SerializeField] private DeployIndicator _deployIndicator;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
        }
        SelectedCardIndex = -1;
    }

    public void SelectCard(int index) {
        DeselectCard();
        GameController.Instance.SwitchControlState(GameController.ControlState.DeployingUnit);
        SelectedCardIndex = index;
        CardUIArray[SelectedCardIndex].StartInflate();
        _deployIndicator.gameObject.SetActive(true);
        _deployIndicator.SR.sprite = CardUIArray[SelectedCardIndex].DeploySprite;
    }

    public void DeselectCard() {
        for (int i = 0; i < GameController.Instance.MaxHandSize; i++)
            CardUIArray[i].StartDeflate();
        SelectedCardIndex = -1;
        _deployIndicator.gameObject.SetActive(false);
    }

    public void InitializeCardUI(Card[] cards) {
        int n = cards.Length;
        SelectedCardIndex = -1;
        float sectionWidth = CardSlotTransform.rect.width / n;
        for (int i = 0; i < CardUIArray.Length; i++) {
            CardUI cardUI = CardUIArray[i];
            if (i < n) {
                cardUI.gameObject.SetActive(true);
                cardUI.CardUITransform.localPosition = new Vector3((i - (n / 2f)) * sectionWidth + (sectionWidth / 2), CardUIVerticalOffset, 0);
                cardUI.Index = i; 
                SetUpCardUI(cardUI, cards[i]);
            } else {
                cardUI.gameObject.SetActive(false);
            }
        }
    }

    private void SetUpCardUI(CardUI ui, Card card) {
        ui.OriginalPosition = ui.transform.localPosition;
        ui.OriginalScale = Vector3.one;
        if (card == null) {
            ui.Selectable = false;
            ui.CardArt.enabled = false;
            ui.CostText.enabled = false;
        } else {
            ui.Selectable = true;
            ui.CardArt.enabled = true;
            ui.CostText.enabled = true;
            ui.CostText.text = card.Cost.ToString();
            ui.CardArt.sprite = card.Art;
            ui.DeploySprite = card.UnitDeploySprite;
        }
    }

    public void UpdateCostUIValue(float value) {
        CostValue.text = value.ToString("F1", CultureInfo.InvariantCulture);
    }

    public void PlayInsufficientCostAnimation() {
        StartCoroutine(InsufficientCostAnimation());
    }

    private IEnumerator InsufficientCostAnimation() {
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration) {
            CostValue.color = new Color(1f - Mathf.Min(elapsed / duration, 1f), 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        CostValue.color = Color.black;
    }
}
