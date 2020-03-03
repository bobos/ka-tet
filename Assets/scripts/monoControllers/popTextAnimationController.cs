using System.Collections;
using UnitNS;
using UnityEngine;

namespace MonoNS
{
  public class PopTextAnimationController : BaseController
  {
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      cc = hexMap.cameraKeyboardController;
    }

    CameraKeyboardController cc;
    public bool Animating = false;

    public override void UpdateChild() {}

    public void Show(View view, string msg, Color color, bool noFix = false)
    {
      if (view == null || !view.viewActivated) { return; }
      Animating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoShow(view, msg, color, noFix));
    }

    IEnumerator CoShow(View view, string msg, Color color, bool noFix)
    {
      if (!noFix) {
        cc.FixCameraAt(view.transform.position);
      }
      while (cc.fixingCamera) { yield return null; }
      PopTextView textView = hexMap.ShowPopText(view, msg, color);
      while (textView.Animating) { yield return null; }
      Destroy(textView.gameObject);
      hexMap.cameraKeyboardController.EnableCamera();
      Animating = false;
    }
  }

}