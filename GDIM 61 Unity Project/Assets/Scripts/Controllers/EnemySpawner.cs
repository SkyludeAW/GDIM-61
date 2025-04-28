using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private float spawnInterval;
    [SerializeField] private float nextSpawnTime = 5f;
    [SerializeField] private float speedUpInterval;
    private float nextSpeedUpTime = 10f;
    [SerializeField] private float _strengthenFactor = 1.01f;

    [SerializeField] private Unit spawnedUnit;
    [SerializeField] private Card initialStats;
    private float _HP;
    private float _attack;

    private void Awake() {
        _HP = initialStats.HitPoint;
        _attack = initialStats.Damage;
    }

    private void Update() {
        if (Time.time > nextSpawnTime) {
            nextSpawnTime = Time.time + spawnInterval;
            Unit unit = Instantiate(spawnedUnit, transform.position, Quaternion.identity);
            unit.Initialize(_HP, _attack);
            unit.SetFaction(1);
            _HP *= _strengthenFactor;
            _attack *= _strengthenFactor;
        }
        if (Time.time > nextSpeedUpTime) {
            spawnInterval *= 0.995f;
            nextSpeedUpTime = Time.time + speedUpInterval;
        }
    }
}
