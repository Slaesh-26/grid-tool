using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class GridTool : EditorWindow
{
    private const string UNDO_ACTION_TEXT = "snap selection";
    private const float WINDOW_WIDTH = 300f;
    private const float WINDOW_HEIGHT = 200f;

    [SerializeField] [Min(0.01f)] 
    private float gridStep = 1f;

    [SerializeField] [Range(5, 500)] 
    private int gridSize = 50;

    [SerializeField] 
    private Color color = Color.white;

    [SerializeField] 
    private GridPlane gridPlane = GridPlane.XZ;

    [SerializeField] 
    private float gridOffset;

    [SerializeField] 
    private bool followCamera;

    private SerializedProperty _propColor;
    private SerializedProperty _propFollow;
    private SerializedProperty _propGridPlane;
    private SerializedProperty _propGridSize;
    private SerializedProperty _propGridStep;
    private SerializedProperty _propOffset;

    private SerializedObject _so;
    
    [MenuItem("Tools/Grid Tool")]
    public static void Open()
    {
        GetWindow<GridTool>("Grid Tool");
    }
    
    private void OnEnable()
    {
        minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

        _so = new SerializedObject(this);
        _propColor = _so.FindProperty("color");
        _propOffset = _so.FindProperty("gridOffset");
        _propGridStep = _so.FindProperty("gridStep");
        _propGridSize = _so.FindProperty("gridSize");
        _propGridPlane = _so.FindProperty("gridPlane");
        _propFollow = _so.FindProperty("followCamera");

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void OnGUI()
    {
        _so.Update();
        EditorGUILayout.PropertyField(_propGridPlane);
        EditorGUILayout.PropertyField(_propGridStep);
        EditorGUILayout.PropertyField(_propGridSize);
        EditorGUILayout.PropertyField(_propColor);
        EditorGUILayout.PropertyField(_propFollow);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(_propOffset, new GUIContent(followCamera ? "Follow offset" : "Origin offset"));

            if (GUILayout.Button("Snap Offset"))
            {
                _propOffset.floatValue = Extensions.Snap(gridOffset, gridStep);
                GUI.FocusControl(null);
            }
        }

        using (new EditorGUI.DisabledScope(!AreAnySelectedInSceneView()))
        {
            if (GUILayout.Button("Snap Selection")) SnapSelection();
        }

        if (_so.ApplyModifiedProperties()) SceneView.RepaintAll();
    }

    private void DuringSceneGUI(SceneView view)
    {
        DrawGrid();
    }

    private void DrawGrid()
    {
        if (Event.current.type != EventType.Repaint) return;
        
        SceneView current = SceneView.currentDrawingSceneView;
        Vector3 camPos = Vector3.zero;

        if (current != null) camPos = current.camera.transform.position;

        float drawDistance = gridStep * gridSize;

        Vector3 startPos = camPos;
        Vector3 fromAxis;
        Vector3 alongAxis;

        switch (gridPlane)
        {
            case GridPlane.XY:
                fromAxis = Vector3.right;
                alongAxis = Vector3.up;
                startPos.z = camPos.z * (followCamera ? 1 : 0) + gridOffset;
                break;
            case GridPlane.XZ:
                fromAxis = Vector3.right;
                alongAxis = Vector3.forward;
                startPos.y = camPos.y * (followCamera ? 1 : 0) + gridOffset;
                break;
            case GridPlane.YZ:
                fromAxis = Vector3.up;
                alongAxis = Vector3.forward;
                startPos.x = camPos.x * (followCamera ? 1 : 0) + gridOffset;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Vector3 offset = GetGridDrawOffset(fromAxis, alongAxis);
        startPos -= offset;

        DrawLines(fromAxis, alongAxis, startPos, gridSize + 1, drawDistance);
        DrawLines(alongAxis, fromAxis, startPos, gridSize + 1, drawDistance);
    }

    private Vector3 GetGridDrawOffset(Vector3 firstAxis, Vector3 secondAxis)
    {
        Vector3 offset = (firstAxis + secondAxis) * (gridSize * gridStep) / 2f;
        return offset;
    }

    private bool AreAnySelectedInSceneView()
    {
        return Selection.gameObjects.Length > 0;
    }

    private void SnapSelection()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, UNDO_ACTION_TEXT);
            go.transform.position = go.transform.position.Round(gridStep);
        }
    }

    private void DrawLines(Vector3 axisFrom, Vector3 axisAlong, Vector3 drawStartPos, int amount, float lineLength)
    {
        Vector3 snappedWorldPos = drawStartPos.Round(gridStep, gridPlane);
        Handles.zTest = CompareFunction.LessEqual;
        Handles.color = color;

        for (int i = 0; i < amount; i++)
        {
            Vector3 snappedPos = snappedWorldPos + axisFrom * (i * gridStep);
            Vector3 startPoint = snappedPos;
            Vector3 endPoint = snappedPos + axisAlong * lineLength;

            Handles.DrawLine(startPoint, endPoint);
        }
    }
}