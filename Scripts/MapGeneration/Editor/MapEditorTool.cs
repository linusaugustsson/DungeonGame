using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for not to force this tool onto player if using the map editor, can be used to draw things that can help use the map editor.
/// </summary>
[EditorTool("Map Editor Tool")]
class MapEditorTool : EditorTool
{
    MapEditor mapEditor;
    void OnEnable()
    {
        mapEditor = FindObjectOfType<MapEditor>();
        SceneView.duringSceneGui += OnSceneGUI;
        EditorSceneManager.sceneOpened += OnNewScene;
    }

    private void OnNewScene(Scene scene, OpenSceneMode mode)
    {
        mapEditor = FindObjectOfType<MapEditor>(); //If the scene have a MapEditor, fetch it.
    }


    private void OnSceneGUI(SceneView obj)
    {
        if(mapEditor) //If we have a mapEditor in the scene, force tool to be used and allow no selection in scene
        {
            ToolManager.SetActiveTool<MapEditorTool>();
            GameObject gameObject = Selection.activeObject as GameObject;
            if(gameObject && gameObject.scene != null && gameObject.scene.IsValid())
            {
                Selection.activeObject = mapEditor.gameObject;
            }
        }
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorSceneManager.sceneOpened -= OnNewScene;
    }
}
