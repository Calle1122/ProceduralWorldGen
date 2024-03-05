using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "RGNode", menuName = "RunGuyGeneration/Node")]
[System.Serializable]
public class RGNode : ScriptableObject
{
    public GameObject prefab;
    
    [EnumFlagsAttribute]
    public RGConnection connections;
    
    public List<string> ReturnConnections()
    {
        List<string> selectedElements = new List<string>();
     
        for (int i = 0; i < Enum.GetValues(typeof(RGConnection)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)connections& layer) != 0)
            {
                selectedElements.Add(Enum.GetValues(typeof(RGConnection)).GetValue(i).ToString());
            }
        }
 
        return selectedElements;
    }
}

[System.Flags]
public enum RGConnection
{
    None,
    Up,
    Down,
    Left,
    Right
}