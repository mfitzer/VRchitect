using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ModelInitializer : MonoBehaviour {
    public Transform model;

	// Use this for initialization
	void Start () {
        intializeModel(model);
	}

    //Configure model for editing
    public void intializeModel(Transform parentTransform)
    {
        Transform parent = parentTransform;
        parent.tag = "ModelParent";

        foreach (Transform child in parent)
        {
            //Debug.Log(child.name);
            if (child != parent)
            {
                if (child.GetComponent<Camera>() != null) //Remove any camera objects
                {
                    //Destroy(child.gameObject);
                    child.GetComponent<Camera>().enabled = false;
                }
                else
                {
                    child.gameObject.tag = "Interactable";
                    child.gameObject.AddComponent<MeshCollider>();

                    //fixTextures(child.GetComponent<Renderer>());

                    //NavMeshObstacle navObstacle = child.gameObject.AddComponent<NavMeshObstacle>();
                    //navObstacle.carving = true;
                    //navObstacle.carveOnlyStationary = true;

                    if (child.childCount > 0) //Process the child's children as well
                    {
                        intializeModel(child);
                    }
                }
            }
        }
    }

    public void fixTextures(Renderer renderer)
    {
        if (renderer != null)
        {
            List<Material> meshMats = new List<Material>();
            renderer.GetMaterials(meshMats);
            //Debug.Log("Child materials: " + meshMats.Count);

            foreach (Material mat in meshMats)
            {
                if (mat.mainTexture != null && mat.mainTexture.name != null)
                {
                    string texName = mat.mainTexture.name;
                    //Debug.Log(texName);
                    texName.Trim('@');
                }
            }
        }
    }
}
