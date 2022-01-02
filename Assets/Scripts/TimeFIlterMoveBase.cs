using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class TimeFilterMoveBase : SystemBase {
    protected override void OnUpdate() {
        float movedW = FilterGenerator.timeFilter;
        Entities
        .WithAll<TimeFilterComponent>()
        .ForEach((Entity entity, ref TimeFilterComponent filterData, ref NoteComponent noteData) => {
            filterData.weight = movedW * noteData.direction.y;
        }).Run();

    }
}