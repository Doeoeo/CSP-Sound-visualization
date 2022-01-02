using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class FilterMoveBase : SystemBase {
    protected override void OnUpdate() {
        NativeArray<float> movedFilter = new NativeArray<float>(FilterGenerator.movedFilter, Allocator.TempJob);

        Entities
        .WithAll<FilterComponent>()
        .ForEach((Entity entity, ref FilterComponent filterData, ref NoteComponent noteData) => {
            filterData.weight = movedFilter[noteData.index];
        }).Run();

        movedFilter.Dispose();

    }
}




