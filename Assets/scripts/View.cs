using FieldNS;
using MapTileNS;
using UnityEngine;

public abstract class View: MonoBehaviour {
  public bool Animating = false;
  public bool viewActivated = true;
  public abstract void OnCreate(DataModel dataModel);
}

public abstract class Building: DataModel {
  public bool isSiegeWall = false;
  public delegate void OnBuildingReady(Building building);
  public event OnBuildingReady onBuildingReady;
  public State state = State.constructing;
  public enum State
  {
    constructing,
    normal
  }
  public Tile baseTile;
  protected int buildWork = 0;
  protected MonoNS.HexMap hexMap;
  protected MonoNS.SettlementMgr settlementMgr;
  protected virtual void TurnEndCB() {}
  public WarParty owner;

  public int buildTurns {
    get {
      if (buildWork <= 0) return 0;
      int canBeDone = HowMuchBuildWorkToFinish();
      if (canBeDone == 0) return -1;
      if (canBeDone >= buildWork) return 1;
      int remaining  = buildWork % HowMuchBuildWorkToFinish();
      int turns  = (buildWork - remaining) / canBeDone;
      return turns + (remaining > 0 ? 1 : 0);
    }
  }

  protected abstract int HowMuchBuildWorkToFinish();
  public bool TurnEnd()
  {
    bool ret = false;;
    if (state == State.constructing) {
      buildWork -= HowMuchBuildWorkToFinish();
    }

    if (buildWork < 1)
    {
      buildWork = 0;
      state = State.normal;
      onBuildingReady(this);
      ret = true;
    }
    TurnEndCB();
    return ret;
  }
}

public interface DataModel {}