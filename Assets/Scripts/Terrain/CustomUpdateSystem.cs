//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;


//public class CustomUpdateSystem : JobComponentSystem
//{
//    static readonly float TimeStep = 2.0f / 30.0f;
//    float processedTime = 0.0f;
//    TerrainGenerationGroup terrainGenerationGroup;

//    protected override void OnCreateManager()
//    {
//        terrainGenerationGroup = Bootstrapped.tGenGroup;
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        float deltaTime = Time.deltaTime;

//        if ((deltaTime - this.processedTime) < TimeStep)
//        {
//            this.processedTime -= deltaTime;
//            return inputDeps;
//        }

//        while ((deltaTime - this.processedTime) >= TimeStep)
//        {
//            this.processedTime += TimeStep;

//            terrainGenerationGroup.Update();
//        }


//        this.processedTime -= deltaTime;

//        return inputDeps;
//    }
//}
