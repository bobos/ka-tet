using System.Collections;
using UnitNS;
using System.Collections.Generic;
using MapTileNS;
using TextNS;
using UnityEngine;
using NatureNS;
using CourtNS;

namespace MonoNS
{
  public class TileAnimationController : BaseController
  {
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      settlementAniController = hexMap.settlementAniController;
      unitAniController = hexMap.unitAniController;
      popAniController = hexMap.popAniController;
      eventDialog = hexMap.eventDialog;
    }

    SettlementAnimationController settlementAniController;
    UnitAnimationController unitAniController;
    PopTextAnimationController popAniController;
    EventDialog eventDialog;
    TextLib textLib = Cons.GetTextLib();
    public override void UpdateChild() {}

    public bool DestroySiegeAnimating = false;
    public void DestroySiegeWall(Unit unit, Tile tile = null)
    {
      DestroySiegeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoDestroySiegeWall(unit, tile));
    }

    IEnumerator CoDestroySiegeWall(Unit u, Tile tile)
    {
      tile = tile == null ? u.tile : tile;
      settlementAniController.DestroySiegeWall(tile.siegeWall);
      while (settlementAniController.Animating) { yield return null; }
      popAniController.Show(hexMap.GetTileView(tile), textLib.get("pop_siegeBreak"), Color.green);
      while (popAniController.Animating) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      DestroySiegeAnimating = false;
    }

    public bool FloodAnimating = false;
    public bool Flood(Unit unit, Tile tile, HashSet<Tile> tiles = null)
    {
      if (unit != null && unit.IsCavalry()) {
        return false;
      }
      FloodAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoFlood(unit, tile, tiles == null ? tile.CreateFlood() : tiles));
      return true;
    }

    IEnumerator CoFlood(Unit u, Tile t, HashSet<Tile> tiles)
    {
      popAniController.Show(hexMap.GetTileView(t), textLib.get("pop_damBroken"), Color.white);
      while (popAniController.Animating) { yield return null; }
      foreach(Tile tile in tiles) {
        hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(tile).transform.position);
        Unit unit = tile.GetUnit();
        tile.Flood();
        TileView view = hexMap.GetTileView(tile);
        view.FloodAnimation();
        while (view.Animating) { yield return null; }
        if (tile.siegeWall != null) {
          DestroySiegeWall(null, tile);
          while (DestroySiegeAnimating) { yield return null; }
        }

        if (unit != null) {
          Tile newTile = tile.Escape();
          if (newTile == null) {
            unitAniController.DestroyUnit(unit, DestroyType.ByFlood);
            while (unitAniController.DestroyAnimating) { yield return null; }
          } else {
            unitAniController.ShowEffects(unit, DisasterEffect.Apply(DisasterType.Flood, unit));
            while(unitAniController.ShowAnimating) { yield return null; }
            unitAniController.MoveUnit(unit, newTile);
            while (unitAniController.MoveAnimating) { yield return null; }
            if (!Util.eq<Tile>(newTile, unit.tile)) {
              // Failed to move, destroy unit
              unitAniController.DestroyUnit(unit, DestroyType.ByFlood);
              while (unitAniController.DestroyAnimating) { yield return null; }
            }
          }
        }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      FloodAnimating = false;
    }

    public bool BurnAnimating = false;
    public bool Burn(Tile tile, HashSet<Tile> tiles = null)
    {
      hexMap.cameraKeyboardController.DisableCamera();
      tiles = tiles == null ? tile.SetFire() : tiles;
      if (tiles.Count == 0) {
        return false;
      }
      BurnAnimating = true;
      StartCoroutine(CoBurn(tile, tiles));
      return true;
    }

    IEnumerator CoBurn(Tile t, HashSet<Tile> tiles)
    {
      popAniController.Show(hexMap.GetTileView(t), textLib.get("pop_setFire"), Color.yellow);
      while (popAniController.Animating) { yield return null; }
      foreach(Tile tile in tiles) {
        hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(tile).transform.position);
        Unit unit = tile.GetUnit();
        tile.Burn();
        TileView view = hexMap.GetTileView(tile);
        view.BurnAnimation();
        while (view.Animating) { yield return null; }
        if (tile.siegeWall != null) {
          DestroySiegeWall(null, tile);
          while (DestroySiegeAnimating) { yield return null; }
        }

        if (unit != null) {
          Tile newTile = tile.Escape();
          if (newTile == null) {
            unitAniController.DestroyUnit(unit, DestroyType.ByWildFire);
            while (unitAniController.DestroyAnimating) { yield return null; }
          } else {
            unitAniController.ShowEffects(unit, DisasterEffect.Apply(DisasterType.WildFire, unit));
            while(unitAniController.ShowAnimating) { yield return null; }
            unitAniController.MoveUnit(unit, newTile);
            while (unitAniController.MoveAnimating) { yield return null; }
            if (!Util.eq<Tile>(newTile, unit.tile)) {
              // Failed to move, destroy unit
              unitAniController.DestroyUnit(unit, DestroyType.ByWildFire);
              while (unitAniController.DestroyAnimating) { yield return null; }
            }
          }
        }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      BurnAnimating = false;
    }

    public bool WeatherAnimating = false;
    public void WeatherChange(Tile tile, Weather weather)
    {
      WeatherAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoWeatherChange(tile, weather));
    }

    IEnumerator CoWeatherChange(Tile tile, Weather weather) {
      if(tile.epidemic != null && tile.epidemic.OnWeatherChange(weather)) {
        // epidemic triggered
        Unit unit = tile.GetUnit();
        if (!unit.IsHeatSicknessAffected() &&
         (
          Util.eq<Province>(unit.rf.province, Cons.heBei) ||
          Util.eq<Province>(unit.rf.province, Cons.heDong)
        )) {
          eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Epidemic, unit, null));
          while (eventDialog.Animating) { yield return null; }
          unitAniController.Riot(unit, unit.epidemic.Occur());
          while (unitAniController.riotAnimating) { yield return null; }
        }
      }

      if (tile.flood != null) {
        HashSet<Tile> tiles = tile.flood.OnWeatherChange(weather);
        if (tiles.Count > 0) {
          Flood(null, tile, tiles);
          while (FloodAnimating) { yield return null; }
        }
      }

      if (tile.wildFire != null) {
        HashSet<Tile> tiles = tile.wildFire.OnWeatherChange(weather);
        if (tiles.Count > 0) {
          Burn(tile, tiles);
          while (BurnAnimating) { yield return null; }
        }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      WeatherAnimating = false;
    }
  }

}