using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.DemoTeam.Hair
{
    class HairAssetDragAndDrop
    {
        static Dictionary<int, GameObject> s_DraggedObject = new Dictionary<int, GameObject>();

        [InitializeOnLoadMethod]
        static void HandleDropHairAssetInScene()
        {
            DragAndDrop.AddDropHandler(HandleDragOntoHierarchy);

            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sv)
        {
            var evt = Event.current;
            if (evt == null)
                return;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                    HandleDragUpdate();
                    break;
                case EventType.DragPerform:
                    HandleDragPerformed();
                    break;
                case EventType.DragExited:
                    HandleDragExited();
                    break;
            }
        }

        static void HandleDragUpdate()
        {
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is HairAsset)
                {
                    HairAsset hairAsset = obj as HairAsset;
                    GameObject hairInstanceGameObject = null;

                    if (!s_DraggedObject.TryGetValue(hairAsset.GetInstanceID(), out hairInstanceGameObject))
                    {
                        hairInstanceGameObject = InstantiateHairPreviewInSceneView(hairAsset);
                        hairInstanceGameObject.hideFlags = HideFlags.HideInHierarchy;
                        s_DraggedObject.Add(hairAsset.GetInstanceID(), hairInstanceGameObject);
                        DragAndDrop.AcceptDrag();
                    }

                    HandleUtility.PlaceObject(Event.current.mousePosition, out Vector3 p, out Vector3 n);
                    hairInstanceGameObject.transform.position = p;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
            }

            if (s_DraggedObject.Count > 0)
                Event.current.Use();
        }

        static void HandleDragPerformed()
        {

            if (s_DraggedObject == null)
                return;

            foreach (var preview in s_DraggedObject)
            {
                preview.Value.hideFlags = HideFlags.None;
                SaveInUndoStack(preview.Value);
            }

            GameObject[] gos = new GameObject[s_DraggedObject.Count];
            s_DraggedObject.Values.CopyTo(gos, 0);
            Selection.objects = gos;

            s_DraggedObject.Clear();
            Event.current.Use();
        }

        static void HandleDragExited()
        {
            if (s_DraggedObject == null)
                return;

            foreach (var preview in s_DraggedObject)
                GameObject.DestroyImmediate(preview.Value);

            s_DraggedObject.Clear();
            Event.current.Use();
        }

        static DragAndDropVisualMode HandleDragOntoHierarchy(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            List<GameObject> draggedObjects = new List<GameObject>();
            int count = 0;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is HairAsset)
                {
                    var hairAsset = obj as HairAsset;
                    if (perform)
                    {
                        var go = InstantiateHairPreviewInSceneView(hairAsset);
                        SaveInUndoStack(go);
                        if (dropMode == HierarchyDropFlags.DropUpon)
                        {
                            go.transform.SetParent((EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject)?.transform, true);
                        }
                        else if (dropMode.HasFlag(HierarchyDropFlags.DropAbove))
                        {
                            var sibling = EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject;
                            go.transform.SetParent(sibling.transform.parent, true);
                            go.transform.SetAsFirstSibling();
                        }
                        else if (dropMode == HierarchyDropFlags.DropBetween)
                        {
                            var sibling = EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject;
                            go.transform.SetParent(sibling.transform.parent, true);
                            go.transform.SetSiblingIndex(sibling.transform.GetSiblingIndex() + 1);
                        }
                        DragAndDrop.AcceptDrag();
                        draggedObjects.Add(go);
                    }
                    ++count;
                }
            }

            if (count > 0)
            {
                if (perform)
                    Selection.objects = draggedObjects.ToArray();
                return DragAndDropVisualMode.Copy;
            }
            return DragAndDropVisualMode.None;
        }

        static GameObject InstantiateHairPreviewInSceneView(HairAsset hairAsset)
        {
            var hairInstanceGameObject = new GameObject(hairAsset.name, typeof(HairInstance));
            var hairComp = hairInstanceGameObject.GetComponent<HairInstance>();
            hairComp.strandGroupProviders[0].hairAsset = hairAsset;
            hairComp.strandGroupProviders[0].hairAssetQuickEdit = true;

            return hairInstanceGameObject;
        }

        static void SaveInUndoStack(Object obj)
        {
            Undo.RegisterCreatedObjectUndo(obj, "Place " + obj.name);
        }
    }
}