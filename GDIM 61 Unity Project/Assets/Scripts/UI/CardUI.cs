using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public const float CARD_WIDTH = 160f;
    public const float CARD_HEIGHT = 240f;

    [field:SerializeField] public Button CardButton { get; private set; }
    [field:SerializeField] public TMP_Text CostText { get; private set; }
    public int Index = -1;
    public RectTransform CardUITransform { get; private set; }
    public bool Selectable = true;
    public Image CardArt;
    public Sprite DeploySprite;

    [SerializeField] private float _floatAmount = 24f;
    [SerializeField] private float _floatTime = 0.2f;
    [Range(0f, 2f), SerializeField] private float _scaleAmount = 1.1f;

    public Vector3 OriginalPosition;
    public Vector3 OriginalScale;

    private void Awake() {
        CardButton = GetComponent<Button>();
        CardArt = GetComponent<Image>();
        CardUITransform = GetComponent<RectTransform>();
        CardButton.onClick.AddListener(SelectOnClick);
    }

    private void SelectOnClick() {
        if (UIManager.Instance.SelectedCardIndex != Index)
            UIManager.Instance.SelectCard(Index);
    }

    private IEnumerator InflateAnimation(bool beginningAnimation) {
        Vector3 endPosition;
        Vector3 endScale;

        float elapsedTime = 0f;
        if (beginningAnimation) {
            endPosition = OriginalPosition + new Vector3(0f, _floatAmount, 0f);
            endScale = OriginalScale * _scaleAmount;
        } else {
            endPosition = OriginalPosition;
            endScale = OriginalScale;
        }

        while (elapsedTime < _floatTime) {
            elapsedTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, endPosition, elapsedTime / _floatTime);
            transform.localScale = Vector3.Lerp(transform.localScale, endScale, elapsedTime / _floatTime);
            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (UIManager.Instance.SelectedCardIndex != Index)
            StartInflate(); 
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (UIManager.Instance.SelectedCardIndex != Index)
            StartDeflate();
    }

    public void StartInflate() {
        StartCoroutine(InflateAnimation(true));
    }

    public void StartDeflate() {
        StartCoroutine(InflateAnimation(false));
    }

    
}
