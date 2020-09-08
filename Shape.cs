using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ShapeType {Sphere, Box, Torus, Cone, Cylinder};

public class Shape : MonoBehaviour
{
    public ShapeType type;
    public Color colour = Color.white;
    public Quaternion rotation {
        get {
            return this.transform.rotation;
        }
    }
    public Vector3 scale {
        get {
            return this.transform.localScale;
        }
    }
    public Vector3 position {
        get {
            return this.transform.position;
        }
    }
    public Vector3 extraShapeData = new Vector3(1, 2, 3);

    private void Update() {
        extraShapeData += new Vector3(0.1f * Time.deltaTime, 0, 0);
    }
}

[CustomEditor(typeof(Shape))]
[CanEditMultipleObjects]
public class ShapeEditor: Editor
{
    SerializedProperty shapeType;
    SerializedProperty colour;
    SerializedProperty extraShapeData;

    void OnEnable() {
        shapeType = serializedObject.FindProperty("type");
        colour = serializedObject.FindProperty("colour");
        extraShapeData = serializedObject.FindProperty("extraShapeData");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(shapeType);
        if (shapeType.enumValueIndex == (int)ShapeType.Sphere) {
            extraShapeData.vector3Value = new Vector3(EditorGUILayout.FloatField("Radius", extraShapeData.vector3Value.x), 0, 0);
        } else if (shapeType.enumValueIndex == (int)ShapeType.Torus) {
            float radius = EditorGUILayout.FloatField("Radius", extraShapeData.vector3Value.x);
            float outerRadius = EditorGUILayout.FloatField("Outer Radius", extraShapeData.vector3Value.y);
            extraShapeData.vector3Value = new Vector3(radius, outerRadius, 0);
        } else if (shapeType.enumValueIndex == (int)ShapeType.Cone) {
            float angle = EditorGUILayout.FloatField("Angle", extraShapeData.vector3Value.x);
            float height = EditorGUILayout.FloatField("Height", extraShapeData.vector3Value.y);
            extraShapeData.vector3Value = new Vector3(angle, height, 0);
        } else if (shapeType.enumValueIndex == (int)ShapeType.Cylinder) {
            float height = EditorGUILayout.FloatField("Radius", extraShapeData.vector3Value.x);
            float radius = EditorGUILayout.FloatField("Height", extraShapeData.vector3Value.y);
            extraShapeData.vector3Value = new Vector3(height, radius, 0);
        }

        EditorGUILayout.PropertyField(colour);
        serializedObject.ApplyModifiedProperties();
    }
}
