using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing.Scened;

public class SceneLoading : MonoBehaviour
{

    
    
    public void LoadMap()
    {
        SceneLoadData sceneLoadData = new SceneLoadData("CatacombsMap");
        //sceneLoadData.MovedNetworkObjects = new FishNet.Object.NetworkObject[] { };
        sceneLoadData.Options.Addressables = true;
        sceneLoadData.ReplaceScenes = ReplaceOption.None;
        sceneLoadData.Options.AutomaticallyUnload = false;
        InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
    }

}
