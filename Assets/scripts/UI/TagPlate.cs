using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class TagPlate : BaseController
  {

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      if (mainCamera == null) mainCamera = Camera.main;
      rectTransform = GetComponent<RectTransform>();
    }

    public GameObject myTarget;
    public Vector3 positionOffset = new Vector3(0, 30, 0);
    public Camera mainCamera;
    RectTransform rectTransform;

    // Update is called once per frame
    public override void UpdateChild()
    {
      // destroy self
      if (myTarget == null) { Destroy(gameObject); };
      Vector3 myPos = mainCamera.WorldToScreenPoint(myTarget.transform.position);
      rectTransform.anchoredPosition = myPos + positionOffset;
    }
  }

}