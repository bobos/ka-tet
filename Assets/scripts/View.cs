using UnitNS;
using UnityEngine;

public abstract class View: MonoBehaviour {
  public bool Animating = false;
  public bool viewActivated = true;
  public abstract void OnCreate(DataModel dataModel);
}

public interface DataModel {}