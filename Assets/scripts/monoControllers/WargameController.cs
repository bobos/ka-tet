using System.Collections;
using System.Collections.Generic;
using MapTileNS;
using UnitNS;
using UnityEngine;

namespace MonoNS {
  public class WargameUnit {
    public Tile tile;
    public Unit unit;
    public Tile[] path;
    public int remainingMovement;
    public Tile originTile;
    public WargameUnit(Tile tile, Unit unit, Tile[] path, Tile originTile, int remainingMovement) {
      this.tile = tile;
      this.unit = unit;
      this.path = path;
      this.remainingMovement = remainingMovement;
      this.originTile = originTile;
    }
  }

  public class WargameController : BaseController {
    List<WargameUnit> unitList;
    public bool start = false;
    public GameObject WargameBtn;
    public GameObject CommitBtn;
    public GameObject CancelBtn;
    public HashSet<Tile> visibleArea;
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      WargameBtn.SetActive(hexMap.deployDone);
      CommitBtn.SetActive(false);
      CancelBtn.SetActive(false);
    }

    public void StartWargame() {
      start = true;
      WargameBtn.SetActive(false);
      CommitBtn.SetActive(true);
      CancelBtn.SetActive(true);
      unitList = new List<WargameUnit>();
      visibleArea = FieldNS.FoW.Get().GetVisibleArea();
    }

    void Quit() {
      start = false;
      WargameBtn.SetActive(true);
      CommitBtn.SetActive(false);
      CancelBtn.SetActive(false);
      hexMap.CleanLines();
    }

    public bool CommitAnimating = false;
    public void Commit() {
      CommitAnimating = true;
      StartCoroutine(CoCommit());
    }

    IEnumerator CoCommit() {
      if (hexMap.turnController.playerTurn) {
        hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_wargame_commiting"), Color.green);
        while (hexMap.turnController.showingTitle) { yield  return null; }
      }
      Cancel();
      unitList.Reverse();
      foreach(WargameUnit u in unitList) {
        u.unit.SetPath(u.path);
        hexMap.actionController.move(u.unit);
        while (hexMap.actionController.ActionOngoing) { yield  return null; }
      }
      if (hexMap.turnController.playerTurn) {
        hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_wargame_committed"), Color.white);
        while (hexMap.turnController.showingTitle) { yield  return null; }
      }
      CommitAnimating = false;
    }

    public void Cancel() {
      Quit();
      // Cancel all unit in reverse order
      unitList.Reverse();
      foreach(WargameUnit u in unitList) {
        u.unit.movementRemaining = u.remainingMovement;
        u.unit.SetWargameTile(u.originTile);
      }
    }

    public bool IsWargameUnit(Unit unit) {
      if (!start) {
        return false;
      }

      bool found = false;
      foreach(WargameUnit u in unitList) {
        if (Util.eq<Unit>(u.unit, unit)) {
          found = true;
          break;
        }
      }

      return found;
    }

    public override void UpdateChild() {}

    public void Add(Tile tile, Unit unit, Tile[] path) {
      Tile originTile = unit.tile;
//      Tile[] path = unit.FindPath(tile);
      int originMovement = unit.movementRemaining;
      // always assume the target tile is within range
      unit.movementRemaining -= unit.GetPathCost(path);
      unit.SetWargameTile(tile);
      unitList.Add(new WargameUnit(tile, unit, path, originTile, originMovement));
    }

  }

}