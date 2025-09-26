using UnityEngine;
using UnityEngine.Events;

public interface ISelectable
{
    bool IsSelectable { get; } // e.g., true if in range or owned by player
    void OnSelect(UnityEvent onSelectedEvent = null); // Trigger visuals/UI (e.g., highlight)
    void OnDeselect();
    Vector3 WorldPosition { get; } // For UI world-space pointers
    // Optional: GetComponent<Unit>() for type-specific actions
}