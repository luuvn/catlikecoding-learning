using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Game : PersistableObject
{
    public PersistableObject prefab;
    public PersistentStorage storage;

    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    private List<PersistableObject> objects;


    void Awake()
    {
        objects = new List<PersistableObject>();
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

        if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this); 
        }

        if (Input.GetKeyDown(loadKey))
        {
            storage.Load(this);
        }
    }

    void CreateObject()
    {
        PersistableObject obj = Instantiate(prefab);
        Transform t = obj.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);

        objects.Add(obj);
    }

    void BeginNewGame()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            Destroy(objects[i].gameObject);
        }

        objects.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(objects.Count);
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].Save(writer);
        }
    }


    public override void Load(GameDataReader reader)
    {
        BeginNewGame();

        int count = reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            PersistableObject obj = Instantiate(prefab);
            obj.Load(reader);
            objects.Add(obj);
        }
    }
}

