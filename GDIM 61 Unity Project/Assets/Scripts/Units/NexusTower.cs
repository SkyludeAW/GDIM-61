using UnityEngine;

public class NexusTower : Unit {

    private void Awake() {
        Initialize();
    }

    protected override void Initialize() {
        base.Initialize();

        foreach (var unit in GetComponentsInChildren<Unit>())
            unit.ConfigureFaction(Faction);
    }

    public override void Die() {
        IsDead = true;
        if (HealthUI != null)
            Destroy(HealthUI.gameObject);

        if (Faction == 0)
            GameStateManager.Instance.EndGame(false);
        else
            GameStateManager.Instance.EndGame(true);
    }

    public override void PerformAttack(Unit targetUnit) {
        // throw new System.NotImplementedException();
    }

    public override void MoveTo(Vector2 destination) {
        // throw new System.NotImplementedException();
    }
}