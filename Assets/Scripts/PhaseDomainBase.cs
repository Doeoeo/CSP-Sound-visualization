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
public class PhaseDomainBase : SystemBase {

    protected override void OnUpdate() {
        NativeArray<float> waveData = new NativeArray<float>(AudioGenerator.timeDomain, Allocator.TempJob);
        int multiplier = NoteEntityCreator.multiplier;
        float max = AudioGenerator.sampleExt[0], min = AudioGenerator.sampleExt[1];
        float size = 200;
        float interval = max - min;
        Debug.Log(" asdf " + waveData.Length);
        int tao = UiHandler.tao;

        Entities
            .WithAll<PhaseComponent>()
            //.WithReadOnly(waveData)
            .ForEach((Entity note, ref Translation notePos, ref NoteComponent noteData) => {
                int index = noteData.index / multiplier;
                int pos = noteData.index % multiplier;
                if (index - tao >= 1) {
                    if (pos == 0) {
                        float x = (waveData[index] - min) / interval * size;
                        float y = (waveData[index - tao] - min) / interval * size;
                        notePos.Value = new float3(x - size / 2, y + noteData.offsetY.y - size / 2, notePos.Value.z);
                    } else {
                        float x = ((waveData[index] - min) * pos + (waveData[index - 1] - min) * (multiplier - pos)) * size / (interval * multiplier);
                        float y = ((waveData[index - tao] - min) * pos + (waveData[index - 1 - tao] - min) * (multiplier - pos)) * size / (interval * multiplier);
                        notePos.Value = new float3(x - size / 2, y + noteData.offsetY.y - size / 2, notePos.Value.z);
                    }
                    //Debug.Log(x.ToString());
                    //Debug.Log(y.ToString());
                } else notePos.Value.x = -500;
            }).Run();
        waveData.Dispose();

    }
}
