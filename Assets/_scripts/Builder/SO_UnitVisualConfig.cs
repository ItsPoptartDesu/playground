// 2. Visual Config (ScriptableObject for easy tuning)
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Visual Config")]
public class SO_UnitVisualConfig : ScriptableObject {
    [System.Serializable]
    public class StateVisuals {
        public UnitState state;
        public Material material;           // Override shader
        public ParticleSystem particles;    // e.g., glow, sparks
        public AnimationClip animation;     // Idle anim variant
        public Color outlineColor = Color.white;
        [Range(0f , 2f)] public float outlineWidth = 0.1f;
    }

    public StateVisuals[] visuals;  // Array for each state

    public StateVisuals GetForState(UnitState state) {
        return System.Array.Find(visuals , v => v.state == state);
    }
}