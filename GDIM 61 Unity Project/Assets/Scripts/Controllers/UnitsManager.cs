using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class UnitsManager : MonoBehaviour {

    // --- Singleton Pattern ---
    public static UnitsManager Instance { get; private set; }
    // --- End Singleton ---

    // Use a List of HashSets. Each index in the list corresponds to a faction.
    // Each HashSet stores the units belonging to that faction.
    private List<HashSet<Unit>> _factionUnits;

    // Define the number of factions (adjust if needed)
    private const int NumberOfFactions = 4;

    private void Awake() {
        // --- Singleton Initialization ---
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
        }
        Instance = this;
        // Optional: Keep the manager persistent across scene loads
        // DontDestroyOnLoad(this.gameObject); 
        // --- End Singleton Initialization ---

        // Initialize the list and the HashSets for each faction
        _factionUnits = new List<HashSet<Unit>>(NumberOfFactions);
        for (int i = 0; i < NumberOfFactions; i++) {
            _factionUnits.Add(new HashSet<Unit>());
        }

        // Optional: Start periodic cleanup of null units (if needed as a failsafe)
        // InvokeRepeating(nameof(PurgeNullUnits), 5f, 5f); // Purge every 5 seconds, starting after 5 seconds
    }

    /// <summary>
    /// Registers a unit with the manager, adding it to the appropriate faction set.
    /// </summary>
    /// <param name="unit">The unit to register.</param>
    public void RegisterUnit(Unit unit) {
        if (unit == null) {
            Debug.LogWarning("Attempted to register a null unit.");
            return;
        }

        int faction = unit.Faction;
        // Check if the faction index is valid
        if (faction >= 0 && faction < _factionUnits.Count) {
            // HashSet automatically handles duplicates (won't add if already present)
            bool added = _factionUnits[faction].Add(unit);
            // if (added) Debug.Log($"{unit.name} registered to faction {faction}");
        } else {
            Debug.LogWarning($"Attempted to register unit {unit.name} with invalid faction index: {faction}");
        }
    }

    /// <summary>
    /// Unregisters a unit from the manager, removing it from its faction set.
    /// </summary>
    /// <param name="unit">The unit to unregister.</param>
    public void UnregisterUnit(Unit unit) {
        if (unit == null) {
            // This might happen naturally during scene cleanup, often safe to ignore
            // Debug.LogWarning("Attempted to unregister a null unit.");
            return;
        }

        int faction = unit.Faction;
        // Check if the faction index is valid
        if (faction >= 0 && faction < _factionUnits.Count) {
            bool removed = _factionUnits[faction].Remove(unit);
            // if (removed) Debug.Log($"{unit.name} unregistered from faction {faction}");
        } else {
            // Less critical on unregister, but indicates a potential issue elsewhere
            Debug.LogWarning($"Attempted to unregister unit {unit.name} with invalid faction index: {faction}");
        }
    }

    /// <summary>
    /// Gets an enumerable collection of all units currently registered for a specific faction.
    /// Returns an empty collection if the faction index is invalid.
    /// </summary>
    /// <param name="faction">The faction index.</param>
    /// <returns>An IEnumerable<Unit> for the requested faction.</returns>
    public IEnumerable<Unit> GetUnitsInFaction(int faction) {
        if (faction >= 0 && faction < _factionUnits.Count) {
            // Return the HashSet directly. Callers should not modify it extensively.
            // If modification protection is needed, could return a copy: `new HashSet<Unit>(_factionUnits[faction])`
            // but that allocates memory. Returning IEnumerable is generally safe for iteration.
            return _factionUnits[faction];
        } else {
            Debug.LogWarning($"Requested units for invalid faction index: {faction}");
            // Return an empty enumerable to prevent null reference errors for the caller
            return Enumerable.Empty<Unit>();
        }
    }

    /// <summary>
    /// Removes any null entries from all faction HashSets. 
    /// This acts as a cleanup if units were destroyed without proper unregistration.
    /// </summary>
    public void PurgeNullUnits() {
        // Debug.Log("Purging null units...");
        int removedCount = 0;
        foreach (HashSet<Unit> unitSet in _factionUnits) {
            // HashSet<T>.RemoveWhere is efficient for removing multiple items based on a condition
            removedCount += unitSet.RemoveWhere(unit => unit == null);
        }
        // if (removedCount > 0) Debug.Log($"Purged {removedCount} null unit entries.");
    }

    // Optional: Provide a way to get the count per faction for debugging
    public int GetUnitCount(int faction) {
        if (faction >= 0 && faction < _factionUnits.Count) {
            return _factionUnits[faction].Count;
        }
        return 0;
    }
}