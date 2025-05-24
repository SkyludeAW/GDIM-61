using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    public static GameController Instance;

    #region Unit Selection Variables
    private Vector2 _clickStartPosition;
    private bool _clickBeganInBattleState;
    [SerializeField] private LayerMask _unitLayer;
    [SerializeField] private Transform _selectionAreaTransform;
    private List<Unit> _selectedUnits;
    #endregion

    public enum ControlState {
        InBattle,
        DeployingUnit,
        Paused
    }
    private ControlState _currentState;

    #region Deck & In-Game Variables
    public List<Card> Deck;
    private Queue<Card> _deckQueue;
    [UnityEngine.Range(1, 10)] public int MaxHandSize = 5;
    private Card[] _cardsInHand;

    private float _cost;
    public float CostGrowthSpeed = 0.4f;
    [SerializeField] private float _costUIUpdateInterval = 0.1f;
    private float _nextCostUIUpdateTime;
    #endregion

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
        }
        Initialize();
    }

    private void Initialize() {
        _cost = 0f;
        _selectedUnits = new List<Unit>();
        _cardsInHand = new Card[MaxHandSize];
        _selectionAreaTransform.gameObject.SetActive(false);
        _deckQueue = Card.ShuffleToQueue(Deck);
        DealCards();
        UIManager.Instance.InitializeCardUI(_cardsInHand);
    }

    private void OnValidate() {
        if (Application.isPlaying) {
            try {
                Initialize();
            } catch { }
        }
    }

    private void Update() {
        switch (_currentState) {
            case ControlState.InBattle:
                // Left mouse button down begins drag selection
                if (Input.GetMouseButtonDown(0)) {
                    _clickStartPosition = GetMouseWorldPosition();
                    _selectionAreaTransform.gameObject.SetActive(true);
                    _clickBeganInBattleState = true;
                }

                // Left mouse button held alters selection box
                if (Input.GetMouseButton(0)) {
                    Vector3 currentMousePosition = GetMouseWorldPosition();
                    Vector3 selectionLowerLeft = new Vector3(Mathf.Min(_clickStartPosition.x, currentMousePosition.x), Mathf.Min(_clickStartPosition.y, currentMousePosition.y));
                    Vector3 selectionUpperRight = new Vector3(Mathf.Max(_clickStartPosition.x, currentMousePosition.x), Mathf.Max(_clickStartPosition.y, currentMousePosition.y));
                    _selectionAreaTransform.position = selectionLowerLeft;
                    _selectionAreaTransform.localScale = selectionUpperRight - selectionLowerLeft;
                }

                // Left mouse button up selects all units in the selection box
                if (Input.GetMouseButtonUp(0) && _clickBeganInBattleState) {
                    _selectionAreaTransform.gameObject.SetActive(false);
                    foreach (Unit unit in _selectedUnits) {
                        if (unit != null)
                            unit.SetSelectionActive(false);
                    }
                    _selectedUnits.Clear();
                    
                    Collider2D[] selectedUnits = Physics2D.OverlapAreaAll(_clickStartPosition, GetMouseWorldPosition());

                    foreach (Collider2D collider in selectedUnits) {
                        Unit unit = collider.GetComponent<Unit>();
                        if (unit != null && unit.Selectable) {
                            _selectedUnits.Add(unit);
                            unit.SetSelectionActive(true);
                        }
                    }
                }

                // Right mouse button down sets destination or target for all selected units
                if (Input.GetMouseButtonDown(1)) {
                    Vector3 currentMousePosition = GetMouseWorldPosition();
                    Collider2D selectedCollider = Physics2D.OverlapPoint(currentMousePosition);
                    if (selectedCollider != null) {
                        Unit selectedTarget = selectedCollider.GetComponent<Unit>();
                        if (selectedTarget != null) {
                            _selectedUnits.RemoveAll(unit => unit == null);
                            foreach (Unit unit in _selectedUnits) {
                                if (unit != selectedTarget)
                                    unit.SetTarget(selectedTarget);
                            }
                        }
                    } else {
                        foreach (Unit unit in _selectedUnits) {
                            unit.SetTargetDestination(currentMousePosition);
                        }
                    }
                }
                break;

            case ControlState.DeployingUnit:
                // Left mouse button down deploys the selected unit in UIManager
                if (Input.GetMouseButtonDown(0)) {
                    _clickBeganInBattleState = false;
                    float cardCost = _cardsInHand[UIManager.Instance.SelectedCardIndex].Cost;
                    if (cardCost <= _cost) {
                        Instantiate(PlayCard(UIManager.Instance.SelectedCardIndex).SummonedUnit, GetMouseWorldPosition(), Quaternion.identity);
                        _cost -= cardCost;
                    } else {
                        UIManager.Instance.PlayInsufficientCostAnimation();
                    }
                    UIManager.Instance.DeselectCard();
                    SwitchControlState(ControlState.InBattle);
                }

                // Right mouse button down deselects the unit in UIManager
                if (Input.GetMouseButtonDown(1)) {
                    UIManager.Instance.DeselectCard();
                    SwitchControlState(ControlState.InBattle);
                }
                break;
        }

        _cost += Time.deltaTime * CostGrowthSpeed;
        if (Time.time > _nextCostUIUpdateTime) {
            _nextCostUIUpdateTime = Time.time + _costUIUpdateInterval;
            UIManager.Instance.UpdateCostUIValue(_cost);
        }
    }

    // Puts the cards from queue in hand
    private void DealCards() {
        int N = _deckQueue.Count;
        int M = _cardsInHand.Length;
        for (int i = 0; i < N && i < M; i++) {
            _cardsInHand[i] = _deckQueue.Dequeue();
            // Debug.Log(_cardsInHand[i].Name + " dealt to position " + i);
        }
    }

    // Plays the card from hand and sorts the cards in hand to the left
    // Should only be called when _currentState is ControlState.DeployingUnit
    private Card PlayCard(int selectedCardIndex) {
        // Removes card from hand and enqueues the card
        Card playedCard = _cardsInHand[selectedCardIndex];
        _cardsInHand[selectedCardIndex] = null;
        _deckQueue.Enqueue(playedCard);
        // Debug.Log(playedCard.Name + " queued to position " + (_deckQueue.Count - 1));

        // Move the rest of the card forward to fill up the null place
        for (int i = selectedCardIndex; i < MaxHandSize - 1; i++)
            _cardsInHand[i] = _cardsInHand[i + 1];
        _cardsInHand[Mathf.Min(Deck.Count, MaxHandSize) - 1] = _deckQueue.Dequeue();
        // Debug.Log(_cardsInHand[Mathf.Min(Deck.Count, MaxHandSize) - 1].Name + " dealt to position " + (Mathf.Min(_deckQueue.Count, MaxHandSize) - 1));

        // Update UI
        UIManager.Instance.InitializeCardUI(_cardsInHand);

        return playedCard;
    }

    public void SwitchControlState(ControlState state) {
        if (_currentState != state) {
            _currentState = state;
            foreach (Unit unit in _selectedUnits) {
                if (unit != null)
                    unit.SetSelectionActive(false);
            }
            _selectedUnits.Clear();
            _selectionAreaTransform.gameObject.SetActive(false);
        }
    }

    #region Utilities
    public static Vector2 GetMouseWorldPosition() {
        return CameraLocator.Instance.PlayerCamera.ScreenToWorldPoint(Input.mousePosition);
    }
    #endregion
}
