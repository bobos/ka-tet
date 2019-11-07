﻿using UnityEngine;
using MapTileNS;
using MonoNS;
using System.Collections;

namespace UnitNS
{
  public class UnitView : View
  {
    Vector3 newPosition;
    Vector3 currentVelocity;
    HexMap hexMap;
    public Unit unit;
    public GameObject nameGO;
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
      actionController.actionDone += ActionDone;
      mouseController.onUnitSelect += OnUnitSelect;
      mouseController.onModeQuit += OnModeQuit;
      mouseController.onUnitDeselect += OnUnitDeselect;
    }

    public void Deactivate() {
      foreach(MeshRenderer mr in nameGO.GetComponentsInChildren<MeshRenderer>()) {
        mr.enabled = false;
      }
      viewActivated = false;
    }

    public void Activate() {
      foreach(MeshRenderer mr in nameGO.GetComponentsInChildren<MeshRenderer>()) {
        mr.enabled = true;
      }
      transform.position = unit.tile.GetSurfacePosition();
      viewActivated = true;
    }

    public void OnModeQuit(Unit unit)
    {
      OnUnitSelect(unit);
    }

    public void Destroy()
    {
      actionController.onBtnClick -= OnBtnClick;
      actionController.actionDone -= ActionDone;
      mouseController.onUnitSelect -= OnUnitSelect;
      mouseController.onModeQuit -= OnModeQuit;
      mouseController.onUnitDeselect -= OnUnitDeselect;
      Destroy(nameGO);
    }

    public void ActionDone(Unit actionUnit, Unit[] units, ActionController.actionName actionName)
    {
      if (actionName == ActionController.actionName.ATTACK)
      {
        foreach (Unit attacker in units)
        {
          if (Util.eq<Unit>(attacker, unit))
          {
          }
        }
      }
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
      if (!viewActivated) return;
      hexMap.HighlightPath(unit.GetPath());
      Animating = true;
      newPosition = newTile.GetSurfacePosition();
      currentVelocity = Vector3.zero;
    }

    public void OnUnitSelect(Unit unit)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        hexMap.HighlightArea(unit.GetAccessibleTiles(), HexMap.RangeType.movement, this.unit);
        hexMap.HighlightPath(unit.GetPath());
        if (!unit.IsAI() && !unit.TurnDone()) hexMap.HighlightUnit(unit);
      }
    }

    public void OnUnitDeselect(Unit unit)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        hexMap.DehighlightPath();
        hexMap.DehighlightArea();
        hexMap.SetUnitSkin(unit);
      }
    }

    public void OnBtnClick(ActionController.actionName name)
    {
      if (Util.eq<Unit>(mouseController.selectedUnit, unit))
      {
        if (name == ActionController.actionName.MOVE)
        {
          hexMap.HighlightArea(unit.GetAccessibleTiles(), HexMap.RangeType.movement, this.unit);
        }
        if (name == ActionController.actionName.ATTACK)
        {
          hexMap.HighlightArea(unit.GetAttackRange(), HexMap.RangeType.attack);
        }
      }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
      if (!Animating || !viewActivated) { return; }
      Vector3 originPosition = this.transform.position;
      // NOTE: this point to the gameobject not the component
      this.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.2f);
      nameGO.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.2f);
      if (Vector3.Distance(this.transform.position, newPosition) < 0.1f)
      {
        Vector3 p = nameGO.transform.position;
        nameGO.transform.position = new Vector3(p.x - 0.5f, p.y, p.z);
        Animating = false;
      }
    }

  }

}