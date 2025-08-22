using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class SpriteToImageDropHandler
{
    static SpriteToImageDropHandler()
    {
        DragAndDrop.AddDropHandler(OnHierarchyDrop);
    }

    private static DragAndDropVisualMode OnHierarchyDrop(
        int dropTargetInstanceID,
        HierarchyDropFlags dropMode,
        Transform parentForDraggedObjects,
        bool perform)
    {
        List<Sprite> sprites = new List<Sprite>();
        foreach (var obj in DragAndDrop.objectReferences)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in all)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
        }

        if (sprites.Count == 0)
        {
            return DragAndDropVisualMode.None;
        }

        if (!perform)
        {
            return DragAndDropVisualMode.Link;
        }

        if (dropTargetInstanceID != 0)
        {
            var parent = EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject;

            foreach (var sprite in sprites)
            {
                GameObject newObj = new GameObject(sprite.name);
                var image = newObj.AddComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;
                
                if ((dropMode & HierarchyDropFlags.DropUpon) != 0)
                {
                    newObj.transform.SetParent(parent?.transform, false);
                }
                else if ((dropMode & HierarchyDropFlags.DropBetween) != 0)
                {
                    newObj.transform.SetParent(parent?.transform.parent, false);
                }
                
                if ((dropMode & HierarchyDropFlags.DropAfterParent) != 0)
                {
                    newObj.transform.SetSiblingIndex(0);
                }
                else if ((dropMode & HierarchyDropFlags.DropBetween) != 0)
                {
                    newObj.transform.SetSiblingIndex(parent != null ? parent.transform.GetSiblingIndex() + 1 : 0);
                }
                
                Undo.RegisterCreatedObjectUndo(newObj, "Create Image from Sprite Drag");
            }
        }

        DragAndDrop.AcceptDrag();
        return DragAndDropVisualMode.Move;
    }
}