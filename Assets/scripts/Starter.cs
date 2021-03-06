﻿using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using MonoNS;
using CourtNS;
using FieldNS;

public class Starter : MonoBehaviour {

  public GameObject StartButton;
  public void OnGameStart() {
    // HexMap must be the first controller to initiate
    HexMap hexMap = GameObject.FindObjectOfType<HexMap>();
    Cons.Init();
    // Set War Province
    hexMap.warProvince = Cons.heDong;

    // controllers only add callbacks on initialization, will not emit initial events
    List<BaseController> controllers = new List<BaseController>();
    ActionController actionController = GameObject.FindObjectOfType<ActionController>();
    controllers.Add(actionController);
    WargameController wargameController = GameObject.FindObjectOfType<WargameController>();
    controllers.Add(wargameController);
    EventDialog eventDialog = GameObject.FindObjectOfType<EventDialog>();
    controllers.Add(eventDialog);
    EventDialogAlt eventDialogAlt = GameObject.FindObjectOfType<EventDialogAlt>();
    controllers.Add(eventDialogAlt);
    Dialogue dialogue = GameObject.FindObjectOfType<Dialogue>();
    controllers.Add(dialogue);
    MouseController mouseController = GameObject.FindObjectOfType<MouseController>();
    controllers.Add(mouseController);
    WeatherGenerator weatherGenerator = GameObject.FindObjectOfType<WeatherGenerator>();
    weatherGenerator.season = Cons.spring;
    controllers.Add(weatherGenerator);
    WindGenerator windGenerator = GameObject.FindObjectOfType<WindGenerator>();
    controllers.Add(windGenerator);
    EventStasher eventStasher = GameObject.FindObjectOfType<EventStasher>();
    controllers.Add(eventStasher);
    CombatController combatController = GameObject.FindObjectOfType<CombatController>();
    controllers.Add(combatController);
    CameraKeyboardController cameraKeyboardController = GameObject.FindObjectOfType<CameraKeyboardController>();
    controllers.Add(cameraKeyboardController);
    UnitSelectionPanel unitSelectionPanel = GameObject.FindObjectOfType<UnitSelectionPanel>();
    controllers.Add(unitSelectionPanel);
    SettlementViewPanel settlementViewPanel = GameObject.FindObjectOfType<SettlementViewPanel>();
    controllers.Add(settlementViewPanel);
    WeatherIndicator weatherIndicator = GameObject.FindObjectOfType<WeatherIndicator>();
    controllers.Add(weatherIndicator);
    SettlementMgr settlementMgr = GameObject.FindObjectOfType<SettlementMgr>();
    controllers.Add(settlementMgr);
    MsgBox msgBox = GameObject.FindObjectOfType<MsgBox>();
    controllers.Add(msgBox);
    HoverInfo hover = GameObject.FindObjectOfType<HoverInfo>();
    controllers.Add(hover);
    TurnIndicator turnIndicator = GameObject.FindObjectOfType<TurnIndicator>();
    controllers.Add(turnIndicator);
    TurnPhaseTitle turnPhaseTitle = GameObject.FindObjectOfType<TurnPhaseTitle>();
    controllers.Add(turnPhaseTitle);
    InputField inputField = GameObject.FindObjectOfType<InputField>();
    controllers.Add(inputField);
    PopTextAnimationController popAniController = GameObject.FindObjectOfType<PopTextAnimationController>();
    controllers.Add(popAniController);
    SettlementAnimationController settlementAniController = GameObject.FindObjectOfType<SettlementAnimationController>();
    controllers.Add(settlementAniController);
    UnitAnimationController unitAniController = GameObject.FindObjectOfType<UnitAnimationController>();
    controllers.Add(unitAniController);
    TileAnimationController tileAniController = GameObject.FindObjectOfType<TileAnimationController>();
    controllers.Add(tileAniController);
    ImgLibrary imgLibrary = GameObject.FindObjectOfType<ImgLibrary>();
    controllers.Add(imgLibrary);

    // controllers that will emit event on initialization, initiate it at last, as order matters
    TurnController turnController = GameObject.FindObjectOfType<TurnController>();
    controllers.Add(turnController);

    // init generals
    General liubei = new General("g_liubei", "g_liubei_d", Cons.heNan, new CommandSkill(3), 10);
    Ability.Acquire4Test(liubei);
    General guanyu = new General("g_guanyu", "g_guanyu_d", Cons.heNan, new CommandSkill(1), Util.Rand(1, 10));
    Ability.Acquire4Test(guanyu);
    General zhangfei = new General("g_zhangfei", "g_zhangfei_d", Cons.heDong, new CommandSkill(1), Util.Rand(1, 10));
    Ability.Acquire4Test(zhangfei);
    General zhaoyun = new General("g_zhaoyun", "g_zhaoyun_d", Cons.heDong, new CommandSkill(1), Util.Rand(1, 10));
    Ability.Acquire4Test(zhaoyun);
    General machao = new General("g_machao", "g_machao_d", Cons.heBei, new CommandSkill(1), Util.Rand(1, 10));
    Ability.Acquire4Test(machao);
    liubei.JoinFaction(Cons.Song, Cons.NewParty);
    guanyu.JoinFaction(Cons.Song, Cons.NewParty);
    zhangfei.JoinFaction(Cons.Song, Cons.OldParty);
    zhaoyun.JoinFaction(Cons.Song, Cons.OldParty);
    machao.JoinFaction(Cons.Song, Cons.OldParty);

    liubei.CreateTroop(hexMap, 10000, Cons.heNan, Type.Infantry);
    zhaoyun.CreateTroop(hexMap, 10000, Cons.heBei, Type.Infantry);
    guanyu.CreateTroop(hexMap, 10000, Cons.heBei, Type.Infantry);
    machao.CreateTroop(hexMap, 2000, Cons.heDong, Type.Cavalry);
    zhangfei.CreateTroop(hexMap, 3000, Cons.heXi, Type.Cavalry);

    General caocao = new General("g_caocao", "g_caocao_d", Cons.xiJing, new CommandSkill(3), 10);
    Ability.Acquire4Test(caocao);
    General xuchu = new General("g_xuchu", "g_xuchu_d", Cons.xiJing, new CommandSkill(2), Util.Rand(1, 10)); 
    Ability.Acquire4Test(xuchu);
    General abc = new General("g_abc", "g_abc", Cons.zhongJing, new CommandSkill(1), Util.Rand(1, 10)); 
    Ability.Acquire4Test(abc);
    General x1 = new General("g_x1", "g_1", Cons.zhongJing, new CommandSkill(1), Util.Rand(1, 10)); 
    Ability.Acquire4Test(x1);
    General x2 = new General("g_x2", "g_1", Cons.shangJing, new CommandSkill(1), Util.Rand(1, 10)); 
    Ability.Acquire4Test(x2);
    General x3 = new General("g_x3", "g_1", Cons.shangJing, new CommandSkill(1), Util.Rand(1, 10)); 
    Ability.Acquire4Test(x3);
    General x4 = new General("g_x4", "g_1", Cons.shangJing, new CommandSkill(1), Util.Rand(1, 10)); 
    Ability.Acquire4Test(x4);
    
    caocao.JoinFaction(Cons.Liao, Cons.NorthCourt);
    xuchu.JoinFaction(Cons.Liao, Cons.NorthCourt);
    abc.JoinFaction(Cons.Liao, Cons.NorthCourt);
    x1.JoinFaction(Cons.Liao, Cons.SouthCourt);
    x2.JoinFaction(Cons.Liao, Cons.SouthCourt);
    x3.JoinFaction(Cons.Liao, Cons.SouthCourt);
    x4.JoinFaction(Cons.Liao, Cons.SouthCourt);

    caocao.CreateTroop(hexMap, 10000, Cons.xiJing, Type.Infantry);
    xuchu.CreateTroop(hexMap, 3000, Cons.shangJing, Type.Cavalry);
    abc.CreateTroop(hexMap, 10000, Cons.zhongJing, Type.Infantry);
    x1.CreateTroop(hexMap, 3000, Cons.xiJing, Type.Cavalry);
    x2.CreateTroop(hexMap, 10000, Cons.shangJing, Type.Infantry);
    x3.CreateTroop(hexMap, 10000, Cons.xiJing, Type.Infantry);
    x4.CreateTroop(hexMap, 2000, Cons.zhongJing, Type.Cavalry);

    // for 100k man fight for 6 months
    const int supply = 100000 * 60;
    //initialization all controllers
    hexMap.PreGameInit();
    hexMap.SetWarParties(
      new WarParty(false, Cons.Song, liubei, supply, hexMap),
      new WarParty(true, Cons.Liao, caocao, supply, hexMap)
    );
    hexMap.deployDone = hexMap.GetWarParty(Cons.Song).commanderGeneral.commandSkill.commandSkill != 4;

    foreach(BaseController controller in controllers) {
      controller.PreGameInit(hexMap, controller);
    }

    hexMap.PostGameInit();
    foreach(BaseController controller in controllers) {
      controller.PostGameInit();
    }

    StartButton.SetActive(false);
    
    // TODO build dam
    Tile dam = hexMap.GetTile(8, 11);
    dam.BuildDam();
    
    // first one is always the commander
    List<General> defenders = new List<General>(){liubei, guanyu, zhangfei, zhaoyun, machao};
    List<General> attackers = new List<General>(){caocao, xuchu, abc, x1, x2, x3, x4};

    // create settlements
    // * tactical phase starts *
    Tile strategyBase = hexMap.GetTile(1, 1);
    Tile camp = hexMap.GetTile(9, 1);
    Tile mainCity = hexMap.GetTile(7, 10);

    // Set Route
    strategyBase.linkedTilesForCamp.Add(camp);
    camp.linkedTilesForCamp.Add(strategyBase);

    camp.linkedTilesForCamp.Add(mainCity);
    mainCity.linkedTilesForCamp.Add(camp);

    hexMap.frontier = new Tile[]{camp};
    hexMap.middleField = new Tile[]{mainCity};
    hexMap.keyPos = new Tile[]{hexMap.GetTile(6, 0)};
    hexMap.theBox = mainCity;
    hexMap.attackerReserveTile = hexMap.GetTile(1, 0);

    Settlement s = settlementMgr.BuildStrategyBase(strategyBase, hexMap.GetWarParty(Cons.Liao));
    if (s == null) {
      Util.Throw("Failed to build base at 1,1");
    }
    settlementMgr.attackerRoot = s;

    if (settlementMgr.BuildCamp("井陉", camp, hexMap.GetWarParty(Cons.Song), 1) == null) {
      Util.Throw("Failed to build base at 10,17");
    }
    s = settlementMgr.BuildCity("晋州", mainCity, 
          hexMap.GetWarParty(Cons.Song),
          3, // wallLevel
          34000, // male
          23889, // female
          8888, // child
          3);
    if (s == null) {
      Util.Throw("Failed to build city at 29,12");}
    settlementMgr.defenderRoot = s; 
    hexMap.AddTiles2Settlements();

    hexMap.InitDefendersOnMap(defenders.ToArray());
    hexMap.InitAttackersOnMap(attackers.ToArray(), strategyBase);
    SettlementMgr.Ready4Refresh = true;
    FoW.Init(hexMap);
    Unit commander = hexMap.GetPlayerParty().commanderGeneral.commandUnit.onFieldUnit;
    View view;
    if (commander.IsCamping()) {
      view = hexMap.GetTileView(commander.tile);
    } else {
      view = hexMap.GetUnitView(commander);
    }
    hexMap.AttackerZone.Add(hexMap.GetTile(0,0));
    hexMap.DefenderZone.Add(hexMap.GetTile(14,14));
    cameraKeyboardController.FixCameraAt(view.transform.position);
  }
}
