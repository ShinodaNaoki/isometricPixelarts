﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaterViewMesh : HasIDColor
{
    private static Vector4[] AdjustVectors = new Vector4[]
    {
        new Vector4(1f,1f,1f,1f), new Vector4(1f,1.155f,1f,1f), new Vector4(0.866f,1f,0.866f,1f)
    }; 

    // Start is called before the first frame update
    void Start()
    {
        var material = GetComponent<MeshRenderer>().material;
        Vector4 vect = (Color)IDColor;
        material.SetVector("_IDColor", vect);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
