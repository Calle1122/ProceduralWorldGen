using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "WFCNode", menuName = "WFC/Node")]
[System.Serializable]
public class WFCNode : ScriptableObject
{
    public string Name;
    public GameObject Prefab;
    public WFCConnection Top, Bottom, Left, Right;
}

[System.Serializable]
public class WFCConnection
{
    public List<WFCNode> CompatibleNodes = new List<WFCNode>();
}
