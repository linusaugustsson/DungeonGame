using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkBehaviour), true)]
public class NetworkedCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {        
        if (GUILayout.Button("NetworkBehaviour"))
        {
            Debug.Log("This component is Networked");
        }

        // will enable the default inpector UI 
        base.OnInspectorGUI();

        //base.DrawDefaultInspector();
    }

}
