// using UnityEngine;
// using System;
// using System.Collections.Generic;
//
// namespace More_World_Locations_AIO.Totems;
//
// public class TotemSpawner : MonoBehaviour
// {
//     public GameObject spawnPrefab;
//     public int spawnInterval;
//     public float spawnTimer;
//     public ZNetView znetView;
//     public int maxNear;
//     public float nearRadius;
//     public float farRadius;
//     public bool onGroundOnly;
//     public float spawnRadius;
//     public float triggerDistance;
//     
//     public void Awake()
//     {
//         this.znetView = this.GetComponent<ZNetView>();
//         this.InvokeRepeating("UpdateSpawn", 2f, 2f);
//     }
//     
//     public void UpdateSpawn()
//     {
//         if (!this.znetView.IsOwner() || ZNetScene.instance.OutsideActiveArea(this.transform.position) || !Player.IsPlayerInRange(this.transform.position, this.triggerDistance))
//             return;
//         this.spawnTimer += 2f;
//         if ((double) this.spawnTimer <= (double) this.spawnInterval)
//             return;
//         this.spawnTimer = 0.0f;
//         this.SpawnOne();
//     }
//     
//     public bool SpawnOne()
//     {
//         int near;
//         int total;
//         this.GetInstances(out near, out total);
//         if (near >= this.maxNear)
//             return false;
//         
//         Vector3 point;
//         if (spawnPrefab == null || !this.FindSpawnPoint(spawnPrefab, out point))
//             return false;
//         GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(spawnPrefab, point, Quaternion.Euler(0.0f, (float) UnityEngine.Random.Range(0, 360), 0.0f));
//         Character component1 = gameObject.GetComponent<Character>();
//         
//         
//         // this.m_spawnEffects.Create(component1.GetCenterPoint(), Quaternion.identity);
//         return true;
//     }
//     
//     public bool FindSpawnPoint(GameObject prefab, out Vector3 point)
//     {
//         prefab.GetComponent<BaseAI>();
//         for (int index = 0; index < 10; ++index)
//         {
//             Vector3 p = this.transform.position + Quaternion.Euler(0.0f, (float) UnityEngine.Random.Range(0, 360), 0.0f) * Vector3.forward * UnityEngine.Random.Range(0.0f, this.spawnRadius);
//             float height;
//             if (ZoneSystem.instance.FindFloor(p, out height) && (!this.onGroundOnly || !ZoneSystem.instance.IsBlocked(p)))
//             {
//                 p.y = height + 0.1f;
//                 point = p;
//                 return true;
//             }
//         }
//         point = Vector3.zero;
//         return false;
//     }
//     
//     public void GetInstances(out int near)
//     {
//         near = 0;
//         Vector3 position = this.transform.position;
//         foreach (BaseAI baseAiInstance in BaseAI.BaseAIInstances)
//         {
//             if (this.IsSpawnPrefab(baseAiInstance.gameObject))
//             {
//                 double num = (double)  DistanceXZ(baseAiInstance.transform.position, position);
//                 if (num < (double) this.nearRadius)
//                     ++near;
//             }
//         }
//     }
//     
//     public bool IsSpawnPrefab(GameObject gameobject)
//     {
//         
//         if (gameobject.name.CustomStartsWith(spawnPrefab.name))
//             return true;
//         
//         return false;
//     }
//     
//     public float DistanceXZ(Vector3 v0, Vector3 v1)
//     {
//         double num1 = (double) v1.x - (double) v0.x;
//         float num2 = v1.z - v0.z;
//         return Mathf.Sqrt((float) (num1 * num1 + (double) num2 * (double) num2));
//     }
// }