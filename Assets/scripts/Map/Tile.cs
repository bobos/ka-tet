﻿using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using UnityEngine;

namespace MapTileNS
{
  public class Tile : Hex, PFTile
  {
    public const float PlainHeightStart = 0f;
    public const float PlainHeightEnd = 0.75f;
    public const float HillHeightStart = 0.5f;
    public const float HillHeightEnd = 1.0f;
    public const float HighgroundWatermark = 0.65f;
    //public const int Work2BuildCamp = 34;
    public const int Work2BuildCamp = 10;
    public const float HighGround = 0.312f;
    public Flood flood = null;
    public WildFire wildFire = null;
    public LandSlide landSlide = null;
    public HeatSickness heatSickness = null;
    public Dehydration dehydration = null;
    public Drowning drowning = null;
    public Poision poision = null;
    public bool waterBound = false;
    public bool burnable = false;

    public Tile(int q, int r, HexMap hexMap) : base(q, r, hexMap) { }
    public void PostCreation()
    {
      weatherGenerator = hexMap.weatherGenerator;
      turnController = hexMap.turnController;
      windGenerator = hexMap.windGenerator;
      settlementMgr = hexMap.settlementMgr;
      if (terrian != TerrianType.Mountain) {
        flood = new Flood(this);
      }
      if (terrian != TerrianType.Water) {
        wildFire = new WildFire(this, burnable);
      }
      if (terrian == TerrianType.Water) {
        poision = new Poision(this);
      }
      if (field == FieldType.Wild) {
        heatSickness = new HeatSickness(this);
      }
      if (terrian == TerrianType.Hill) {
        landSlide = new LandSlide(this);
      }

      foreach (Tile tile in neighbours) {
        if (tile.terrian == TerrianType.Water) {
          drowning = new Drowning(this);
          waterBound = true;
          break;
        }
      }

      if (!waterBound) {
        dehydration = new Dehydration(this);
      }
    }
    
    public Vector3 GetSurfacePosition() {
      float y = 0f;
      if (terrian == TerrianType.Hill || terrian == TerrianType.Mountain) {
        y = HighGround;
      }
      return new Vector3(Position().x, y, Position().z);
    }
    // ==============================================================
    // ================= Information ================================
    // ==============================================================
    // TODO: textLib
    Tile[] _neighbours;
    public Tile[] neighbours {
      get {
        if (_neighbours == null) {
          _neighbours = Neighbours<Tile>();
        }
        return _neighbours;
      }
      private set {}
    }

    public string GetFieldType()
    {
      string prefix = "";
      if (field == FieldType.Wild) prefix = "Wild";
      if (field == FieldType.Farm) prefix = "Farmed";
      if (field == FieldType.Settlement) prefix = "Settelment On";
      if (field == FieldType.Schorched) prefix = "Schorched";
      if (field == FieldType.Burning) prefix = "Burning";
      if (field == FieldType.Flooded) prefix = "Flooded";
      if (field == FieldType.Flooding) prefix = "Flooding";

      string suffix = "";
      if (terrian == TerrianType.Hill) suffix = "Hills";
      if (terrian == TerrianType.Mountain) suffix = "Mountain";
      if (terrian == TerrianType.Plain) suffix = "Plain";
      if (terrian == TerrianType.Water) suffix = "Water";

      return prefix + " " + suffix;
    }

    // ==============================================================
    // ================= Listeners ==================================
    // ==============================================================
    public WindGenerator windGenerator;
    public WeatherGenerator weatherGenerator;
    public TurnController turnController;

    public void ListenOnHeavyRain(WeatherGenerator.OnWeather onHeavyRain)
    {
      weatherGenerator.onHeavyRain -= onHeavyRain;
      weatherGenerator.onHeavyRain += onHeavyRain;
    }

    public void RemoveOnHeavyRainListener(WeatherGenerator.OnWeather onHeavyRain)
    {
      weatherGenerator.onHeavyRain -= onHeavyRain;
    }

    public void ListenOnRain(WeatherGenerator.OnWeather onRain)
    {
      weatherGenerator.onRain -= onRain;
      weatherGenerator.onRain += onRain;
    }

    public void RemoveOnRainListener(WeatherGenerator.OnWeather onRain)
    {
      weatherGenerator.onRain -= onRain;
    }

    public void ListenOnHeat(WeatherGenerator.OnWeather onHeat)
    {
      weatherGenerator.onHeat -= onHeat;
      weatherGenerator.onHeat += onHeat;
    }

    public void RemoveOnHeatListener(WeatherGenerator.OnWeather onHeat)
    {
      weatherGenerator.onHeat -= onHeat;
    }

    public void ListenOnDry(WeatherGenerator.OnWeather onDry)
    {
      weatherGenerator.onDry -= onDry;
      weatherGenerator.onDry += onDry;
    }

    public void RemoveOnDryListener(WeatherGenerator.OnWeather onDry)
    {
      weatherGenerator.onDry -= onDry;
    }

    public void ListenOnSeason(WeatherGenerator.OnSeasonChange onSeasonChange)
    {
      weatherGenerator.onSeasonChange -= onSeasonChange;
      weatherGenerator.onSeasonChange += onSeasonChange;
    }

    public void RemoveOnSeasonListener(WeatherGenerator.OnSeasonChange onSeasonChange)
    {
      weatherGenerator.onSeasonChange -= onSeasonChange;
    }

    public void ListenOnTurnEnd(TurnController.OnNewTurn onNewTurn)
    {
      turnController.onNewTurn -= onNewTurn;
      turnController.onNewTurn += onNewTurn;
    }

    public void RemoveTurnEndListener(TurnController.OnNewTurn onNewTurn)
    {
      turnController.onNewTurn -= onNewTurn;
    }

    // ==============================================================
    // ================= Disasters ==================================
    // ==============================================================
    public bool SetFire()
    {
      if (wildFire != null) {
        return wildFire.Start();
      }
      return false;
    }

    public bool SabotageDam()
    {
      if (flood != null) {
        return flood.Start();
      }
      return false;
    }

    public bool Poision() {
      if (poision == null) {
        return false;
      }
      poision.SetPoision();
      return true;
    }

    // ==============================================================
    // ================= Terrian ====================================
    // ==============================================================
    public TerrianType terrian;
    public FieldType field;
    public bool isDam = false;

    int movementCost = Unit.BasicMovementCost;
    public void SetFieldType(FieldType type)
    {
      field = type;
      if (type == FieldType.Burning) {
        burnable = false;
      }
      if (type == FieldType.Settlement || type == FieldType.Burning || type == FieldType.Flooding)
      {
        movementCost = Unit.MovementCostOnUnaccesible;
      }
      else
      {
        UpdateMovementcost();
      }

      hexMap.UpdateTileTextAndSkin(this);
    }

    public bool Ambushable()
    {
      return terrian == TerrianType.Hill && field == FieldType.Wild;
    }

     // TODO
    public int GetVantagePoint()
    {
      return 0;
    }

    public void SetTerrianType(TerrianType type)
    {
      terrian = type;
      UpdateMovementcost();
    }

    private void UpdateMovementcost()
    {
      if (terrian == TerrianType.Hill)
      {
        movementCost = Unit.MovementcostOnHill;
      }
      else if (terrian == TerrianType.Plain)
      {
        movementCost = Unit.MovementcostOnPlain;
      }
      else
      {
        movementCost = Unit.MovementCostOnUnaccesible;
      }
    }

    public override int GetCost(Unit unit, Mode mode)
    {
      //if (settlement != null && settlement.owner.isAI == unit.IsAI()
      //  && settlement.state == Settlement.State.normal)
      //{
      //  // settlement is passible
      //  return Unit.MovementcostOnPlain;
      //}
      Unit u = GetUnit();
      if (u != null)
      {
        if (mode == Mode.Supply) {
          // for supply routine, should ignore same side units and ignore enemy's side hidden units
          if (u.IsAI() != unit.IsAI() && !u.IsConcealed()) {
            return Unit.MovementCostOnUnaccesible;
          }
        } else {
          // normal path find and access range
          if (u.IsAI() == unit.IsAI()) {
            return Unit.MovementCostOnUnaccesible;
          } else {
            if (!u.IsConcealed()) {
              return Unit.MovementCostOnUnaccesible;
            }
          }
        }
      }

      int ret;
      if (unit.IsCavalry())
      {
        // apply movement modifier for calvary unit
        ret = (int)(movementCost * (terrian == TerrianType.Plain ?
                    Cavalry.MovementCostModifierOnPlain : Cavalry.MovementCostModifierOnHill));
      }
      else
      {
        ret = movementCost;
      }

      return ret > 0 ? ret : Unit.MovementCostOnUnaccesible;
    }

    public bool Deployable(Unit unit)
    {
      Unit u = GetUnit();
      bool deployable = u == null && movementCost != Unit.MovementCostOnUnaccesible;
      if (!deployable && IsThereConcealedEnemy(unit.IsAI())) {
        u.DiscoveredByEnemy();
      }
      return deployable;
    }

    public bool DeployableForPathFind(Unit unit) {
      return (GetUnit() == null || IsThereConcealedEnemy(unit.IsAI())) && movementCost != Unit.MovementCostOnUnaccesible;
    }

    public bool IsThereConcealedEnemy(bool isAI) {
      Unit u = GetUnit();
      return u != null && u.IsAI() != isAI && u.IsConcealed();
    }

    // ==============================================================
    // ================= Settlement =================================
    // ==============================================================
    public HashSet<Tile> linkedTilesForCamp = new HashSet<Tile>();

    public Settlement settlement
    {
      get
      {
        return _settlement;
      }
      set
      {
        _settlement = value;
        SetFieldType(FieldType.Settlement);
      }
    }
    Settlement _settlement;
    SettlementMgr settlementMgr;
    public int Work2BuildSettlement()
    {
      if ((terrian == TerrianType.Plain || terrian == TerrianType.Hill) &&
        (field == FieldType.Wild || field == FieldType.Farm || field == FieldType.Flooded
         || field == FieldType.Schorched))
      {
        return Work2BuildCamp;
      }
      return -1;
    }

    public void RemoveCamp(bool setFire)
    {
      _settlement = null;
      if (setFire) SetFire();
    }
    
    public void BuildDam()
    {
      SetTerrianType(TerrianType.Water);
      SetFieldType(FieldType.Wild);
      isDam = true;
      flood.BuildDam();
    }
    
    // ==============================================================
    // ================= Units ======================================
    // ==============================================================
    HashSet<Unit> units;
    public void AddUnit(Unit u)
    {
      if (units == null)
      {
        units = new HashSet<Unit>();
      }
      units.Add(u);
    }

    public bool RemoveUnit(Unit u)
    {
      if (units == null || !units.Contains(u))
      {
        return false;
      }
      return units.Remove(u);
    }

    public Unit GetUnit()
    {
      if (units == null || units.Count == 0)
      {
        return null;
      }
      return units.First();
    }

    public void DisasterAffectUnit(DisasterType type)
    {
      if (field == FieldType.Settlement && settlement != null)
      {
        settlementMgr.DestroyCamp(settlement, BuildingNS.DestroyType.ByFire, false);
      }
      Unit u = GetUnit();
      if (u != null)
      {
        Tile retreatTile = null;
        foreach (Tile tile in Neighbours<Tile>())
        {
          if (tile.Deployable(u))
          {
            retreatTile = tile;
            break;
          }
        }
        if (retreatTile == null)
        {
          u.Destroy(DestroyType.ByWildFire);
        }
        else
        {
          DisasterEffect.Apply(type, u);
          u.SetTile(retreatTile);
        }
      }
    }

    // * PathFind interfaces *
    public PFTile[] GetNeighbourTiles()
    {
      List<PFTile> ret = new List<PFTile>();
      foreach (Tile h in Neighbours<Tile>())
      {
        ret.Add(h);
      }
      return ret.ToArray();
    }

    public int AggregateCostToEnter(int costSoFar, PFTile sourceTile, PFUnit unit, Mode mode)
    {
      return ((Unit)unit).AggregateCostToEnterTile(this, costSoFar, mode);
    }
  }

}