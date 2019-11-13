using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Game : PersistableObject
{
    const int version = 2;

    public PersistentStorage storage;
    public ShapeFactory shapeFactory;

    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    private List<Shape> shapes;

    void Awake()
    {
        shapes = new List<Shape>();
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
            CreateShape();
        }

        if (Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
        }

        if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this, version);
        }

        if (Input.GetKeyDown(loadKey))
        {
            storage.Load(this);
        }
    }

    void CreateShape()
    {
        Shape shape = shapeFactory.GetRandom();
        Transform t = shape.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        shape.SetColor(Random.ColorHSV(hueMin: 0f, hueMax: 1f,
            saturationMin: 0.5f, saturationMax: 1f,
            valueMin: 0.25f, valueMax: 1f,
            alphaMin: 1f, alphaMax: 1f)
            );

        shapes.Add(shape);
    }

    void BeginNewGame()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            Destroy(shapes[i].gameObject);
        }

        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
        for (int i = 0; i < shapes.Count; i++)
        {
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }


    public override void Load(GameDataReader reader)
    {
        BeginNewGame();

        int version = reader.Version;
        if (version > Game.version)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        int count = version <= 0 ? -version : reader.ReadInt();

        for (int i = 0; i < count; i++)
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape obj = shapeFactory.Get(shapeId, materialId);
            obj.Load(reader);
            shapes.Add(obj);
        }
    }
}

