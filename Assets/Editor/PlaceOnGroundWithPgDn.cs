using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlaceOnGroundWithPgDn
{
    static PlaceOnGroundWithPgDn()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.PageDown)
        {
            PlaceOnGround();
            e.Use();
        }
    }

    private static void PlaceOnGround()
    {
        if (Selection.gameObjects.Length == 0)
            return;

        var transforms = new Transform[Selection.gameObjects.Length];
        for (int i = 0; i < transforms.Length; i++)
            transforms[i] = Selection.gameObjects[i].transform;

        Undo.RecordObjects(transforms, "Place objects on ground");

        foreach (var obj in Selection.gameObjects)
        {
            Vector3 pointOnGround = obj.transform.position;

            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                pointOnGround = GetPointOnGround(collider);
            else
            {
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                pointOnGround = meshRenderer != null ?
                    GetPointOnGround(meshRenderer) :
                    GetPointOnGround(obj.transform.position);
            }

            obj.transform.position = pointOnGround;
        }
    }

    private static Vector3 GetPointOnGround(Collider collider)
    {
        Vector3 target = collider.transform.position;
        var bounds = collider.bounds;
        var localBoundsCenter = bounds.center - collider.transform.position;
        int originalLayer = collider.gameObject.layer;
        collider.gameObject.layer = 2;

        if (Physics.Raycast(bounds.center, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, Physics.DefaultRaycastLayers))
        {
            Vector3 point = hitInfo.point;
            target = point + Vector3.up * (bounds.size.y / 2f - localBoundsCenter.y);
            target.x = collider.transform.position.x;
            target.z = collider.transform.position.z;
        }

        collider.gameObject.layer = originalLayer;
        return target;
    }

    private static Vector3 GetPointOnGround(MeshRenderer mesh)
    {
        Vector3 target = mesh.transform.position;
        var bounds = mesh.bounds;
        var localBoundsCenter = bounds.center - mesh.transform.position;
        int originalLayer = mesh.gameObject.layer;
        mesh.gameObject.layer = 2;

        if (Physics.Raycast(bounds.center, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, Physics.DefaultRaycastLayers))
        {
            Vector3 point = hitInfo.point;
            target = point + Vector3.up * (bounds.size.y / 2f - localBoundsCenter.y);
            target.x = mesh.transform.position.x;
            target.z = mesh.transform.position.z;
        }

        mesh.gameObject.layer = originalLayer;
        return target;
    }

    private static Vector3 GetPointOnGround(Vector3 origin)
    {
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, Physics.DefaultRaycastLayers))
            return hitInfo.point;
        return origin;
    }
}
