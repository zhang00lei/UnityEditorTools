using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneColliderVisualizer))]
public class SceneColliderVisualizerEditor : Editor
{
    private void OnSceneGUI()
    {
        SceneColliderVisualizer visualizer = (SceneColliderVisualizer)target;
        visualizer.DrawColliders();
    }
}
