using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using UnityEngine;
using CourtNS;

namespace MapTileNS
{
  public class Tile : Hex, PFTile, DataModel
  {
    public const float PlainHeightStart = 0f;
    public const float PlainHeightEnd = 0.75f;
    public const float HillHeightStart = 0.5f;
    public const float HillHeightEnd = 1.0f;
    public const float HighgroundWatermark = 0.65f;
    //public const int Work2BuildCamp = 34;
    public const int Work2BuildCamp = 10;
    public const float HighGround = 0.312f;
    public const float VantageGround = 0.8f;
    public Flood flood = null;
    public WildFire wildFire = null;
    public Epidemic epidemic = null;
    public Poision poision = null;
    public bool waterBound = false;
    public bool burnable = false;
    public bool road = false;
    public bool vantagePoint = false;

    public Dictionary<Tile, Tile[]> roads = new Dictionary<Tile, Tile[]>();
    public Tile(int q, int r, HexMap hexMap) : base(q, r, hexMap) { }
    public void PostCreation()
    {
      weatherGenerator = hexMap.weatherGenerator;
      turnController = hexMap.turnController;
      windGenerator = hexMap.windGenerator;
      settlementMgr = hexMap.settlementMgr;
      eventDialog = hexMap.eventDialog;
      settlementAniController = hexMap.settlementAniController;
      if (terrian != TerrianType.Mountain) {
        flood = new Flood(this);
      }
      if (terrian != TerrianType.Water) {
        wildFire = new WildFire(this, burnable);
      }
      if (terrian == TerrianType.Water) {
        poision = new Poision(this);
      }
      if (field == FieldType.Wild && Util.eq<Region>(hexMap.warProvince.region, Cons.lowLand)) {
        epidemic = new Epidemic(this);
      }

      if (flood != null || wildFire != null || epidemic != null) {
        hexMap.weatherGenerator.tileCB.Add(this);
      }

      foreach (Tile tile in neighbours) {
        if (tile.terrian == TerrianType.Water) {
          waterBound = true;
          break;
        }
      }
    }

    private string _name = "unnamed";
    private int _storageLevel = 1;
    public string name {
      get {
        return _name;
      }
    } 

    public int storageLevel {
      get {
        return _storageLevel;
      }
    }

    public void SetAsCampField(string name, int storageLevel) {
      _name = name;
      _storageLevel = storageLevel;
    }

    public void BuildRoad() {
      road = true;
      SetFieldType(FieldType.Road);
      // TODO: view animation
    }

    public bool RepairRoad() {
      if (!road) {
        return false;
      }
      if (field == FieldType.Road) {
        // no need to repair
        return false;
      }
      // repair road
      return true;
    }
    
    public Vector3 GetSurfacePosition() {
      float y = 0f;
      if (terrian == TerrianType.Hill || terrian == TerrianType.Mountain) {
        y = HighGround;
      }
      if (vantagePoint) {
        y = VantageGround;
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
      if (field == FieldType.Village) prefix = "Village";
      if (field == FieldType.Road) prefix = "Road";
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
    public HashSet<Tile> SetFire()
    {
      if (wildFire != null) {
        return wildFire.Start();
      }
      return new HashSet<Tile>();
    }

    public void Burn() {
      if (wildFire != null) {
        wildFire.BurnTile();
      }
    }

    public bool IsFloodable() {
      if (flood != null) {
        return isDam && flood.Floodable();
      }
      return false;
    }

    public bool IsBurnable() {
      if (wildFire != null) {
        return wildFire.Burnable();
      }
      return false;
    }

    public HashSet<Tile> CreateFlood()
    {
      if (flood != null) {
        return flood.Start();
      }
      return new HashSet<Tile>();
    }

    public void Flood() {
      if (flood != null) {
        flood.FloodTile();
      }
    }

    public bool Poision(Unit unit) {
      if (poision == null) {
        return false;
      }
      poision.SetPoision(unit);
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
      // TODO remove this
      hexMap.GetTileView(this).RefreshVisual();
    }

    public bool Accessible() {
      return movementCost != Unit.MovementCostOnUnaccesible;
    }

    public bool Passable(bool isAI, bool forDeploy = false) {
      Unit u = GetUnit();
      bool deployable = u == null && Accessible();
      if (!deployable && u != null) {
        if (u.IsAI() == isAI) {
          if (!forDeploy) {
            deployable = true;
          }
        } else if(IsThereConcealedEnemy(isAI)) {
          deployable = true;
        }
      }
      return deployable;
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
        if (field == FieldType.Road) {
          movementCost = Unit.MovementcostOnHillRoad;
        }
      }
      else if (terrian == TerrianType.Plain)
      {
        movementCost = Unit.MovementcostOnPlain;
        if (field == FieldType.Road) {
          movementCost = Unit.MovementcostOnPlainRoad;
        }
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
        if (mode != Mode.Supply) {
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
                    1f : Cavalry.MovementCostModifierOnHill));
      }
      else
      {
        ret = movementCost;
      }

      if (vantagePoint) {
        ret = (int)(ret * 1.2f);
      }

      return ret > 0 ? ret : Unit.MovementCostOnUnaccesible;
    }

    public bool Deployable(Unit unit)
    {
      if (IsThereConcealedEnemy(unit.IsAI())) {
        GetUnit().DiscoveredByEnemy();
      }
      return Passable(unit.IsAI(), true);
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
        if (value != null) {
          SetFieldType(FieldType.Settlement);
        } else {
          SetFieldType(FieldType.Schorched);
        }
      }
    }
    Settlement _settlement;
    SettlementMgr settlementMgr;
    EventDialog eventDialog;
    SettlementAnimationController settlementAniController;
    public int Work2BuildSettlement()
    {
      if ((!vantagePoint) && (terrian == TerrianType.Plain || terrian == TerrianType.Hill) &&
        (field == FieldType.Wild || field == FieldType.Village || field == FieldType.Flooded
         || field == FieldType.Schorched))
      {
        return Work2BuildCamp;
      }
      return -1;
    }

    public void BuildDam()
    {
      SetTerrianType(TerrianType.Water);
      SetFieldType(FieldType.Wild);
      isDam = true;
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

    public Tile Escape() {
      Unit unit = GetUnit();
      foreach (Tile tile in neighbours)
      {
        if (tile.Deployable(unit))
        {
          return tile;
        }
      }
      return null;
    }

    // * PathFind interfaces *
    public PFTile[] GetNeighbourTiles()
    {
      return neighbours;
    }

    public int AggregateCostToEnter(int costSoFar, PFTile sourceTile, PFUnit unit, Mode mode)
    {
      return ((Unit)unit).AggregateCostToEnterTile(this, costSoFar, mode);
    }
  }

}