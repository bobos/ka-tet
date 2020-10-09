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
    public int totalMaleDead = 0;
    public int totalFemaleDead = 0;
    public int totalChildDead = 0;
    public Settlement attackerRoot;
    public Settlement defenderRoot;
    public List<Settlement> allNodes = new List<Settlement>(); 

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      buildingQueue = new List<Building>();
      mouseController = hexMap.mouseController;
      popAniController = hexMap.popAniController;
      actionController = hexMap.actionController;
      cc = hexMap.cameraKeyboardController;
    }

    public Settlement GetRoot(bool isAI) {
      return hexMap.IsAttackSide(isAI) ? attackerRoot : defenderRoot;
    }

    public override void UpdateChild() {}

    public bool turnEndOngoing = false;
    public void TurnEnd(WarParty warParty) {
      turnEndOngoing = true;
      StartCoroutine(PerformTurnEnd(warParty));
    }

    public View GetView(Building settlement) {
      if (!settlement2GO.ContainsKey(settlement)) return null;
      return settlement2GO[settlement].GetComponent<View>();
    }

    TextLib textLib = Cons.GetTextLib();
    CameraKeyboardController cc;
    PopTextAnimationController popAniController;
    IEnumerator PerformTurnEnd(WarParty warParty)
    {
      foreach (Building settlement in buildingQueue.ToArray())
      {
        if (Util.eq<WarParty>(warParty, settlement.owner))
        {
          if (settlement.TurnEnd()) {
            popAniController.Show(GetView(settlement), textLib.get("pop_builded"), Color.green);
            while (popAniController.Animating) { yield return null; }
          }
        }
      }

      foreach(Settlement s in allNodes) {
        if (Util.eq<WarParty>(warParty, s.owner)) {
          s.TurnEndCB();
        }
      }
      
      turnEndOngoing = false;
    }

    public List<Tile> GetControlledTiles(bool isAI) {
      List<Tile> tiles = new List<Tile>();
      List<Settlement> nodes = new List<Settlement>();
      (hexMap.IsAttackSide(isAI) ? attackerRoot : defenderRoot).GetLinked(nodes);
      foreach(Settlement node in nodes) {
        foreach(Tile tile in node.myTiles) {
          if (!tiles.Contains(tile)) {
            tiles.Add(tile);
          }
        }
      }
      return tiles;
    }

    ActionController actionController;
    MouseController mouseController;
    List<Building> buildingQueue;

    public Settlement BuildCamp(string name, Tile location, WarParty warParty, int storageLvl) {
      return BuildSettlement(name, location, Settlement.Type.camp, warParty,
        0, 0, 0,
        storageLvl, 1, null);
    }

    public Settlement BuildStrategyBase(Tile location, WarParty warParty) {
      return BuildSettlement(
        System.String.Format(
          textLib.get("settlement_strategyBase"),
          warParty.faction.Name()
        ),
        location, Settlement.Type.strategyBase, warParty, 0, 0, 0, 3, 1, null);
    }

    public Settlement BuildCity(string name, Tile location,
      WarParty warParty,
      int wallLevel,
      int male, int female, int child, int storageLvl) {
      return BuildSettlement(
        name,
        location,
        Settlement.Type.city,
        warParty,
        male, female, child, storageLvl, wallLevel, null);
    }

    public bool BuildSiegeWall(Unit builder, WarParty warParty) {
      Tile location = builder.tile;
      if(location.Work2BuildSettlement() == -1) return false; // unbuildable tile
      SiegeWall siegeWall = new SiegeWall(location, warParty);
      siegeWall.onBuildingReady += this.SettlementReady;
      buildingQueue.Add(siegeWall);
      CreateSiegeWall(siegeWall);
      location.siegeWall = siegeWall;
      return true;
    }

    private Settlement BuildSettlement(
      string name, Tile location, Settlement.Type type, WarParty warParty,
      int male, int female, int child,
      int storageLevel,
      int wallLevel, Unit unit)
    {
      if(location.Work2BuildSettlement() == -1) return null; // unbuildable tile
      Settlement settlement = null;
      if (type == Settlement.Type.camp)
      {
        settlement = new Camp(name, location, warParty, storageLevel);
      }
      else
      {
        if (type == Settlement.Type.city)
        {
          settlement = new City(name, location, warParty, male, female,
            child, storageLevel, wallLevel);
        }
        if (type == Settlement.Type.strategyBase)
        {
          settlement = new StrategyBase(name, location, warParty);
        }
      }

      CreateSettlement(settlement);
      settlement.Encamp(unit);
      allNodes.Add(settlement);
      return settlement;
    }

    public void Add2Settlement(Tile tile) {
      Settlement settlement = null;
      float score = -1f;
      bool firstOne = true;
      foreach (Settlement node in allNodes) {
        float distance = Tile.Distance(node.baseTile, tile);
        if (firstOne) {
          firstOne = false;
          score = distance;
          settlement = node;
        } else if (distance < score) {
          score = distance;
          settlement = node;
        }
      }
      settlement.myTiles.Add(tile);
    }

    public Dictionary<Building, GameObject> settlement2GO = new Dictionary<Building, GameObject>();
    void CreateSettlement(Settlement settlement)
    {
      Vector3 position = settlement.baseTile.GetSurfacePosition();
      GameObject GO = (GameObject)Instantiate(settlement.type == Settlement.Type.camp ? hexMap.CampPrefab : hexMap.TentPrefab,
        position,
        Quaternion.identity,
        hexMap.GetTileView(settlement.baseTile).transform);
      SettlementView view = GO.GetComponent<SettlementView>();
      view.OnCreate(settlement);
      view.SetNameGO((GameObject)Instantiate(hexMap.NameTextPrefab,
        SettlementView.NamePosition(position),
        Quaternion.identity, hexMap.GetTileView(settlement.baseTile).transform));
      settlement2GO[settlement] = GO;
      SetBuildingSkinReady(GetView(settlement).gameObject);
    }

    void CreateSiegeWall(SiegeWall siegeWall)
    {
      Tile tile = siegeWall.baseTile;
      Vector3 position = tile.GetSurfacePosition();
      GameObject GO = (GameObject)Instantiate(hexMap.SiegeWallPrefab,
        position,
        Quaternion.identity,
        hexMap.GetTileView(tile).transform);
      SiegeWallView view = GO.GetComponent<SiegeWallView>();
      view.OnCreate(siegeWall);
      settlement2GO[siegeWall] = GO;
    }

    public void SettlementReady(Building settlement)
    {
      buildingQueue.Remove(settlement);
      SetBuildingSkinReady(GetView(settlement).gameObject);
      if (settlement.isSiegeWall) {
        Settlement s = null;
        foreach(Tile tile in settlement.baseTile.neighbours) {
          if (tile.settlement != null) {
            s = tile.settlement;
            break;
          }
        }
        if (s.IsUnderSiege()) {
          hexMap.eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.UnderSiege, null, s));
        }
      }
    }

    void SetBuildingSkinReady(GameObject view) {
      MeshRenderer[] mrs = view.GetComponentsInChildren<MeshRenderer>();
      foreach (MeshRenderer mr in mrs) {
        mr.material = hexMap.MatSchorched;
      }
    }

    public int[] OccupySettlement(Unit unit, Settlement settlement) {
      int[] deathNum = new int[]{0,0,0,0};
      settlement.owner = unit.IsAI() ? hexMap.GetAIParty() : hexMap.GetPlayerParty();
      settlement.Encamp(unit);
      int moraleIncr = settlement.type == Settlement.Type.camp ? 30 : 40;
      deathNum[3] = moraleIncr;
      foreach(Unit u in hexMap.GetWarParty(unit).GetUnits()) {
        u.morale += moraleIncr;
      }
      if (settlement.owner.attackside) {
        if (settlement.type == Settlement.Type.city) {
          int maleDead = unit.rf.soldiers * 2;
          int femaleDead = unit.rf.soldiers;
          int childDead = (int)(unit.rf.soldiers * 0.05f);
          maleDead = maleDead > settlement.civillian_male ? settlement.civillian_male : maleDead;
          femaleDead = femaleDead > settlement.civillian_female ? settlement.civillian_female : femaleDead;
          childDead = childDead > settlement.civillian_child ? settlement.civillian_child : childDead;

          settlement.civillian_male -= maleDead;
          settlement.civillian_female -= femaleDead;
          settlement.civillian_child -= childDead;

          totalMaleDead += maleDead;
          totalFemaleDead += femaleDead;
          totalChildDead += childDead;

          deathNum[0] = maleDead;
          deathNum[1] = femaleDead;
          deathNum[2] = childDead;
        }
      }
      return deathNum;
    }

    public void DestroySiegeWall(SiegeWall sw)
    {
      buildingQueue.Remove(sw);
    }

    public bool IsThereSiegedCity() {
      bool yes = false;
      foreach(Settlement s in allNodes) {
        if (!s.owner.attackside && s.type == Settlement.Type.city && s.IsUnderSiege()) {
          yes = true;
          break;
        }
      }
      return yes;
    }

    public static bool Ready4Refresh = false;
  }

}