using System.Linq;
using UnityEngine;

public class NexusTower : Unit {

    private void Awake() {
        Initialize();
    }

    protected override void Initialize() {
        base.Initialize();
        if (UnitsManager.Instance != null && !UnitsManager.Instance.GetUnitsInFaction(Faction).Contains(this)) {
            UnitsManager.Instance.RegisterUnit(this);
        }
    }

    public override void Die() {
        Destroy(this.gameObject);
        SceneController.Instance.GoToDefeatScene();
    }

    public override void PerformAttack(Unit targetUnit) {
        // throw new System.NotImplementedException();
    }
}