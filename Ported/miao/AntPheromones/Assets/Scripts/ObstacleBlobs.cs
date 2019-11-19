﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AntPheromones_ECS
{
    public struct ObstacleBlobs
    {
        public struct Bucket
        {
            public BlobArray<Obstacle> Obstacles;
        }
        
        public BlobArray<Bucket> Buckets;
        public BlobArray<Obstacle> Empty;
        public BlobArray<Obstacle> Obstacles;
        private BlobArray<Matrix4x4> Matrices;
        
        private const int BucketResolution = 64;
        private const int RingCount = 3;
        private const int Radius = 2;
        private const float NumObstaclesPerRing = 0.8f;
        private const int MaxHoleCount = 3;
        private const int MinHoleCount = 1;

        public static BlobAssetReference<ObstacleBlobs> Generate()
        {
            using (BlobBuilder builder = new BlobBuilder(Allocator.Temp))
            {
                ref ObstacleBlobs root = ref builder.ConstructRoot<ObstacleBlobs>();

                builder.Allocate(ref root.Empty, length: 0);
                
                BlobBuilderArray<Obstacle> obstacles = builder.Allocate(ref root.Obstacles, length: 250); // TODO: 250 is a placeholder

                for (int ring = 0; ring < RingCount; ring++)
                {
                    float ringRadius = ring / (RingCount + 1f) * (Map.Width * 0.5f);
                    float circumference = ringRadius * 2f * Mathf.PI;

                    int maxObstacleCount = Mathf.CeilToInt(circumference / (2f * Radius) * 2f);

                    int offset = Random.Range(0, maxObstacleCount);
                    int holeCount = Random.Range(MinHoleCount, MaxHoleCount);

                    for (int obstacle = 0; obstacle < maxObstacleCount; obstacle++)
                    {
                        float t = (float)obstacle / maxObstacleCount;

                        if (t * holeCount % 1f >= NumObstaclesPerRing)
                        {
                            continue;
                        }
                        
                        float angle = (obstacle + offset) / (float) maxObstacleCount * (2f * Mathf.PI);
                        obstacles[ring] = new Obstacle
                        {
                            Position = 
                                new float3(
                                    Map.Width * 0.5f + Mathf.Cos(angle) * ringRadius, 
                                    Map.Width * .5f + Mathf.Sin(angle) * ringRadius,
                                    0),
                            Radius = Radius
                        };
                    }
                }

                BlobBuilderArray<Matrix4x4> obstacleMatrices = builder.Allocate(ref root.Matrices, length: obstacles.Length);

                for (int i = 0; i < obstacleMatrices.Length; i++)
                {
                    obstacleMatrices[i] = float4x4.TRS(
                            obstacles[i].Position / Map.Width, Quaternion.identity,
                            new float3(Radius * 2f, Radius * 2f, 1f) / Map.Width);
                }

                var obstacleBuckets = builder.Allocate(ref root.Buckets, length: BucketResolution * BucketResolution);
//
//                for (int i = 0; i < obstacleBuckets.Length; i++)
//                {
//                    obstacleBuckets[i] = builder.Allocate(ref root.) // new BlobArray<Obstacle>();
//                }

                for (int i = 0; i < obstacles.Length; i++)
                {
                    
                    float3 position = obstacles[i].Position;
                    float radius = obstacles[i].Radius;

                    for (int x = Mathf.FloorToInt((position.x - radius) / Map.Width * BucketResolution);
                        x <= Mathf.FloorToInt((position.x + radius) / Map.Width * BucketResolution);
                        x++)
                    {
                        if (x < 0 || x >= BucketResolution)
                        {
                            continue;
                        }

                        int currentBucketIndex = x * BucketResolution + BucketResolution;
                        int currentBlobArrayWriteIndex = 0;
                        
                        for (int y = Mathf.FloorToInt((position.y - radius) / Map.Width * BucketResolution);
                            y <= Mathf.FloorToInt((position.y + radius) / Map.Width * BucketResolution);
                            y++)
                        {
                            if (y < 0 || y >= BucketResolution)
                            {
                                continue;
                            }

//                            obstacleBuckets[currentBucketIndex][currentBlobArrayWriteIndex] = obstacles[i];
                            currentBucketIndex++;
                        }
                    }
                }

                return builder.CreateBlobAssetReference<ObstacleBlobs>(Allocator.Persistent);
            }
        }

//        public BlobArray<Obstacle> GetObstacleBucket(float candidateDestinationX, float candidateDestinationY)
//        {
//            int x = (int)(candidateDestinationX / Map.Width * BucketResolution);
//            int y = (int)(candidateDestinationY / Map.Width * BucketResolution);
//            
//            return Map.IsWithinBounds(x, y) ? Empty : this.Buckets[x * BucketResolution + BucketResolution];
//        }
    }
}