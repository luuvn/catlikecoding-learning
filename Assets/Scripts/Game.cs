using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Transform prefab;
    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;

    private List<Transform> objects;
    private string savePath;


    void Awake()
    {
        objects = new List<Transform>();
        savePath = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(createKey))
        {
            CreateObject();
        }

        if (Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
        }
    }

    void CreateObject()
    {
        Transform t = Instantiate(prefab);
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);

        objects.Add(t);
    }

    void BeginNewGame()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            Destroy(objects[i].gameObject);
        }

        objects.Clear();
    }

    void Save()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
        { 
            
        };
    }
}
