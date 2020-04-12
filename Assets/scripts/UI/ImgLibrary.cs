using System.Collections.Generic;
using System.IO;
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
      LoadTexture2Sprite();
    }

    public override void UpdateChild() {}

    public Sprite generalPortait1;
    public Sprite generalPortait2;
    public Sprite defaultPortrait1;
    public Sprite defaultPortrait2;
    Queue<Sprite> availablePortraits = new Queue<Sprite>();
    Dictionary<General, Sprite> lib = new Dictionary<General, Sprite>();

    public Sprite GetGeneralPortrait(General general) {
      if (!lib.ContainsKey(general)) {
        Sprite sprite = null;
        if (general.commandUnit.onFieldUnit.IsCommander()) {
          if (general.faction == Cons.Song) {
            sprite = generalPortait1;
          } else {
            sprite = generalPortait2;
          }
        } else {
          if (availablePortraits.Count > 0) {
            sprite = availablePortraits.Dequeue();
          } else {
            sprite = general.faction.IsAI() ? defaultPortrait1 : defaultPortrait2;
          }
        }
        lib[general] = sprite;
      }
      return lib[general];
    }

    private byte[] getImageByte(string imagePath) {
      FileStream files = new FileStream(imagePath, FileMode.Open);
      byte[] imgByte = new byte[files.Length];
      files.Read(imgByte, 0, imgByte.Length);
      files.Close();
      return imgByte;
    }

    private List<string>  GetImagePath() {
      List<string> filePaths = new List<string>();
      foreach(string imgType in new string[]{"*.jpg", "*.png"}) {
        // unity root path(Assets folder): Application.dataPath
        foreach(string dir in Directory.GetFiles(Application.dataPath + @"/Imgs/portraits", imgType)) {
          filePaths.Add(dir);
        }
      }
      return filePaths;
    }

    private void LoadTexture2Sprite() {
      foreach (string filePath in GetImagePath()) {
        Texture2D t2d = new Texture2D(1920, 1080);
        t2d.LoadImage(getImageByte(filePath));
        availablePortraits.Enqueue(Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero));
      }
    }

  }


}
