using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    //Gets the parent of a model if it is nested in another model
    public static Transform getNamedParent(Transform obj, string parentTag)
    {
        if (obj.parent.CompareTag(parentTag))
        {
            return obj;
        }
        else
        {
            return getNamedParent(obj.parent, parentTag);
        }
    }

    //Takes a dictionary of transform keys and sets their parents to their associated values
    public static void resetTransformParents(Dictionary<Transform, Transform> transformToReset)
    {
        foreach (KeyValuePair<Transform, Transform> item in transformToReset) //Reset the interactables' materials
        {
            item.Key.parent = item.Value; //Set the parent of the key to the value
        }

        transformToReset.Clear(); //Clear the dictionary
    }

    //Reset materials of recorded transforms and remove the references to the materials
    public static void resetMaterials(Dictionary<Transform, Material> materials)
    {
        foreach (KeyValuePair<Transform, Material> item in materials) //Reset the interactables' materials
        {
            item.Key.GetComponent<Renderer>().material = materials[item.Key];
        }

        materials.Clear(); //Clear the dictionary
    }

    //Sets the material of a parent's child transforms and records the original materials in a dictionary of materials
    public static void setMaterialOfChildren(Transform parent, Dictionary<Transform, Material> materials, Material newMaterial)
    {
        if (parent.childCount == 0) //No children
        {
            Renderer renderer = parent.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (!materials.ContainsKey(parent))
                {
                    materials.Add(parent, renderer.material);
                }

                renderer.material = newMaterial;
            }
        }
        else
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>())
            {
                if (child != parent)
                {
                    setMaterialOfChildren(child, materials, newMaterial);
                }
            }
        }
    }
}
