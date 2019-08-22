using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoNS
{
  public abstract class BaseController : MonoBehaviour
  {

    public abstract void UpdateChild();

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() { }
    BaseController childController;
    protected HexMap hexMap;
    public virtual void PreGameInit(HexMap hexMap, BaseController childController)
    {
      this.hexMap = hexMap;
      this.childController = childController;
      updateReady = false;
    }

    public virtual void PostGameInit()
    {
      updateReady = true;
    }

    bool updateReady = false;

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
      if (updateReady)
      {
        childController.UpdateChild();
      }
    }

  }

}