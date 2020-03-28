using System.Collections.Generic;
using MapTileNS;
using UnityEngine;
using MonoNS;
using System.Collections;
using BuildingNS;

public class SiegeWallView : View
{

  // Use this for initialization
  public SiegeWall siegeWall = null;
  public override void OnCreate(DataModel siegeWall)
  {
    this.siegeWall = (SiegeWall)siegeWall;
  }

  public void DestroyAnimation()
  {
    Animating = true;
    StartCoroutine(CoDestroyAnimation());
  }

  IEnumerator CoDestroyAnimation() {
    yield return new WaitForSeconds(1);
    Animating = false;
  }

  public void Destroy()
  {
    GameObject.Destroy(gameObject);
  }
}