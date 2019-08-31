using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using FieldNS;

namespace MonoNS
{
  public class HexMap : MonoBehaviour
  {
    // all controllers
    public TurnController turnController;
    public WeatherGenerator weatherGenerator;
    public WindGenerator windGenerator;
    public SettlementMgr settlementMgr;
    public MsgBox msgBox;
    public ActionController actionController;
    public MouseController mouseController;
    public CombatController combatController;
    public CameraKeyboardController cameraKeyboardController;
    public UnitSelectionPanel unitSelectionPanel;
    public SettlementViewPanel settlementViewPanel;
    public WeatherIndicator weatherIndicator;
    public HoverInfo hoverInfo;
    public TurnIndicator turnIndicator;
    public TurnPhaseTitle turnPhaseTitle;
    public InputField inputField;

    // ==============================================================
    // ================= Interfaces required from hex map plugin ====
    // ==============================================================
    public Hex GetHex(int x, int y)
    {
      if (tiles == null)
      {
        Debug.LogError("not initiated");
        throw new UnityException("map not initiated yet");
      }
      if (x < 0 || y < 0 || x > numCols - 1 || y > numRows - 1) return null;
      return (Hex)(tiles[x, y]);
    }

    // ==============================================================
    // ==============================================================
    public enum RangeType
    {
      movement,
      attack,
      camp,
      supplyRange
    }
    public int numRows = 20;
    public int numCols = 40;
    public Material MatOcean;
    public Material MatPlain;
    public Material MatLessPlain;
    public Material MatGrassland;
    public Material MatMountain;
    public Material MatBurning;
    public Material MatSchorched;
    public Material MatFlooded;
    public Material MovementRange;
    public Material AttackRange;
    public Material CampRange;
    public Material SupplyRange;
    public Material PlayerSkin;
    public Material GreySkin;
    public Material AISkin;
    public Material UnitHightlight;
    public Material OverLayMat;
    public Material TransMat;
    public Material WarningMat;
    [System.NonSerialized] public float HeightMountain = 0.33f, HeightHill = 0.28f, HeightFlat = 0f;
    public GameObject InfantryPrefab;
    public GameObject CavalryPrefab;
    public GameObject HexPrefab;
    public GameObject HighGroundPrefab;
    public GameObject MountainRootPrefab;
    public GameObject MountainPeakPrefab;
    public GameObject MountainPrefab;
    public GameObject TentPrefab;
    public GameObject CampPrefab;
    public WarParty[] warParties = new WarParty[2];
    public List<GameObject> lineCache = new List<GameObject>();

    Tile[,] tiles;

    //bool updateReady = false;
    Tile[] highlightedArea;
    LineRenderer lineRenderer;
    Dictionary<Tile, GameObject> tile2GO;
    Dictionary<GameObject, Tile> GO2Tile;
    Dictionary<Unit, GameObject> unit2GO = new Dictionary<Unit, GameObject>();

    public Tile GetTile(int x, int y)
    {
      return (Tile)GetHex(x, y);
    }

    // Use this for initialization
    public void PreGameInit()
    {
      turnController = GameObject.FindObjectOfType<TurnController>();
      weatherGenerator = GameObject.FindObjectOfType<WeatherGenerator>();
      windGenerator = GameObject.FindObjectOfType<WindGenerator>();
      settlementMgr = GameObject.FindObjectOfType<SettlementMgr>();
      msgBox = GameObject.FindObjectOfType<MsgBox>();
      actionController = GameObject.FindObjectOfType<ActionController>();
      mouseController = GameObject.FindObjectOfType<MouseController>();
      combatController = GameObject.FindObjectOfType<CombatController>();
      cameraKeyboardController = GameObject.FindObjectOfType<CameraKeyboardController>();
      unitSelectionPanel = GameObject.FindObjectOfType<UnitSelectionPanel>();
      settlementViewPanel = GameObject.FindObjectOfType<SettlementViewPanel>();
      weatherIndicator = GameObject.FindObjectOfType<WeatherIndicator>();
      hoverInfo = GameObject.FindObjectOfType<HoverInfo>();
      turnIndicator = GameObject.FindObjectOfType<TurnIndicator>();
      turnPhaseTitle = GameObject.FindObjectOfType<TurnPhaseTitle>();
      inputField = GameObject.FindObjectOfType<InputField>();
      // init actionBroker
      UnitActionBroker actionBroker = UnitActionBroker.GetBroker();
      actionBroker.onUnitAction += OnUnitAction;

      GenerateMap();
      lineRenderer = transform.GetComponentInChildren<LineRenderer>();
      lineRenderer.startWidth = 0.1f;
      lineRenderer.endWidth = 0.1f;
      //updateReady = true;
    }

    public void PostGameInit()
    {
      foreach (Tile h in tiles)
      {
        h.PostCreation();
      }
    }

    public void SetWarParties(WarParty defender, WarParty invader) {
      warParties[0] = invader.isAI ? defender : invader;
      warParties[1] = invader.isAI ? invader : defender;
    }

    public WarParty GetPlayerParty() {
      return warParties[0];
    }

    public WarParty GetAIParty() {
      return warParties[1];
    }

    public bool IsInEnemyScoutRange(Unit unit, Tile tile, bool ignoreConcealed = false) {
      HashSet<Tile> enemyScoutArea = new HashSet<Tile>();
      WarParty party = unit.IsAI() ? GetPlayerParty() : GetAIParty();
      foreach (Unit u in party.GetUnits())
      {
        foreach(Tile t in u.GetScoutArea()) {
          if (u.IsConcealed() && ignoreConcealed) {
            continue;
          }
          enemyScoutArea.Add(t);
        }
      }
      return enemyScoutArea.Contains(tile);
    }

    public void CreateLine(Tile[] path)
    {
      if (path.Length == 0) return;
      GameObject myLine = new GameObject();
      lineCache.Add(myLine);
      myLine.transform.position = tile2GO[path[0]].transform.position + (Vector3.up * 0.5f);
      myLine.AddComponent<LineRenderer>();
      LineRenderer lr = myLine.GetComponent<LineRenderer>();
      lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
      //lr.SetWidth(0.1f, 0.1f);
      lr.startWidth = 0.1f;
      lr.endWidth = 0.1f;
      Vector3[] ps = new Vector3[path.Length];
      for (int i = 0; i < path.Length; i++)
      {
        GameObject GO = tile2GO[path[i]];
        ps[i] = GO.transform.position + (Vector3.up * 0.5f);
      }
      lr.positionCount = path.Length;
      lr.SetPositions(ps);
    }

    public void CleanLines()
    {
      foreach (GameObject line in lineCache)
      {
        GameObject.Destroy(line);
      }
      lineCache = new List<GameObject>();
    }

    public void DehighlightPath()
    {
      lineRenderer.enabled = false;
    }
    public void HighlightPath(Tile[] tiles)
    {
      lineRenderer.enabled = true;
      Vector3[] ps = new Vector3[tiles.Length];
      for (int i = 0; i < tiles.Length; i++)
      {
        GameObject GO = tile2GO[tiles[i]];
        ps[i] = GO.transform.position + (Vector3.up * 0.5f);
      }
      lineRenderer.positionCount = tiles.Length;
      lineRenderer.SetPositions(ps);
    }

    public void UpdateTileVisuals()
    {
      for (int x = 0; x < numCols; x++)
      {
        for (int y = 0; y < numRows; y++)
        {
          Tile tile = tiles[x, y];
          SetTileVisual(tile);
        }
      }
      StaticBatchingUtility.Combine(this.gameObject);
    }

    public void RerenderTileTxt() {
      for (int x = 0; x < numCols; x++)
      {
        for (int y = 0; y < numRows; y++)
        {
          Tile tile = tiles[x, y];
          UpdateTileTextAndSkin(tile);
        }
      }
    }

    private void SetTroopSkin(GameObject view, Material skin) {
      MeshRenderer[] mrs = view.GetComponentsInChildren<MeshRenderer>();
      foreach (MeshRenderer mr in mrs) {
        mr.material = skin;
      }
    }

    public void SetUnitSkin(Unit unit)
    {
      if (unit == null) return;
      if (!unit2GO.ContainsKey(unit))
      {
        // unit has encamped
        return;
      }
      GameObject view = unit2GO[unit];
      if (unit.TurnDone())
      {
        SetTroopSkin(view, GreySkin);
      }
      else
      {
        SetTroopSkin(view, 
        unit.IsAI() ? AISkin : PlayerSkin);
      }
    }

    public void OnUnitAction(Unit unit, ActionType type, Tile tile) {
      if (type == ActionType.UnitLeft || type == ActionType.UnitDestroyed) {
        DestroyUnitView(unit);
      }

      if (type == ActionType.UnitHidden) {
        DeactivateUnitView(unit);
      }

      if (type == ActionType.UnitVisible) {
        if (!ActivateUnitView(unit)) {
          if (tile != null) CreateUnitViewAt(unit, tile);
        }
      }
    }
    
    public void HighlightUnit(Unit unit)
    {
      if (unit == null && unit2GO.ContainsKey(unit)) return;
      SetTroopSkin(unit2GO[unit], UnitHightlight);
    }

    void SetTileVisual(Tile tile)
    {
      GameObject prefab = null;
      FieldType fieldType = FieldType.Wild;
      float elevation = tile.getElevation();
      if (elevation >= HeightMountain)
      {
        if (Cons.SlimChance()) {
          prefab = MountainPeakPrefab;
        } else if (Cons.FairChance()) {
          prefab = MountainPrefab;
        } else {
          prefab = MountainRootPrefab;
        }
        fieldType = FieldType.Wild;
        tile.SetTerrianType(TerrianType.Mountain);
        if(Cons.SlimChance()) {
          tile.burnable = true;
        }
      }
      else if (elevation >= HeightHill)
      {
        prefab = HighGroundPrefab;
        fieldType = Cons.MostLikely() ? FieldType.Wild : FieldType.Farm;
        tile.SetTerrianType(TerrianType.Hill);
        if(fieldType == FieldType.Wild && Cons.SlimChance()) {
          tile.burnable = true;
        }
      }
      else if (elevation >= HeightFlat)
      {
        prefab = HexPrefab;
        tile.SetTerrianType(TerrianType.Plain);
        fieldType = Cons.MostLikely() ? FieldType.Farm : FieldType.Wild;
      }
      else
      {
        prefab = HexPrefab;
        tile.SetTerrianType(TerrianType.Water);
        fieldType = FieldType.Wild;
      }
      GameObject tileGO = (GameObject)Instantiate(prefab, tile.Position(), Quaternion.identity, this.transform);
      tile2GO[tile] = tileGO;
      tileGO.name = tile.GetCoord().x + "," + tile.GetCoord().y;
      GO2Tile[tileGO] = tile;
      tile.SetFieldType(fieldType);
      UpdateTileTextAndSkin(tile);
    }

    void SetTileMat(Tile tile)
    {
      Material mat = null;
      // MeshFilter points to the model
      //MeshFilter mf = tileGO.GetComponentInChildren<MeshFilter>();
      //mf.mesh = MeshFlat;
      if (tile.field == FieldType.Burning)
      {
        mat = MatBurning;
      } else if (tile.field == FieldType.Settlement)
      {
        mat = MatGrassland;
      }
      else if (tile.field == FieldType.Schorched)
      {
        mat = MatSchorched;
      }
      else if (tile.field == FieldType.Flooding)
      {
        mat = MatOcean;
      }
      else if (tile.field == FieldType.Flooded)
      {
        mat = MatFlooded;
      }
      else if (tile.terrian == TerrianType.Mountain)
      {
        mat = MatMountain;
      }
      else if (tile.field == FieldType.Farm)
      {
        mat = MatGrassland;
      }
      else if (tile.terrian != TerrianType.Water && tile.field == FieldType.Wild)
      {
        mat = MatPlain;
      }
      else
      {
        mat = MatOcean;
      }
      GameObject tileGO = tile2GO[tile];
      MeshRenderer[] mrs = tileGO.GetComponentsInChildren<MeshRenderer>();
      foreach(MeshRenderer mr in mrs) {
        if (mr.name == "HexCoordLabel" || mr.name == "overlay") {
          continue;
        }
        mr.material = mat;
      }
    }

    public void UpdateTileTextAndSkin(Tile tile)
    {
      GameObject tileGO = tile2GO[tile];
      //string txt = tile.Q + "," + tile.R + "\n";
      //txt = txt + (tile.isHighGround ? "High Ground" : "Flat Ground") + "\n";
      //txt = txt + tile.GetFieldType() + "\n";
      string txt = "";
      if (tile.terrian == TerrianType.Water) {
        if (tile.isDam)
        {
          txt = txt + "Dam\n";
        }
      } else if (tile.terrian != TerrianType.Mountain) {
        txt = txt + (tile.CanSetFire() && tile.burnable ? " Fire" : "");
        if (settlementMgr.IsCampable(tile)) {
          txt = txt + " Camp\n";
        }
      }
      Color fontColor;
      if (tile.field == FieldType.Burning || tile.field == FieldType.Schorched) {
        fontColor = Color.white;
      } else if (tile.field == FieldType.Flooded) {
        fontColor = Color.black;
      } else {
        fontColor = Color.white;
      }
      tileGO.GetComponentInChildren<TextMesh>().fontSize = 10;
      tileGO.GetComponentInChildren<TextMesh>().color = fontColor;
      tileGO.GetComponentInChildren<TextMesh>().text = txt;
      SetTileMat(tile);
    }

    public void SpawnUnit(Unit unit)
    {
      if (unit == null)
      {
        return;
      }
      if(unit.IsVisible() || unit.IsConcealed()) {
        CreateUnitViewAt(unit, unit.tile);
        if (unit.IsConcealed() && unit.IsAI()) DeactivateUnitView(unit);
      }
    }

    public UnitView GetUnitView(Unit unit)
    {
      if (unit == null || unit2GO == null || !unit2GO.ContainsKey(unit))
      {
        return null;
      }
      return unit2GO[unit].GetComponent<UnitView>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
    }

    void Overlay(Tile tile, Material mat) {
      GameObject tileGO = tile2GO[tile];
      MeshRenderer[] mrs = tileGO.GetComponentsInChildren<MeshRenderer>();
      foreach(MeshRenderer mr in mrs) {
        if (mr.name == "overlay") {
          mr.material = mat;
          break;
        }
      }
    }

    public void DehighlightArea()
    {
      if (highlightedArea == null) return;
      foreach (Tile tile in highlightedArea)
      {
        Overlay(tile, TransMat);
      }
      highlightedArea = null;
    }

    public void HighlightArea(Tile[] tiles, RangeType type, Unit unit = null)
    {
      DehighlightArea();
      foreach (Tile tile in tiles)
      {
        Material mat = null;
        if (type == RangeType.attack) mat = AttackRange;
        if (type == RangeType.movement) {
          mat = MovementRange;
          if (unit != null && unit.IsConcealed() && IsInEnemyScoutRange(unit, tile, true)) {
            mat = WarningMat;
          }
        }
        if (type == RangeType.camp) mat = CampRange;
        if (type == RangeType.supplyRange) mat = SupplyRange;
        Overlay(tile, mat);
      }
      highlightedArea = tiles;
    }

    public Tile GetTileFromGO(GameObject GO)
    {
      if (GO2Tile == null || !GO2Tile.ContainsKey(GO))
      {
        return null;
      }
      return GO2Tile[GO];
    }

    public GameObject GetTileGO(Tile tile)
    {
      if (tile2GO == null || !tile2GO.ContainsKey(tile))
      {
        return null;
      }
      return tile2GO[tile];
    }

    public void DestroyUnitView(Unit unit)
    {
      if (unit2GO.ContainsKey(unit))
      {
        GameObject view = unit2GO[unit];
        view.GetComponent<UnitView>().Destroy();
        Destroy(view);
        unit2GO.Remove(unit);
      }
    }

    public void DeactivateUnitView(Unit unit)
    {
      if (unit2GO.ContainsKey(unit))
      {
        GameObject view = unit2GO[unit];
        foreach(MeshRenderer mr in view.GetComponentsInChildren<MeshRenderer>()) {
          mr.enabled = false;
        }
        view.GetComponent<UnitView>().Deactivate();
      } else {
        CreateUnitViewAt(unit, unit.tile);
        DeactivateUnitView(unit);
      }
    }

    public bool ActivateUnitView(Unit unit)
    {
      if (unit2GO.ContainsKey(unit))
      {
        GameObject view = unit2GO[unit];
        foreach(MeshRenderer mr in view.GetComponentsInChildren<MeshRenderer>()) {
          mr.enabled = true;
        }
        view.GetComponent<UnitView>().Activate();
        return true;
      }
      return false;
    }

    public void CreateUnitViewAt(Unit unit, Tile tile)
    {
      if (unit2GO.ContainsKey(unit)) {
        return;
      }
      GameObject prefab = unit.IsCavalry() ? CavalryPrefab : InfantryPrefab;
      GameObject tileGO = tile2GO[tile];
      //NOTE: spawn as child gameobject of hex
      GameObject unitGO = (GameObject)Instantiate(prefab,
           tile.Position(),
           Quaternion.identity, tileGO.transform);
      UnitView view = unitGO.GetComponent<UnitView>();
      view.unit = unit;
      view.OnCreate();
      unit2GO[unit] = unitGO;
      SetUnitSkin(unit);
    }

    void GenerateMap()
    {
      tiles = new Tile[numCols, numRows];
      tile2GO = new Dictionary<Tile, GameObject>();
      GO2Tile = new Dictionary<GameObject, Tile>();

      for (int x = 0; x < numCols; x++)
      {
        for (int y = 0; y < numRows; y++)
        {
          Tile tile = new Tile(x, y, this);
          tile.setElevation(-0.3f);
          tiles[x, y] = tile;
          
          //hexGO.GetComponentInChildren<TextMesh>().text = x + "," + y;
        }
      }

      float noiseResolution = 0.3f;
      float noiseScale = 1.1f; //larger value, more islands
      Random.InitState(1);
      Vector2 noiseRandom = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
      for (float col = 0; col < numCols; col++)
      {
        for (float row = 0; row < numRows; row++)
        {
          Tile h = GetTile((int)col, (int)row);
          h.setElevation((Mathf.PerlinNoise(col / (float)numCols / noiseResolution + noiseRandom.x,
          row / (float)numRows / noiseResolution + noiseRandom.y) - 0.35f) * noiseScale);
        }
      }

      UpdateTileVisuals();
    }
  }
}