using CourtNS;
using TextNS;
using UnitNS;
using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class ImgLibrary : BaseController
  {

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
    }

    public override void UpdateChild() {}

    public Sprite generalPortait1;
    public Sprite generalPortait2;
    public Sprite generalPortait3;
    public Sprite generalPortait4;

    public Sprite GetGeneralPortrait(General general) {
      if (general.commandUnit.onFieldUnit.IsCommander()) {
        if (general.faction == Cons.Song) {
          return generalPortait1;
        }
        return generalPortait2;
      }

      if (general.faction == Cons.Song) {
        return generalPortait3;
      }
      return generalPortait4;
    }

  }


}
