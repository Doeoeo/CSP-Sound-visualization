using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class TimeFilterApplyBase : SystemBase {
    int b;

    protected override void OnUpdate() {
        int multiplier = NoteEntityCreator.multiplier;
        NativeArray<float> timeDomain = new NativeArray<float>(AudioGenerator.timeDomain, Allocator.TempJob);
        Debug.Log("asdasf");
        Entities
        .WithAll<TimeFilterComponent>()
        .ForEach((Entity entity, ref Translation notePos, ref TimeFilterComponent filterData, in NoteComponent noteData) => {
            int index = noteData.index / multiplier;
            int pos = noteData.index % multiplier;

            if (pos == 0) {
                notePos.Value = noteData.offsetY + noteData.startingPos + filterData.weight * noteData.distance;
                if (filterData.weight > 0 && filterData.weight < timeDomain[index]) timeDomain[index] = filterData.weight;
                else if (filterData.weight < 0 && filterData.weight > timeDomain[index]) timeDomain[index] = filterData.weight;

            } else if (index == 0) notePos.Value = noteData.offsetY + noteData.startingPos + filterData.weight * noteData.distance;
            else {
                notePos.Value = noteData.offsetY + noteData.startingPos + (filterData.weight * noteData.distance * (multiplier - pos) + filterData.weight * noteData.distance * (pos)) / multiplier;
            }

        }).Run();

        for (int i = 0; i < timeDomain.Length; i++) AudioGenerator.timeDomain[i] = timeDomain[i];
        AudioGenerator.postTimeUpdate();
        timeDomain.Dispose();
    }
}