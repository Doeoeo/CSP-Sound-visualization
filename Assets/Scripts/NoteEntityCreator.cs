using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;


public class NoteEntityCreator : MonoBehaviour {

    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    public static float width = 0.5f;
    private int agentNumber;
    private float startX, distance;
    private float3 localScale, offsetY;
    public static int multiplier = 31;
    void Awake() {
        #region Inital data
        agentNumber = AudioGenerator.timeDomain.Length * multiplier;
        width = AudioGenerator.timeDomain.Length / (float)agentNumber;

        startX = -agentNumber / 2 * width;
        localScale = new float3(1f, 1f, 1);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        #endregion Inital data

        #region Time domain notes
        offsetY = new float3(0, 100, 0);
        distance = 100f;

        EntityArchetype timeNoteArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(NonUniformScale),
            typeof(NoteComponent),
            typeof(TimeComponent)
        );


        makeNotes(timeNoteArchetype, entityManager);

        #endregion Time domain notes

        #region Phase domain
        offsetY = new float3(0, -30, 0);
        EntityArchetype phaseNoteArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(NonUniformScale),
            typeof(NoteComponent),
            typeof(PhaseComponent)
        );
        makeNotes(phaseNoteArchetype, entityManager);
        #endregion Phase domain

        #region Freq domain notes
        offsetY = new float3(0, -150, 0);
        distance = 600f;
        // We only want to visualize the first half of the FFT
        agentNumber /= 2;
        width *= 2;

        EntityArchetype freqNoteArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(NonUniformScale),
            typeof(NoteComponent),
            typeof(FrequencyComponent)
        );
        //agentNumber = AudioGenerator.timeDomain.Length * multiplier;
        makeNotes(freqNoteArchetype, entityManager);

        #endregion Freq domain 



        Debug.Log("Notes were made");
    }

    private void makeNotes(EntityArchetype noteArchetype, EntityManager entityManager) {
        NativeArray<Entity> noteArray = new NativeArray<Entity>(agentNumber, Allocator.Temp);

        entityManager.CreateEntity(noteArchetype, noteArray);

        for (int i = 0; i < noteArray.Length; i++) {
            Entity note = noteArray[i];
            //fix offsetY
            entityManager.SetComponentData(note, new NoteComponent {
                direction = new float3(0, 1, 0),
                startingPos = new float3(startX + i * width, 0, 0),
                distance = distance,
                index = i,
                offsetY = offsetY
            });

            entityManager.SetComponentData(note, new Translation {
                Value = new float3(startX + i * width, 0, 0)
            });

            entityManager.SetSharedComponentData(note, new RenderMesh {
                mesh = mesh,
                material = material
            });

            entityManager.SetComponentData(note, new NonUniformScale {
                Value = localScale
            });

        }
        noteArray.Dispose();
    }
}
