using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
[UpdateAfter(typeof(TimeFilterApplyBase))]
public class FilterApplyBase : SystemBase {
    int b;

    protected override void OnUpdate() {
        int multiplier = NoteEntityCreator.multiplier;
        NativeArray<float> freqData = new NativeArray<float>(AudioGenerator.frequncyDomain.Length / 2, Allocator.TempJob);
        for (int i = 0; i < freqData.Length; i++) freqData[i] = 0;
        

        Entities
        .WithAll<FilterComponent>()
        .ForEach((Entity entity, ref Translation notePos, ref FilterComponent filterData, ref NoteComponent noteData) => {
            int index = noteData.index / multiplier;
            int pos = noteData.index % multiplier;

            if (pos == 0){
                notePos.Value = noteData.offsetY + noteData.startingPos + noteData.direction * filterData.weight * noteData.distance;
                freqData[index] = filterData.weight;
            }
            else if (index == 0) notePos.Value = noteData.offsetY + noteData.startingPos + noteData.direction * filterData.weight * noteData.distance;
            else {
                notePos.Value = noteData.offsetY + noteData.startingPos + (noteData.direction * filterData.weight * noteData.distance * (multiplier - pos) + noteData.direction * filterData.weight * noteData.distance * (pos)) / multiplier;
            }

        }).Run();


        for(int i = 0; i < freqData.Length; i++) {
            AudioGenerator.frequncyDomain[i] = AudioGenerator.frequncyDomain[i] * freqData[i];
            AudioGenerator.fft[i] *= freqData[i];
            AudioGenerator.fft[AudioGenerator.fft.Length - i - 1] *= freqData[i];
        }

        
        b = AudioGenerator.revert();

        freqData.Dispose();
    }
}