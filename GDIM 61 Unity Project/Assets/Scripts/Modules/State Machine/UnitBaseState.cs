/// <summary>
/// Abstract base class for all unit states.
/// Defines the common interface for states within the UnitStateMachine.
/// Now works with the base 'Unit' class for broader applicability.
/// </summary>
public abstract class UnitBaseState {
    protected Unit _unit;             // Reference to the unit this state belongs to (now base Unit type).
    protected UnitStateMachine _stateMachine; // Reference to the state machine managing this state.

    /// <summary>
    /// Constructor for the base state.
    /// </summary>
    /// <param name="unit">The unit instance this state will control (base Unit type).</param>
    /// <param name="stateMachine">The state machine that manages this state.</param>
    public UnitBaseState(Unit unit, UnitStateMachine stateMachine) {
        _unit = unit;
        _stateMachine = stateMachine;
    }

    /// <summary>
    /// Called when entering this state.
    /// Use this for setup logic specific to the state.
    /// </summary>
    public virtual void EnterState() { }

    /// <summary>
    /// Called every frame while this state is active.
    /// Use this for the main logic of the state.
    /// </summary>
    public abstract void UpdateState();

    /// <summary>
    /// Called when exiting this state.
    /// Use this for cleanup logic specific to the state.
    /// </summary>
    public virtual void ExitState() { }
}
