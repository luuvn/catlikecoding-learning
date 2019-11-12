using System;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    int shapeId;
    Color color;
    MeshRenderer meshRender;
    static int colorPropertyId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        meshRender = GetComponent<MeshRenderer>();
    }

    public int ShapeId
    {
        get
        {
            return shapeId;
        }

        set
        {
            if (shapeId == 0)
            {
                shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId.");
            }
        }
    }

    public int MaterialId { get; private set; }

    public void SetMaterial(Material material, int materialId)
    {
        MaterialId = materialId;
        meshRender.material = material;
    }

    public void SetColor(Color color)
    {
        this.color = color;
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor(colorPropertyId, color);
        meshRender.SetPropertyBlock(propertyBlock);
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(color);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
    }
}
