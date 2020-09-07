using UnityEngine;
using System.Collections;

public class MaterialSelector : MonoBehaviour
{
    [SerializeField] 
    private Material[] materials = default;

    [SerializeField] 
    private MeshRenderer meshRenderer = default;

    public void Select(int index)
    {
        if (meshRenderer && materials != null && index >= 0 && index < materials.Length)
        {
            meshRenderer.material = materials[index];
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
