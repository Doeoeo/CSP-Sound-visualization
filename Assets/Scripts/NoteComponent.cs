using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct NoteComponent : IComponentData {
    public float3 direction;
    public float3 startingPos;
    public float distance;
    public float3 offsetY;
    public int index;
}
