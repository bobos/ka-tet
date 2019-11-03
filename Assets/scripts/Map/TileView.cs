﻿using MapTileNS;
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

  public void RefreshVisual() {
    GameObject tileGO = gameObject;
    string txt = "";
    if (tile.terrian == TerrianType.Water) {
      if (tile.isDam)
      {
        txt = txt + "Dam\n";
      }
    } else if (tile.terrian != TerrianType.Mountain) {
      txt = txt + (tile.burnable ? " Fire" : "");
      if (settlementMgr.IsCampable(tile)) {
        txt = txt + " Camp\n";
      }
    }
    if (tile.field == FieldType.Village) {
      txt = txt + " Village";
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
    else if (tile.field == FieldType.Village)
    {
      mat = hexMap.MatGrassland;
    }
    else if (tile.field == FieldType.Road)
    {
      mat = hexMap.MatLessPlain;
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
      if (mr.name == "HexCoordLabel" || mr.name == "overlay") {
        continue;
      }
      mr.material = mat;
    }

  }

  public void FloodAnimation()
  {
    Animating = true;
    StartCoroutine(CoFloodAnimation());
  }

  IEnumerator CoFloodAnimation() {
    yield return new WaitForSeconds(1);
    Animating = false;
  }

  public void BurnAnimation()
  {
    Animating = true;
    StartCoroutine(CoBurnAnimation());
  }

  IEnumerator CoBurnAnimation() {
    yield return new WaitForSeconds(1);
    Animating = false;
  }
}
