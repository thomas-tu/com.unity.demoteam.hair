using UnityEditor;
using UnityEngine;

namespace Unity.DemoTeam.Hair
{
    class HairAssetDragAndDrop
    {
        static GameObject s_DraggedObject = null;

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
            int count = 0;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is HairAsset)
                {
                    var hairAsset = obj as HairAsset;

                    var hairInstanceGameObject = s_DraggedObject;
                    if (hairInstanceGameObject == null)
                    {
                        s_DraggedObject = InstantiateHairPreviewInSceneView(hairAsset);
                        DragAndDrop.AcceptDrag();
                    }

                    HandleUtility.PlaceObject(Event.current.mousePosition, out Vector3 p, out Vector3 n);
                    hairInstanceGameObject.transform.position = p;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    ++count;
                }
            }

            if (count > 0)
                Event.current.Use();
        }

        static void HandleDragPerformed()
        {

            if (s_DraggedObject == null)
                return;

            var instance = PerformHairInstantationInSceneView(s_DraggedObject);
            s_DraggedObject = null;

            Selection.activeGameObject = instance;
            Event.current.Use();
        }

        static void HandleDragExited()
        {
            if (s_DraggedObject == null)
                return;

            GameObject.DestroyImmediate(s_DraggedObject);
            s_DraggedObject = null;
            Event.current.Use();
        }

        static DragAndDropVisualMode HandleDragOntoHierarchy(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            int count = 0;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is HairAsset)
                {
                    var hairAsset = obj as HairAsset;
                    if (perform)
                    {
                        var go = new GameObject(hairAsset.name, typeof(HairInstance));
                        var hairInstance = go.GetComponent<HairInstance>();
                        hairInstance.strandGroupProviders[0].hairAsset = hairAsset;
                        hairInstance.strandGroupProviders[0].hairAssetQuickEdit = true;

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

                        Selection.activeGameObject = go;
                        DragAndDrop.AcceptDrag();
                    }
                    ++count;
                }
            }

            if (count > 0)
                return DragAndDropVisualMode.Copy;
            return DragAndDropVisualMode.None;
        }

        static GameObject InstantiateHairPreviewInSceneView(HairAsset hairAsset)
        {
            var hairInstanceGameObject = new GameObject(hairAsset.name, typeof(HairInstance));
            hairInstanceGameObject.hideFlags = HideFlags.HideInHierarchy;

            var hairComp = hairInstanceGameObject.GetComponent<HairInstance>();
            hairComp.strandGroupProviders[0].hairAsset = hairAsset;
            hairComp.strandGroupProviders[0].hairAssetQuickEdit = true;

            return hairInstanceGameObject;
        }

        static GameObject PerformHairInstantationInSceneView(GameObject preview)
        {
            preview.hideFlags = HideFlags.None;
            Undo.RegisterCreatedObjectUndo(preview, "Place " + preview.name);

            return preview;
        }
    }
}