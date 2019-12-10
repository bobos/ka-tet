﻿using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using FieldNS;
using CourtNS;

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
    public UnitActionBroker actionBroker;
    public InputField inputField;
    public EventDialog eventDialog;
    public SettlementAnimationController settlementAniController;
    public UnitAnimationController unitAniController;
    public TileAnimationController tileAniController;
    public PopTextAnimationController popAniController;
    public EventStasher eventStasher;
    public Province warProvince;

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
      supplyRange,
      PoisionRange
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
    public Material PoisionRange;
    public Material PlayerSkin;
    public Material GreySkin;
    public Material AISkin;
    public Material UnitHightlight;
    public Material OverLayMat;
    public Material TransMat;
    public Material WarningMat;
    public Material SupplyRouteMat;
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
    public GameObject NameTextPrefab;
    public GameObject UnitInfoPrefab;
    public GameObject PopTextPrefab;
    public WarParty[] warParties = new WarParty[2];
    public List<GameObject> lineCache = new List<GameObject>();

    public Tile[,] tiles;

    //bool updateReady = false;
    Tile[] highlightedArea;
    LineRenderer lineRenderer;
    Dictionary<Tile, GameObject> tile2GO;
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
      eventDialog = GameObject.FindObjectOfType<EventDialog>();
      settlementAniController = GameObject.FindObjectOfType<SettlementAnimationController>();
      unitAniController = GameObject.FindObjectOfType<UnitAnimationController>();
      tileAniController = GameObject.FindObjectOfType<TileAnimationController>();
      popAniController = GameObject.FindObjectOfType<PopTextAnimationController>();
      eventStasher = GameObject.FindObjectOfType<EventStasher>();
      // init actionBroker
      actionBroker = UnitActionBroker.GetBroker();
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

    public int FoodPerTenMenPerTurn(bool isAI) {
      return IsAttackSide(isAI) ? 2 : 1;
    }

    public bool IsAttackSide(bool isAI) {
      WarParty wp = isAI ? GetAIParty() : GetPlayerParty();
      return wp.attackside;
    }

    public bool IsInEnemyScoutRange(bool isAI, Tile tile, bool ignoreConcealed = false) {
      HashSet<Tile> enemyScoutArea = new HashSet<Tile>();
      WarParty party = isAI ? GetPlayerParty() : GetAIParty();
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
          CreateTileGO(tile);
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
          GetTileView(tile).RefreshVisual();
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
        return;
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

    void CreateTileGO(Tile tile)
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
        fieldType = Cons.MostLikely() ? FieldType.Wild : (Cons.TinyChance() ? FieldType.Village : FieldType.Wild);
        tile.SetTerrianType(TerrianType.Hill);
        if(fieldType == FieldType.Wild && Cons.SlimChance()) {
          tile.burnable = true;
        }
      }
      else if (elevation >= HeightFlat)
      {
        prefab = HexPrefab;
        tile.SetTerrianType(TerrianType.Plain);
        fieldType = Cons.FairChance() ? FieldType.Wild : (Cons.TinyChance() ? FieldType.Village : FieldType.Wild);
      }
      else
      {
        prefab = HexPrefab;
        tile.SetTerrianType(TerrianType.Water);
        fieldType = FieldType.Wild;
      }
      GameObject tileGO = (GameObject)Instantiate(prefab, tile.Position(), Quaternion.identity, this.transform);
      View view = tileGO.GetComponentInChildren<TileView>();
      view.OnCreate(tile);
      tile2GO[tile] = tileGO;
      tileGO.name = tile.GetCoord().x + "," + tile.GetCoord().y;
      tile.SetFieldType(fieldType);
    }

    public TileView GetTileView(Tile tile) {
      if (tile == null || tile2GO == null || !tile2GO.ContainsKey(tile))
      {
        return null;
      }
      return tile2GO[tile].GetComponent<TileView>();
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

    public void OverlayFoW(Tile tile) {
      Unit unit = tile.GetUnit();
      if (unit != null && !unit.IsConcealed()) {
        DeactivateUnitView(unit);
      }
      Overlay(tile, OverLayMat);
    }

    public void OverlaySupplyLine(Tile tile) {
      Overlay(tile, SupplyRouteMat);
    }

    public bool IsOverlayFoW(Tile tile) {
      GameObject tileGO = tile2GO[tile];
      MeshRenderer[] mrs = tileGO.GetComponentsInChildren<MeshRenderer>();
      foreach(MeshRenderer mr in mrs) {
        if (mr.name == "overlay") {
          if(mr.material.name == "OverlayMat (Instance)") {
            return true;
          }
          return false;
        }
      }
      return false;
    }

    public void OverlayDisable(Tile tile) {
      Unit unit = tile.GetUnit();
      if (unit != null && !unit.IsConcealed()) {
        ActivateUnitView(unit);
      }
      Overlay(tile, TransMat);
    }

    public void DehighlightArea()
    {
      if (highlightedArea == null) return;
      foreach (Tile tile in highlightedArea)
      {
        OverlayDisable(tile);
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
          if (unit != null && unit.IsConcealed() && IsInEnemyScoutRange(unit.IsAI(), tile, true)) {
            mat = WarningMat;
          }
        }
        if (type == RangeType.camp) mat = CampRange;
        if (type == RangeType.supplyRange) mat = SupplyRange;
        if (type == RangeType.PoisionRange) mat = PoisionRange;
        Overlay(tile, mat);
      }
      highlightedArea = tiles;
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
      Vector3 position = tile.GetSurfacePosition();
      Vector3 namePosition = UnitView.NamePosition(position);
      Vector3 unitInfoPosition = UnitView.UnitInfoPosition(position);
      GameObject unitGO = (GameObject)Instantiate(prefab,
          position,
          Quaternion.identity, tileGO.transform);
      GameObject nameGO = (GameObject)Instantiate(NameTextPrefab,
          namePosition,
          Quaternion.identity, tileGO.transform);
      UnitView view = unitGO.GetComponent<UnitView>();
      view.nameGO = nameGO;

      GameObject unitInfoGO = (GameObject)Instantiate(UnitInfoPrefab,
          unitInfoPosition,
          Quaternion.identity, tileGO.transform);
      unitInfoGO.GetComponent<UnitInfoView>().SetName(unit.rf);
      view.unitInfoGO = unitInfoGO;

      view.OnCreate(unit);
      unit2GO[unit] = unitGO;
      unit.UpdateGeneralName();
      SetUnitSkin(unit);
    }

    UnitView GetView(Unit unit) {
      if (!unit2GO.ContainsKey(unit)) {
        return null;
      }
      return unit2GO[unit].GetComponent<UnitView>();
    }

    public PopTextView ShowPopText(View view, string msg, Color color) {
      Vector3 p = view.transform.position;
      GameObject popGO = (GameObject)Instantiate(PopTextPrefab,
          new Vector3(p.x - 0.5f, p.y, p.z),
          Quaternion.identity, view.transform);
      PopTextView textView = popGO.GetComponent<PopTextView>();
      textView.Show(view, msg, color);
      return textView;
    }

    void GenerateMap()
    {
      tiles = new Tile[numCols, numRows];
      tile2GO = new Dictionary<Tile, GameObject>();

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