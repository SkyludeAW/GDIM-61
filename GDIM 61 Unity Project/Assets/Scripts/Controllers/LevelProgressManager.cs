using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour {
    [SerializeField] PopupMessage popupMessagePrefab;

    public static LevelProgressManager Instance { get; private set; }

    HashSet<Checkpoint> checkpoints;
    float totalLevelProgress;
    public float TotalLevelProgress => totalLevelProgress;
    float levelProgress;
    public float LevelProgress => levelProgress;

    [SerializeField] HealthBarUI levelProgressBar;
    [SerializeField] TMP_Text levelProgressPercentage;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        checkpoints = new HashSet<Checkpoint>();
    }

    private void Start() {
        if (levelProgressBar != null) 
            levelProgressBar.SetHealth(0f);
        if (levelProgressPercentage != null)
            levelProgressPercentage.text = "0.00%";
    }

    public void RegisterCheckpoint(Checkpoint checkpoint) {
        if (checkpoints.Add(checkpoint))
            totalLevelProgress += checkpoint.Weight;
    }

    public void UnregisterCheckpoint(Checkpoint checkpoint) {
        if (checkpoints.Remove(checkpoint))
            totalLevelProgress -= checkpoint.Weight;
    }

    public void ConquerCheckpoint(Checkpoint checkpoint, GameObject controlledArea = null) {
        if (!checkpoints.Remove(checkpoint))
            return;

        levelProgress += checkpoint.Weight;
        
        float normalizedLevelProgress = levelProgress / totalLevelProgress;
        if (levelProgressBar != null)
            levelProgressBar.SetHealth(normalizedLevelProgress);

        if (levelProgressPercentage != null)
            levelProgressPercentage.text = normalizedLevelProgress.ToString("P2");

        if (checkpoint.CostIncrementBoost > 0f) {
            GameController.Instance.CostGrowthSpeed += checkpoint.CostIncrementBoost;
            if (popupMessagePrefab != null)
                Instantiate(popupMessagePrefab, checkpoint.transform.position, Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f))).SetUpAndActivate("Cost increment rate increased!", new Vector2(0f, Random.Range(0.01f, 0.1f)), Random.Range(5f, 10f));
        }

        if (controlledArea != null && DeployableAreaLocator.Instance != null) {
            controlledArea.transform.SetParent(DeployableAreaLocator.Instance.transform, true);
            DeployableAreaLocator.Instance.DeployableArea.GenerateGeometry();
            DeployableAreaLocator.Instance.OutlineDrawer.UpdateOutline();

            if (popupMessagePrefab != null)
                Instantiate(popupMessagePrefab, checkpoint.transform.position, Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f))).SetUpAndActivate("Deployable area expanded!", new Vector2(0f, Random.Range(0.01f, 0.1f)), Random.Range(5f, 10f));
        }
    }
}
