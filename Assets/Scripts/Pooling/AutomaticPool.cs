﻿// Copyright 2019 The Gamedev Guru (http://thegamedev.guru)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TheGamedevGuru
{
public class AutomaticPool : MonoBehaviour
{
  [SerializeField] private int initialElementCount = 2;
  [SerializeField] private AssetReference assetReferenceToInstantiate = null;

  private Stack<GameObject> _availableObjectsPool = null;
  private Stack<GameObject> _allObjects = null;
  private AsyncOperationHandle<GameObject> _prefabAsyncOperationHandle;

  public GameObject Take(Transform parent = null)
  {
    var isReady = _prefabAsyncOperationHandle.IsValid() && _prefabAsyncOperationHandle.IsDone;
    Assert.IsTrue(isReady, "Pool is not ready, my friend");
    if (_availableObjectsPool.Count == 0)
    {
      AddNewElement(_prefabAsyncOperationHandle.Result);
    }

    var newGameObject = _availableObjectsPool.Pop();
    newGameObject.transform.SetParent(parent, false);
    newGameObject.SetActive(true);
    return newGameObject;
  }

  public void Return(GameObject gameObjectToReturn)
  {
    Assert.IsTrue(_prefabAsyncOperationHandle.IsValid());
    gameObjectToReturn.SetActive(false);
    gameObjectToReturn.transform.parent = transform;
    _availableObjectsPool.Push(gameObjectToReturn);
  }

  void OnEnable()
  {
    Assert.IsNotNull(assetReferenceToInstantiate, "Prefab to instantiate must be non-null");
    SetupPool();
  }

  void OnDisable()
  {
    foreach (var obj in _allObjects)
    {
      Destroy(obj);
    }

    _availableObjectsPool = null;
    _allObjects = null;
    Addressables.Release(_prefabAsyncOperationHandle);
  }

  private void SetupPool()
  {
    _availableObjectsPool = new Stack<GameObject>(initialElementCount);
    _allObjects = new Stack<GameObject>(initialElementCount);
    _prefabAsyncOperationHandle = assetReferenceToInstantiate.LoadAssetAsync<GameObject>();
    _prefabAsyncOperationHandle.Completed += handle =>
    {
      var prefab = _prefabAsyncOperationHandle.Result;
      for (var i = 0; i < initialElementCount; i++)
      {
        AddNewElement(prefab);
      }
    };
  }

  private void AddNewElement(GameObject prefab)
  {
    var newGameObject = Instantiate(prefab, transform);
    _availableObjectsPool.Push(newGameObject);
    _allObjects.Push(newGameObject);
    newGameObject.SetActive(false);
  }
}
}