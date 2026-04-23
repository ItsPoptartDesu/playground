// Runtime manager - attach to empty GameObject per faction (or pool them)
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour {
    [Header("Data")]
    public FactionData data;

    [Header("Runtime State")]
    public int stars = 10; // Currency
    public int population = 1; // From cities
    public HashSet<string> researchedTechs = new(); // Fast lookup
    public List<Unit> ownedUnits = new();
    //public List<City> ownedCities = new(); // Cities expand population/stars

    [Header("Turn State")]
    public bool isPlayerControlled = false;
    public bool hasTakenTurn = false;

    void Awake() {
        if (data != null) {
            stars = data.startingStars;
            foreach (string tech in data.startingTechs)
                ResearchTech(tech);
        }
    }

    // Tech Tree Integration
    public bool CanResearch(string techName) {
        //TechNode node = TechManager.Instance.GetTechNode(techName);
        //return node != null && node.prerequisites.All(prereq => researchedTechs.Contains(prereq.name));
        return false;
    }

    public void ResearchTech(string techName) {
        //if (CanResearch(techName) && stars >= TechManager.Instance.GetTechCost(techName)) {
        //    stars -= TechManager.Instance.GetTechCost(techName);
        //    researchedTechs.Add(techName);
        //    // Unlock units/buildings via events
        //    TechManager.Instance.OnTechResearched?.Invoke(this , techName);
        //}
    }

    // Unit Management
    public void AddUnit(Unit unit) {
        ownedUnits.Add(unit);
        unit.myOwner = this;
    }

    public void RemoveUnit(Unit unit) {
        ownedUnits.Remove(unit);
    }

    // Resource Generation (End of turn)
    public void GenerateResources() {
        //int starIncome = ownedCities.Sum(city => city.GetStarProduction(this));
        //stars += starIncome;
        //population = ownedCities.Sum(city => city.population);
    }

    // Production (Buy units/cities)
    public bool TryProduceUnit(UnitData unitData , HexTile targetTile) {
        //if (stars >= unitData.cost && CanProduceUnit(unitData)) {
        //    stars -= unitData.cost;
        //    Unit newUnit = Instantiate(unitData.prefab , targetTile.transform).GetComponent<Unit>();
        //    newUnit.Initialize(unitData , this);
        //    targetTile.SetOccupant(newUnit);
        //    AddUnit(newUnit);
        //    return true;
        //}
        return false;
    }

    public bool CanProduceUnit(UnitData unitData) {
        return true;
        //return researchedTechs.Contains(unitData.requiredTech) && !targetTile.HasOccupant();
    }

    // Visual Helpers
    public Color GetFactionColor() => data?.factionColor ?? Color.white;

    // Equality for quick checks (mirror matchups)
    public override bool Equals(object obj) => obj is Faction f && f.data == data;
    public override int GetHashCode() => data?.name?.GetHashCode() ?? 0;
}