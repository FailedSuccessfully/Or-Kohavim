using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Or Kohavim/VoltControl")]
public class VoltControl : ScriptableObject
{
    [Range(0,1)]
    public float minVolt, maxVolt;
}
