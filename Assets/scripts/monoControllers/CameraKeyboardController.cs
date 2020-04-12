using UnityEngine;

namespace MonoNS
{
  public class CameraKeyboardController : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      hexMap.eventDialog.eventDialogOn += EnableCamera;
      hexMap.eventDialog.eventDialogOff += DisableCamera;
    }

    float moveSpeed = 20;
    bool allowUserCtrl = true;

    public void DisableCamera() {
      allowUserCtrl = false;
    }

    public void EnableCamera() {
      allowUserCtrl = true;
    }

    Vector3 fixedPosition;
    public bool fixingCamera = false;
    public void FixCameraAt(Vector3 p) {
      fixedPosition = new Vector3(p.x, 10, p.z-4f);
      fixingCamera = true;
    }

    public override void UpdateChild()
    {
      if (fixingCamera) {
        if (Vector3.Distance(this.transform.position, fixedPosition) < 0.1f) {
          fixingCamera = false;
        } else {
          transform.position = Vector3.Lerp(transform.position, fixedPosition, 4 * Time.deltaTime);
        }
      } else {
        if (!allowUserCtrl) return;
        Vector3 translate = new Vector3(
          Input.GetAxis("Horizontal"),
          0,
          Input.GetAxis("Vertical")
        );
        this.transform.Translate(translate * moveSpeed * Time.deltaTime, Space.World);
      }
    }
  }

}