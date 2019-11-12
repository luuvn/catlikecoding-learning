using UnityEngine;

[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    [SerializeField]
    Shape[] prefabs;

    [SerializeField]
    Material[] materials;

    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        Shape obj = Instantiate(prefabs[shapeId]);
        obj.ShapeId = shapeId;
        obj.SetMaterial(materials[materialId], materialId);
        return obj;
    }

    public Shape GetRandom()
    {
        return Get(
            Random.Range(0, prefabs.Length),
            Random.Range(0, materials.Length)
            );
    }
}
