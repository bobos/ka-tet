using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnitNS;
using MapTileNS;
using FieldNS;
using System.Collections;
using TextNS;

namespace MonoNS
{
  public class SettlementMgr : BaseController
  {
    public enum QueueJobType {
      DistSupply,
      DistLabor
    }

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      attackerRoots = new List<Settlement>();
      defenderRoots = new List<Settlement>();
      buildingQueue = new List<Settlement>();
      mouseController = hexMap.mouseController;
      actionController = hexMap.actionController;
      actionController.onBtnClick += OnBtnClick;
      cc = hexMap.cameraKeyboardController;
      ghostUnit = GhostUnit.createGhostUnit();
    }

    public override void UpdateChild() {}

    HashSet<Tile> campableTiles;

    public void SetCampableField(HashSet<Tile> tiles) {
      campableTiles = tiles;
    }

    public bool IsCampable(Tile tile) {
      if (campableTiles == null) return false;
      return campableTiles.Contains(tile);
    } 

    // TODO: just for tmp test
    public void BuildRoad(Tile from, Tile to) {
      List<Tile> path = new List<Tile>(); 
      foreach(Tile tile in ghostUnit.FindPath(from, to, true)) {
        if (!Util.eq<Tile>(tile, from) && !Util.eq<Tile>(tile, to)) {
          tile.BuildRoad();
          path.Add(tile);
        }
      }
      Tile[] road = path.ToArray();
      from.roads[to] = road;
      to.roads[from] = road;
    }

    class DistJob {
      public Settlement from;
      public Settlement to;
      public int amount;
      public QueueJobType type;

      public DistJob(Settlement from, Settlement to, int amount, QueueJobType type) {
        this.from = from;
        this.to = to;
        this.amount = amount;
        this.type = type;
      }
    }

    List<DistJob> attackerDistJobs = new List<DistJob>();
    List<DistJob> defenderDistJobs = new List<DistJob>();
    public void AddDistJob(Settlement from, Settlement to, int amount, QueueJobType type) {
      DistJob job = new DistJob(from, to, amount, type);
      if (from.owner.attackside) {
        attackerDistJobs.Add(job);
      } else {
        defenderDistJobs.Add(job);
      }
    }

    public bool turnEndOngoing = false;
    public void TurnEnd(WarParty warParty) {
      turnEndOngoing = true;
      StartCoroutine(PerformTurnEnd(warParty));
    }

    public SettlementView GetView(Settlement settlement) {
      if (!settlement2GO.ContainsKey(settlement)) return null;
      return settlement2GO[settlement].GetComponent<SettlementView>();
    }

    TextLib textLib = Cons.GetTextLib();
    CameraKeyboardController cc;
    IEnumerator PerformTurnEnd(WarParty warParty)
    {
      foreach (Settlement settlement in buildingQueue.ToArray())
      {
        if (Util.eq<WarParty>(warParty, settlement.owner))
        {
          if (settlement.TurnEnd()) {
            SettlementView view = GetView(settlement);
            cc.FixCameraAt(view.transform.position);
            while (cc.fixingCamera) { yield return null; }
            view.Animating = true;
            hexMap.ShowPopText(view, textLib.get("pop_builded"), Color.green);
            while (view.Animating)
            {
              yield return null;
            }
          }
        }
      }

      List<Settlement> roots = warParty.attackside ? attackerRoots : defenderRoots;
      HashSet<Unit> units = warParty.GetUnits();
      foreach (Settlement root in roots)
      {
        if (root.state != Settlement.State.constructing) {
          SettlementView view = GetView(root);
          List<List<Unit>> failedUnits = root.ReduceSupply();
          foreach(Unit u in failedUnits[0]) {
            cc.FixCameraAt(view.transform.position);
            while (cc.fixingCamera) { yield return null; }

            view.Animating = true;
            hexMap.ShowPopText(view, 
              System.String.Format(textLib.get("pop_failedToSupplyUnitInSettlement"), u.GeneralName()),
              Color.yellow);
            while (view.Animating)
            {
              yield return null;
            }
          }

          foreach(Unit u in failedUnits[1]) {
            View unitView = hexMap.GetUnitView(u);
            cc.FixCameraAt(unitView.transform.position);
            while (cc.fixingCamera) { yield return null; }

            unitView.Animating = true;
            unitView.textView = hexMap.ShowPopText(unitView, 
              textLib.get("pop_failedToSupplyUnitNearby"),
              Color.yellow);
            while (unitView.Animating)
            {
              yield return null;
            }
          }
        }
        root.availableLabor = root.labor;
      }

      List<DistJob> jobs = warParty.attackside ? attackerDistJobs : defenderDistJobs;
      foreach(DistJob job in jobs) {
        View view = GetView(job.from);
        if (!job.from.IsFunctional() || !job.to.IsFunctional()) {
          cc.FixCameraAt(view.transform.position);
          while (cc.fixingCamera) { yield return null; }

          view.Animating = true;
          hexMap.ShowPopText(view,
            textLib.get(
              job.type == QueueJobType.DistSupply ?
              "pop_failedToDistSupply" : "pop_failedToDistLabor"),
            Color.red);
          while (view.Animating)
          {
            yield return null;
          }
          continue;
        }
        // ambush supply caravans
        if(!job.from.GetReachableSettlements().Contains(job.to)) {
          hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.SupplyRouteBlocked, null, job.from, 0, 0, 0, 0, 0, job.to));
          continue;
        }
        Tile[] route = job.from.baseTile.roads[job.to.baseTile];
        Unit ambusher = IsSupplyRouteAmbushed(route, units.First().IsAI());
        if (ambusher != null) {
          // There is enemy unit ambushed on the supply route
          if (job.type == QueueJobType.DistSupply) {
            job.from.SupplyIntercepted(ambusher, job.to, job.amount);
          } else {
            job.from.LaborIntercepted(ambusher, job.to, job.amount);
          }
          continue;
        }

        if (job.type == QueueJobType.DistSupply) {
          job.from.DistSupply(job.amount, job.to);
        } else {
          job.from.DistLabor(job.amount, job.to);
        }
      }

      if (warParty.attackside) {
        attackerDistJobs = new List<DistJob>();
      } else {
        defenderDistJobs = new List<DistJob>();
      }

      turnEndOngoing = false;
    }

    public Unit IsSupplyRouteAmbushed(Tile[] route, bool isAI) {
      foreach (Tile tile in route) {
        if (tile.IsThereConcealedEnemy(isAI)) {
          return tile.GetUnit();
        }
      }
      return null;
    }

    public void OnBtnClick(ActionController.actionName name)
    {
      if (name == ActionController.actionName.DECAMP)
      {
        Settlement settlement = mouseController.selectedSettlement;
        if (settlement == null || settlement.garrison.ToArray().Length == 0) return;
        settlement.Decamp(settlement.garrison[0]);
      }
    }

    ActionController actionController;
    MouseController mouseController;
    GhostUnit ghostUnit;
    List<Settlement> attackerRoots;
    List<Settlement> defenderRoots;
    List<Settlement> buildingQueue;
    public bool BuildSettlement(string name, Tile location, Settlement.Type type, WarParty warParty,
      int civillian, int labor, int supply = 0, Unit unit = null)
    {
      if(location.Work2BuildSettlement() == -1) return false; // unbuildable tile
      Settlement settlement = null;
      if (type == Settlement.Type.camp)
      {
        settlement = new Camp(unit.rf.general.Name() + Cons.textLib.get("b_ownedCamp"), location, warParty, supply, labor);
        settlement.onSettlementReady += this.SettlementReady;
        buildingQueue.Add(settlement);
      }
      else
      {
        if (type == Settlement.Type.city)
        {
          settlement = new City(name, location, warParty, supply, civillian, labor);
        }
        if (type == Settlement.Type.strategyBase)
        {
          settlement = new StrategyBase(name, location, warParty, supply, labor);
        }
      }

      if (warParty.attackside)
      {
        attackerRoots.Add(settlement);
      }
      else
      {
        defenderRoots.Add(settlement);
      }

      CreateSettlement(settlement);
      settlement.Encamp(unit);

      if (type != Settlement.Type.camp) SettlementReady(settlement);
      return true;
    }

    Dictionary<Settlement, GameObject> settlement2GO = new Dictionary<Settlement, GameObject>();
    public void CreateSettlement(Settlement settlement)
    {
      GameObject tileGO = hexMap.GetTileGO(settlement.baseTile);
      if (tileGO == null) Util.Throw("CreateSettlement: Tile doesn't exist!");
      GameObject GO = (GameObject)Instantiate(settlement.type == Settlement.Type.camp ? hexMap.CampPrefab : hexMap.TentPrefab,
        settlement.baseTile.GetSurfacePosition(),
        Quaternion.identity,
        tileGO.transform);
      GO.GetComponent<SettlementView>().settlement = settlement;
      settlement2GO[settlement] = GO;
    }

    public void SettlementReady(Settlement settlement)
    {
      buildingQueue.Remove(settlement);
    }

    public HashSet<Tile> GetSupplyRangeTiles(Settlement settlement) {
      SetGhostOwner(settlement);
      HashSet<Tile> ret = new HashSet<Tile>();
      if (!settlement.IsNormal()) {
        ret.Add(settlement.baseTile);
        return ret;
      }
      foreach(Tile tile in ghostUnit.GetAccessibleTilesForSupply(settlement.baseTile,
                            GetGhostUnitRangeForSettlementSupply())) {
        ret.Add(tile);
      }
      return ret;
    }

    public Tile[] GetCampableFields() {
      List<Tile> tiles = new List<Tile>();
      foreach(Tile tile in campableTiles) {
        if (tile.Work2BuildSettlement() != -1) {
          tiles.Add(tile);
        }
      }
      return tiles.ToArray();
    }

    public void DestroyCamp(Settlement camp, BuildingNS.DestroyType type, bool setFire = true)
    {
      buildingQueue.Remove(camp);
      if (camp.owner.attackside)
      {
        attackerRoots.Remove(camp);
      }
      else
      {
        defenderRoots.Remove(camp);
      }
      GameObject view = settlement2GO[camp];
      view.GetComponent<SettlementView>().Destroy();
      GameObject.Destroy(view);
      camp.baseTile.RemoveCamp(setFire);
      camp.Destroy(type);
    }

    public void GetVisibleArea(bool attackSide, HashSet<Tile> tiles) {
      foreach(Settlement settlement in attackSide ? attackerRoots : defenderRoots) {
        foreach(Tile tile in settlement.GetVisibleArea()) {
          tiles.Add(tile);
        }
      }
    }

    int GetGhostUnitRangeForSettlementSupply()
    {
      return (int)(ghostUnit.GetFullMovement() * 2);
    }

    void SetGhostOwner(Settlement settlement) {
      ghostUnit.SetPlayer(!settlement.owner.isAI);
    }

    public static bool Ready4Refresh = false;
  }

}