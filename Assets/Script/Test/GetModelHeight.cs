using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetModelHeight : MonoBehaviour
{
    MeshFilter meshFilter;
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        if(meshFilter != null)
        {
            float height = meshFilter.mesh.bounds.size.y;
            Debug.Log(height);
        }
    }


}
