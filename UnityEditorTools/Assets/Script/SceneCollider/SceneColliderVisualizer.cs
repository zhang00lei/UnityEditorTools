using UnityEditor;
using UnityEngine;

public class SceneColliderVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        DrawColliders();
    }

    public void DrawColliders()
    {
        Collider[] colliders = FindObjectsOfType<Collider>();

        foreach (Collider collider in colliders)
        {
            DrawColliderBounds(collider);
        }
    }

    private void DrawColliderBounds(Collider collider)
    {
        Bounds bounds = collider.bounds;
        Handles.color = Color.green;
        Handles.DrawWireCube(bounds.center, bounds.size);
    }
}