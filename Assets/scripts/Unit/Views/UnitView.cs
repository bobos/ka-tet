﻿using UnityEngine;
using MapTileNS;
using MonoNS;
using System.Collections;
using System.Collections.Generic;

namespace UnitNS
{
  public class UnitView : View
  {
    Vector3 newPosition;
    Vector3 currentVelocity;
    HexMap hexMap;
    public Unit unit;
    public GameObject unitInfoGO;
    public GameObject unitIconGO;
    ActionController actionController;
    MouseController mouseController;

    // Call Start by next frame is too late, need to call this init on create
    public override void OnCreate(DataModel unit)
    {
      this.unit = (Unit)unit;
      Animating = false;
      newPosition = this.transform.position;
      hexMap = GameObject.FindObjectOfType<HexMap>();
      actionController = hexMap.actionController;
      mouseController = hexMap.mouseController;
      actionController.onBtnClick += OnBtnClick;
      mouseController.onUnitSelect += OnUnitSelect;
      mouseController.onModeQuit += OnModeQuit;
      mouseController.onUnitDeselect += OnUnitDeselect;
    }

    public static Vector3 UnitInfoPosition(Vector3 unitPosition) {
      return new Vector3(unitPosition.x - 0.5f, unitPosition.y + 0.6f, unitPosition.z);
    }

    public static Vector3 NamePosition(Vector3 unitPosition) {
      return new Vector3(unitPosition.x - 0.5f, unitPosition.y + 0.6f, unitPosition.z + 0.35f);
    }

    void UpdateUnitInfo() {
      unitInfoGO.transform.position = UnitInfoPosition(transform.position);
      unitInfoGO.GetComponent<UnitInfoView>().SetName(unit);
      unitIconGO.transform.position = NamePosition(transform.position);
      unitIconGO.GetComponent<UnitIconView>().SetName(unit);
    }

    public void ToggleText(bool on) {
      foreach(MeshRenderer mr in unitInfoGO.GetComponentsInChildren<MeshRenderer>()) {
        mr.enabled = on;
      }
      foreach(MeshRenderer mr in unitIconGO.GetComponentsInChildren<MeshRenderer>()) {
        mr.enabled = on;
      }
    }

    public void Deactivate() {
      OnUnitDeselect(mouseController.selectedUnit);
      ToggleText(false);
      viewActivated = false;
    }

    public void Activate() {
      ToggleText(true);
      transform.position = unit.tile.GetSurfacePosition();
      viewActivated = true;
    }

    public void OnModeQuit(Unit unit)
    {
      OnUnitSelect(unit);
    }

    public void Destroy()
    {
      OnUnitDeselect(mouseController.selectedUnit);
      actionController.onBtnClick -= OnBtnClick;
      mouseController.onUnitSelect -= OnUnitSelect;
      mouseController.onModeQuit -= OnModeQuit;
      mouseController.onUnitDeselect -= OnUnitDeselect;
      Destroy(unitInfoGO);
      Destroy(unitIconGO);
    }

    public void DestroyAnimation(DestroyType type)
    {
      Animating = true;
      StartCoroutine(CoDestroyAnimation(type));
    }

    IEnumerator CoDestroyAnimation(DestroyType type) {
      yield return new WaitForSeconds(1);
      Animating = false;
    }

    public void RoutAnimation()
    {
      Animating = true;
      StartCoroutine(CoRoutAnimation());
    }

    IEnumerator CoRoutAnimation() {
      yield return new WaitForSeconds(1);
      Animating = false;
    }

    public void Move(Tile newTile)
    {
      newPosition = newTile.GetSurfacePosition();
      if (viewActivated) {
        hexMap.HighlightPath(unit.GetPath());
        Animating = true;
      }
      currentVelocity = Vector3.zero;
    }

    public void OnUnitSelect(Unit unit, bool _isGarrison=false)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        //if (!unit.IsAI() && !unit.TurnDone()) hexMap.HighlightUnit(unit);
        if (hexMap.wargameController.start && hexMap.wargameController.IsWargameUnit(unit)) {
        } else {
          Tile[] range = unit.GetAccessibleTiles(unit.IsAI() == hexMap.turnController.playerTurn);
          hexMap.HighlightArea(range, HexMap.RangeType.movement);
          hexMap.HighlightPath(unit.GetPath());
        }
      }
    }

    public void OnUnitDeselect(Unit unit)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        hexMap.DehighlightPath();
        hexMap.DehighlightArea();
        hexMap.UnhighlightUnit(unit);
      }
    }

    public void OnBtnClick(ActionController.actionName name)
    {
      if (Util.eq<Unit>(mouseController.selectedUnit, unit))
      {
        if (name == ActionController.actionName.MOVE)
        {
          hexMap.HighlightArea(unit.GetAccessibleTiles(), HexMap.RangeType.movement);
        }
        if (name == ActionController.actionName.ATTACK)
        {
        }
      }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
      UpdateUnitInfo();
      if (!Animating) {
        HashSet<Unit> units = unit.tile.GetUnits();
        if (units.Count > 1) {
          bool allStop = true;
          foreach(Unit unit in units) {
            if (unit.GetPath().Length != 0) {
              allStop = false;
            }
          }

          if (allStop) {
            // aquire lock
            lock(hexMap.actionController.movingLock) {
              if (unit.tile.GetUnits().Count > 1 && !hexMap.actionController.movingLock.ContainsKey(unit.tile)) {
                // aquired
                hexMap.actionController.movingLock[unit.tile] = unit;
                hexMap.actionController.reverseMovingLock[unit] = unit.tile;
                Tile target = null;
                foreach(Tile t in unit.tile.neighbours) {
                  if (t.Deployable(unit)) {
                    target = t;
                    break;
                  }
                }
                if (target == null) {
                  foreach(Tile t in unit.tile.neighbours) {
                    if (t.Accessible()) {
                      target = t;
                      break;
                    }
                    target = t;
                  }
                }

                Animating = true;
                hexMap.unitAniController.MoveUnit(unit, target);
              }
            }
          }
        }
        return;
      }
      Vector3 originPosition = this.transform.position;
      // NOTE: this point to the gameobject not the component
      this.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.1f);
      //unitInfoGO.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.1f);
      if (Vector3.Distance(this.transform.position, newPosition) < 0.1f)
      {
        if (hexMap.actionController.movingLock.ContainsValue(unit)) {
          Tile tile = hexMap.actionController.reverseMovingLock[unit];
          hexMap.actionController.reverseMovingLock.Remove(unit);
          hexMap.actionController.movingLock.Remove(tile);
        }
        Animating = false;
      }
    }

  }

}