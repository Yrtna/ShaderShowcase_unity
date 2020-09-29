using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPropertySetter : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color MaterialColor;
    private MaterialPropertyBlock propertyBlock;

    // OnValidate is called in the editor after the component is edited
    void OnValidate()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        var renderer = GetComponentInChildren<Renderer>();
        propertyBlock.SetColor("Color_4E2ABACF", MaterialColor);
        renderer.SetPropertyBlock(propertyBlock);
    }
}