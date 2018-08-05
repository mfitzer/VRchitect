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

            if (child.GetComponent<Camera>() != null) //Remove any camera objects
            {
                Destroy(child.gameObject);
            }
            else
            {
                child.gameObject.tag = "Interactable";
                child.gameObject.AddComponent<MeshCollider>();

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
