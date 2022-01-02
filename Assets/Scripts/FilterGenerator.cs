using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using System;

public class FilterGenerator : MonoBehaviour
{
    //[SerializeField] Func<float, float> f;
    [SerializeField] static float begin;
    [SerializeField] static float end;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    private int agentNumber;
    private float startX, width, q = 0.001f, lastValue, lMax = 0;
    public static float[] filter;

    EntityManager entityManager;
    EntityArchetype freqFilterArchetype, timeFilterArchetype;
    public Func<float, float, float> currentFilter;
    public static float[] movedFilter;
    public static float timeFilter;
    // Start is called before the first frame update
    void Start() {
        width = NoteEntityCreator.width;
        agentNumber  = AudioGenerator.timeDomain.Length * NoteEntityCreator.multiplier / 2;
        startX = -agentNumber / 2 * width;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        freqFilterArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(NonUniformScale),
            typeof(NoteComponent),
            typeof(FilterComponent)
        );

        timeFilterArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(NonUniformScale),
            typeof(NoteComponent),
            typeof(TimeFilterComponent)
        );

        filter = new float[agentNumber];
        movedFilter = new float[agentNumber];

        World.DefaultGameObjectInjectionWorld.GetExistingSystem(typeof(FilterMoveBase)).Enabled = false;
    }

    public void filterChanged(Func<float, float, float> filterFun, int shift) {
        clearFilter();
        currentFilter = filterFun;
        makeFilter(freqFilterArchetype, entityManager, filterFun, shift);
    }
    public void timeFilterChanged(Func<float, float, float> filterFun, int direction, bool clear) {
        if (clear) clearTimeFilter();
        currentFilter = filterFun;
        makeTimeFilter(timeFilterArchetype, entityManager, filterFun, direction);
    }

    public void clearFilter() {
        NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);
        foreach (Entity e in entities) if (entityManager.HasComponent<FilterComponent>(e)) entityManager.DestroyEntity(e);
        entities.Dispose();
    }
    public void clearTimeFilter() {
        NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);
        foreach (Entity e in entities) if (entityManager.HasComponent<TimeFilterComponent>(e)) entityManager.DestroyEntity(e);
        entities.Dispose();
    }

    public void filterMoved(int shift) {
        movedFilter = new float[filter.Length];
        for (int i = 0; i < agentNumber; i++) {
            if (i + shift >= filter.Length) movedFilter[(shift + i) - filter.Length] = filter[0];
            else if (i + shift < 0) movedFilter[filter.Length + (shift + i)] = filter[filter.Length - 1];
            else movedFilter[i + shift] = filter[i];
        }
        World.DefaultGameObjectInjectionWorld.GetExistingSystem(typeof(FilterMoveBase)).Enabled = true;
    }

    public void timeFilterMoved(float shift) {
        timeFilter = shift;
        World.DefaultGameObjectInjectionWorld.GetExistingSystem(typeof(FilterMoveBase)).Enabled = true;
    }

    private void makeFilter(EntityArchetype filterArchetype, EntityManager entityManager, Func<float, float, float> f, int shift) {
        clearFilter();
        //if (shift < 0) shift = filter.Length + shift;
        for (int i = 0; i < agentNumber; i++) filter[i] = f(startX + i * width, 0);      

        NativeArray < Entity> filterArray = new NativeArray<Entity>(agentNumber, Allocator.Temp);

        entityManager.CreateEntity(filterArchetype, filterArray);

        for (int i = 0; i < filterArray.Length; i++) {
            Entity note = filterArray[i];
            float3 startPos = new float3(startX + i * width, -100, 0);
            entityManager.SetComponentData(note, new NoteComponent {
                // Change 0F 
                direction = new float3(0, 1, 0),
                startingPos = startPos,
                distance = 50f,
                index = i,
                offsetY = new float3(0,-50,0)
            });
            entityManager.SetComponentData(note, new FilterComponent {
                weight = filter[i]
            });
            entityManager.SetComponentData(note, new Translation {
                Value = startPos
            });

            entityManager.SetSharedComponentData(note, new RenderMesh {
                mesh = mesh,
                material = material
            });

            entityManager.SetComponentData(note, new NonUniformScale {
                Value = new float3(1, 1, 1)
            });

        }
        filterArray.Dispose();
    }

    private void makeTimeFilter(EntityArchetype filterArchetype, EntityManager entityManager, Func<float, float, float> f, int shift) {
        clearFilter();
        //if (shift < 0) shift = filter.Length + shift;
        timeFilter = f(0, 0);
        NativeArray<Entity> filterArray = new NativeArray<Entity>(agentNumber * 2, Allocator.Temp);

        entityManager.CreateEntity(filterArchetype, filterArray);

        for (int i = 0; i < filterArray.Length; i++) {
            Entity note = filterArray[i];
            float3 startPos = new float3(startX * 2 + i * width, 0, 0);
            entityManager.SetComponentData(note, new NoteComponent {
                // Change 0F 
                direction = new float3(0, 1 * shift, 0),
                startingPos = startPos,
                distance = 100f,
                index = i,
                offsetY = new float3(0, 100, 0)
            });
            entityManager.SetComponentData(note, new TimeFilterComponent {
                weight = timeFilter
            });
            entityManager.SetComponentData(note, new Translation {
                Value = startPos
            });

            entityManager.SetSharedComponentData(note, new RenderMesh {
                mesh = mesh,
                material = material
            });

            entityManager.SetComponentData(note, new NonUniformScale {
                Value = new float3(1, 1, 1)
            });

        }
        filterArray.Dispose();
    }

    public float highPass(float i, float threshold) {
        float omega = 0.4f;
        float n = math.pow(math.E, -1 *(i - threshold) * omega);
        if (math.abs(n) > 20) return 0;
        if (math.abs(n) < 0.0000000001) return 1f;
        return lastValue = math.abs(math.sin(omega * math.PI * n) / (math.PI * n)) / 0.4f;
    }

    public float lowPass(float i, float threshold) {
        float omega = 0.4f;
        float n = math.pow(math.E, (i - threshold) * omega);
        if (math.abs(n) > 20) return 0;
        if (math.abs(n) < 0.0000000001) return 1;
        lMax = math.max(lMax, math.abs(math.sin(omega * math.PI * n) / (math.PI * n)));

        return lastValue = math.min(math.abs(math.sin(omega * math.PI * n) / (math.PI * n)) / 0.4f, 1);
    }

    public float bandPass(float i, float threshold) {
        return filterNester(i, threshold, highPass, lowPass);
    }

    public float bandStop(float i, float threshold) {
        return filterNester(i, threshold, lowPass, highPass);
    }

    public float filterNester(float i, float threshold, Func<float, float, float> f, Func<float, float, float> g) {
        if (i < threshold) return f(i, threshold - 3);
        else return g(i, threshold + 3);
    }

    public float ampFilter(float i, float threshold) {
        return threshold;
    }
}
