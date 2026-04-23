// 2. Simplified UnitStateMachine (No event subscriptions!)
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public enum UnitState {
    Idle,           // Default, no highlight
    Selected,       // Gold glow, movement UI
    Friendly,       // Blue outline (own faction, not selected)
    Hostile,        // Red tint/shader (enemy)
    Moving,         // Path trail, speed lines
    Attacking,      // Swing animation + particles
    Dead            // Fade out, ragdoll
}
public class UnitStateMachine : MonoBehaviour {
    public SO_UnitVisualConfig config;

    private UnitState currentState = UnitState.Idle;
    private Renderer rend;
    private Outline outline;

    void Awake() {
        rend = GetComponent<Renderer>();
        outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();

        // Register with manager on spawn
        SelectionManager.Instance?.RegisterUnit(this);
    }

    // Called directly by SelectionManager — zero events
    public void SetState(UnitState newState) {
        if (currentState == newState) return;

        currentState = newState;

        var visuals = config.GetForState(newState);
        if (visuals == null) return;

        if (visuals.material) {
            rend.material = visuals.material;
        }

        //outline.OutlineColor = visuals.outlineColor;
        //outline.OutlineWidth = visuals.outlineWidth;
        outline.enabled = visuals.outlineWidth > 0f;
    }

    // Optional: For animations/particles if needed
}