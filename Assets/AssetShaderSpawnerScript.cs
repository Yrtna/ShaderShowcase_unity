using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetShaderSpawnerScript : MonoBehaviour
{
    public int numberToSpawn = 1;
    public float spacing = 1.5f;
    public GameObject go;
    public Vector3 direction;

    private static readonly int EmissionColor = Shader.PropertyToID("EmissionColor");

    // Start is called before the first frame update
    void Start()
    {
        var colorStep = 1f / numberToSpawn;
        for (int i = 0; i < numberToSpawn; i++)
        {
            var obj = Instantiate(go, transform.position + (direction * spacing) * i, Quaternion.identity, this.transform);
            var propertyBlock = new MaterialPropertyBlock();
            var componentInChildren = obj.GetComponentInChildren<Renderer>();

            propertyBlock.SetColor(EmissionColor, Random.ColorHSV(colorStep*i, colorStep*i, 0.8f, 1f, 0.8f, 1f)*2f);
            componentInChildren.SetPropertyBlock(propertyBlock);
        }
    }
}