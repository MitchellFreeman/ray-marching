using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class MasterScript : MonoBehaviour
{
    public ComputeShader marchingShader;

    public bool blend = false;

    private RenderTexture target;

    private Camera camera;

    private ComputeBuffer shapes;
    private int numShapes;

    private Light lightSource;

    struct Data {
        public Vector3 position;
        public Vector3 colour;
        public int type;
        public Vector3 scale;
        public Vector3 rotation;
        public Vector3 extraData;

        public static int getSize() {
            return sizeof(float) * 3 * 5 + sizeof(int);
        }
    }

    struct OperationData {
        public int type;
        public int child1;
        public int child2;
        public int blendStrength;
    }

    private List<Data> orderedShapes;
    private List<OperationData> orderedOperations;

    private void Start() {
        Application.targetFrameRate = 24;
    }

    private int frame = 0;
/*
    private void Update() {
        camera = Camera.main;
        InitRenderTexture();
        GetShapes();
        SetParameters();

        marchingShader.SetTexture(0, "Result", target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        marchingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        //Graphics.Blit(target, destination);

        Texture2D textureToSave = new Texture2D(Screen.width, Screen.height);
        byte[] bytes = textureToSave.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";
        if(!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + frame.ToString() +".png", bytes);

        frame++;

        shapes.Dispose();
    }*/

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        camera = Camera.current;
        InitRenderTexture();
        GetShapes();
        SetParameters();

        marchingShader.SetTexture(0, "Result", target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        marchingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, destination);

        shapes.Dispose();
    }

    private void GetShapes() {
        lightSource = FindObjectOfType<Light>();
        List<Shape> shapeList = new List<Shape>(FindObjectsOfType<Shape>());
        List<Data> data = new List<Data>();
        foreach (Shape shape in shapeList) {
            data.Add(new Data() {
                position = shape.position,
                colour = new Vector3(shape.colour.r, shape.colour.g, shape.colour.b),
                type = (int)shape.type,
                scale = shape.scale,
                rotation = shape.rotation.eulerAngles,
                extraData = shape.extraShapeData
                });
        }
        numShapes = data.Count;
        shapes = new ComputeBuffer(data.Count, Data.getSize());
        shapes.SetData(data);
    }

    private void SetParameters() {
        marchingShader.SetMatrix("cameraToWorld", camera.cameraToWorldMatrix);
        marchingShader.SetMatrix("cameraInverseProjection", camera.projectionMatrix.inverse);
        marchingShader.SetVector("lightPos", lightSource.transform.position);
        marchingShader.SetInt("numShapes", numShapes);
        marchingShader.SetInt("blend", blend ? 1 : 0);
        marchingShader.SetBuffer(0, "shapes", shapes);
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != Screen.width || target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (target != null)
                target.Release();

            // Get a render target for Ray Tracing
            target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }
}
