using UnityEditor;
using UnityEngine;

namespace Unity.DemoTeam.Hair
{
    class HairAssetDragAndDrop
    {
        [InitializeOnLoadMethod]
        static void HandleDropHairAssetInScene()
        {
            DragAndDrop.AddDropHandler(OnDragOntoScene);
            DragAndDrop.AddDropHandler(OnDragOntoHierarchy);
        }

        static DragAndDropVisualMode OnDragOntoScene(UnityEngine.Object dropUpon, Vector3 worldPosition, Vector2 viewportPosition, Transform parentForDraggedObjects, bool perform)
        {
            int count = 0;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is HairAsset)
                {
                    var hairAsset = obj as HairAsset;
                    if (perform)
                    {
                        //DragAndDrop.AcceptDrag();
                        var go = new GameObject(hairAsset.name, typeof(HairInstance));
                        var hairInstance = go.GetComponent<HairInstance>();
                        hairInstance.strandGroupProviders[0].hairAsset = hairAsset;
                        hairInstance.strandGroupProviders[0].hairAssetQuickEdit = true;
                        go.transform.position = worldPosition;
                        go.transform.SetParent(parentForDraggedObjects);
                        Selection.activeGameObject = go;
                    }
                    ++count;
                }
            }

            if (count > 0)
                return DragAndDropVisualMode.Generic;
            return DragAndDropVisualMode.None;
        }

        static DragAndDropVisualMode OnDragOntoHierarchy(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            int count = 0;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is HairAsset)
                {
                    var hairAsset = obj as HairAsset;
                    if (perform)
                    {
                        //DragAndDrop.AcceptDrag();
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
                    }
                    ++count;
                }
            }

            if (count > 0)
                return DragAndDropVisualMode.Generic;
            return DragAndDropVisualMode.None;
        }
    }
}