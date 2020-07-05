using MapTileNS;
using UnityEngine;
using MonoNS;
using System.Collections;

public class TileView : View
{

  // Use this for initialization
  HexMap hexMap;
  SettlementMgr settlementMgr;
  public Tile tile;
  public Settlement settlement = null;
  /// <summary>
  /// Start is called on the frame when a script is enabled just before
  /// any of the Update methods is called the first time.
  /// </summary>
  public override void OnCreate(DataModel tile)
  {
    hexMap = GameObject.FindObjectOfType<HexMap>();
    settlementMgr = hexMap.settlementMgr;
    this.tile = (Tile)tile;
  }

  public void RefreshVisual(bool enemyZone = false) {
    GameObject tileGO = gameObject;
    string txt = "";
    if (hexMap.AttackerZone.Contains(tile) || hexMap.DefenderZone.Contains(tile)) {
      txt += "☒\n";
    }
    if (enemyZone) {
      txt += "⊘\n";
    }
    if (tile.deadZone != null && tile.deadZone.DecompositionCntDown > 0) {
      txt += "☠☠☠☠\n";
    }
    if (tile.vantagePoint) {
      txt += "hi\n";
    }
    if (tile.terrian == TerrianType.Water) {
      if (tile.isDam)
      {
        txt = txt + "Dam\n";
      }
    }
    txt = txt + (tile.burnable ? " Fr" : "");
    if (tile.field == FieldType.Forest) {
      txt = txt + " Fst";
    }
    Color fontColor;
    if (tile.field == FieldType.Burning || tile.field == FieldType.Schorched) {
      fontColor = Color.white;
    } else {
      fontColor = Color.black;
    }
    tileGO.GetComponentInChildren<TextMesh>().color = fontColor;
    tileGO.GetComponentInChildren<TextMesh>().text = txt;

    Material mat = null;
    // MeshFilter points to the model
    //MeshFilter mf = tileGO.GetComponentInChildren<MeshFilter>();
    //mf.mesh = MeshFlat;
    if (tile.field == FieldType.Burning)
    {
      mat = hexMap.MatBurning;
    } else if (tile.field == FieldType.Settlement)
    {
      mat = hexMap.MatGrassland;
    }
    else if (tile.field == FieldType.Schorched)
    {
      mat = hexMap.MatSchorched;
    }
    else if (tile.field == FieldType.Flooding)
    {
      mat = hexMap.MatOcean;
    }
    else if (tile.field == FieldType.Flooded)
    {
      mat = hexMap.MatFlooded;
    }
    else if (tile.terrian == TerrianType.Mountain)
    {
      mat = hexMap.MatMountain;
    }
    else if (tile.field == FieldType.Forest)
    {
      mat = hexMap.MatGrassland;
    }
    else if (tile.terrian != TerrianType.Water && tile.field == FieldType.Wild)
    {
      mat = hexMap.MatPlain;
    }
    else
    {
      mat = hexMap.MatOcean;
    }
    MeshRenderer[] mrs = tileGO.GetComponentsInChildren<MeshRenderer>();
    foreach(MeshRenderer mr in mrs) {
      if (mr.name == "HexModel" || mr.name.StartsWith("Quad")) {
        mr.material = mat;
      }
    }
  }

  public void FloodAnimation()
  {
    Animating = true;
    StartCoroutine(CoFloodAnimation());
  }

  IEnumerator CoFloodAnimation() {
    yield return new WaitForSeconds(0.1f);
    Animating = false;
  }

  public void BurnAnimation()
  {
    Animating = true;
    StartCoroutine(CoBurnAnimation());
  }

  IEnumerator CoBurnAnimation() {
    yield return new WaitForSeconds(0.2f);
    Animating = false;
  }
}
