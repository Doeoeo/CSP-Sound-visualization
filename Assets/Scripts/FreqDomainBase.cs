using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[UpdateAfter(typeof(FilterApplyBase))]

public class FilterBase : SystemBase {
    protected override void OnUpdate() {
        Debug.Log(" Drawing freq ");

        NativeArray<float> freqData = new NativeArray<float>(AudioGenerator.frequncyDomain, Allocator.TempJob);
        int multiplier = NoteEntityCreator.multiplier;

        Entities
            .WithAll<FrequencyComponent>()
            //.WithReadOnly(freqData)
            .ForEach((Entity note, ref Translation notePos, ref NoteComponent noteData) => {
                int index = noteData.index / multiplier;
                int pos = noteData.index % multiplier;
                //Debug.Log(index + "at pos " + pos + "?" + (index - 1));

                if (pos == 0) notePos.Value = noteData.offsetY + noteData.startingPos + noteData.direction * freqData[index] * noteData.distance;
                else if (index == 0) notePos.Value = noteData.offsetY + noteData.startingPos + noteData.direction * freqData[index] * noteData.distance;
                else {
                    notePos.Value = noteData.offsetY + noteData.startingPos + (noteData.direction * freqData[index - 1] * noteData.distance * (multiplier - pos) + noteData.direction * freqData[index] * noteData.distance * (pos)) / multiplier;
                }
            }).Run();
        freqData.Dispose();
    }
}
