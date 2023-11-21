using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.DemoTeam.Hair
{
    class HairAssetDragAndDrop
    {
        const string k_GenericDataKey = "HairAssetDragAndDrop";

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

                    var hairInstanceGameObject = DragAndDrop.GetGenericData(k_GenericDataKey) as GameObject;
                    if (hairInstanceGameObject == null)
                    {
                        hairInstanceGameObject = new GameObject(hairAsset.name, typeof(HairInstance));
                        hairInstanceGameObject.hideFlags = HideFlags.HideInHierarchy ;

                        var hairComp = hairInstanceGameObject.GetComponent<HairInstance>();
                        hairComp.strandGroupProviders[0].hairAsset = hairAsset;
                        hairComp.strandGroupProviders[0].hairAssetQuickEdit = true;

                        DragAndDrop.SetGenericData(k_GenericDataKey, hairInstanceGameObject);
                    }

                    hairInstanceGameObject.transform.position = worldPosition;
                    hairInstanceGameObject.transform.SetParent(parentForDraggedObjects);

                    if (perform)
                    {
                        hairInstanceGameObject.hideFlags = HideFlags.None;
                        Undo.RegisterCreatedObjectUndo(hairInstanceGameObject, "Place " + hairInstanceGameObject.name);

                        Selection.activeGameObject = hairInstanceGameObject;

                        DragAndDrop.AcceptDrag();
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
                return DragAndDropVisualMode.Generic;
            return DragAndDropVisualMode.None;
        }
    }
}