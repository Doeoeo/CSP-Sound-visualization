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
public class TimeDomainBase : SystemBase {
   
    protected override void OnUpdate() {
        NativeArray<float> waveData = new NativeArray<float>(AudioGenerator.timeDomain, Allocator.TempJob);
        int multiplier = NoteEntityCreator.multiplier;
        Debug.Log(" Drawing time ");

        Entities
            .WithAll<TimeComponent>()
            //.WithReadOnly(waveData)
            .ForEach((Entity note, ref Translation notePos, ref NoteComponent noteData) => {
                int index = noteData.index / multiplier;
                int pos = noteData.index % multiplier;
                //Debug.Log(index + "at pos " + pos + "?" + (index - 1));

                if (pos == 0) notePos.Value = noteData.offsetY + noteData.startingPos + noteData.direction * waveData[index] * noteData.distance;
                else if(index == 0) notePos.Value = noteData.offsetY + noteData.startingPos + (noteData.direction * waveData[index] * noteData.distance * (pos)) / multiplier;
                else {
                    notePos.Value = noteData.offsetY + noteData.startingPos + (noteData.direction * waveData[index - 1] * noteData.distance * (multiplier - pos) + noteData.direction * waveData[index] * noteData.distance * (pos)) / multiplier;
                }
            }).Run();
        waveData.Dispose();

    }
}
