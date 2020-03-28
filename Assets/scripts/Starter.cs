using System.Collections.Generic;
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

    // controllers that will emit event on initialization, initiate it at last, as order matters
    TurnController turnController = GameObject.FindObjectOfType<TurnController>();
    controllers.Add(turnController);

    // for 100k man fight for 6 months
    const int supply = 10 * Infantry.MaxTroopNum * 60;
    //initialization all controllers
    hexMap.PreGameInit();
    hexMap.SetWarParties(
      new WarParty(false, Cons.Song, supply),
      new WarParty(true, Cons.Liao, supply)
    );
    foreach(BaseController controller in controllers) {
      controller.PreGameInit(hexMap, controller);
    }

    hexMap.PostGameInit();
    foreach(BaseController controller in controllers) {
      controller.PostGameInit();
    }

    StartButton.SetActive(false);

    // TODO build dam
    Tile dam = hexMap.GetTile(16, 15);
    dam.BuildDam();
    
    // init generals
    General liubei = new General("g_liubei", "g_liubei_d", Cons.heNan, new Traits[0]); 
    General guanyu = new General("g_guanyu", "g_guanyu_d", Cons.heNan, new Traits[0]); 
    General zhangfei = new General("g_zhangfei", "g_zhangfei_d", Cons.heDong, new Traits[0]); 
    General zhaoyun = new General("g_zhaoyun", "g_zhaoyun_d", Cons.heDong, new Traits[0]); 
    General machao = new General("g_machao", "g_machao_d", Cons.heBei, new Traits[0]); 
    liubei.JoinFaction(Cons.Song, Cons.NewParty);
    guanyu.JoinFaction(Cons.Song, Cons.NewParty);
    zhangfei.JoinFaction(Cons.Song, Cons.OldParty);
    zhaoyun.JoinFaction(Cons.Song, Cons.OldParty);
    machao.JoinFaction(Cons.Song, Cons.OldParty);

    General caocao = new General("g_caocao", "g_caocao_d", Cons.xiJing, new Traits[0]); 
    General xuchu = new General("g_xuchu", "g_xuchu_d", Cons.xiJing, new Traits[0]); 
    General abc = new General("g_abc", "g_abc", Cons.zhongJing, new Traits[0]); 
    General x1 = new General("g_x1", "g_1", Cons.zhongJing, new Traits[0]); 
    General x2 = new General("g_x2", "g_1", Cons.shangJing, new Traits[0]); 
    General x3 = new General("g_x3", "g_1", Cons.shangJing, new Traits[0]); 
    General x4 = new General("g_x4", "g_1", Cons.shangJing, new Traits[0]); 
    caocao.JoinFaction(Cons.Liao, Cons.NorthCourt);
    xuchu.JoinFaction(Cons.Liao, Cons.NorthCourt);
    abc.JoinFaction(Cons.Liao, Cons.NorthCourt);
    x1.JoinFaction(Cons.Liao, Cons.SouthCourt);
    x2.JoinFaction(Cons.Liao, Cons.SouthCourt);
    x3.JoinFaction(Cons.Liao, Cons.SouthCourt);
    x4.JoinFaction(Cons.Liao, Cons.SouthCourt);

    // step 3, assign general to units
    liubei.CreateTroop(hexMap, 8000, Cons.heNan, Type.Infantry, Cons.elite);
    zhaoyun.CreateTroop(hexMap, 6000, Cons.heBei, Type.Infantry, Cons.veteran);
    guanyu.CreateTroop(hexMap, 4500, Cons.heBei, Type.Infantry, Cons.rookie);
    machao.CreateTroop(hexMap, 3000, Cons.heDong, Type.Infantry, Cons.veteran);
    zhangfei.CreateTroop(hexMap, 1500, Cons.shanXi, Type.Cavalry, Cons.elite);

    caocao.CreateTroop(hexMap, 10000, Cons.xiJing, Type.Infantry, Cons.elite);
    xuchu.CreateTroop(hexMap, 2500, Cons.zhongJing, Type.Cavalry, Cons.elite);
    Cons.Liao.SetRoyalGuard(xuchu);
    abc.CreateTroop(hexMap, 8000, Cons.xiJing, Type.Infantry, Cons.veteran);
    x1.CreateTroop(hexMap, 1000, Cons.shangJing, Type.Cavalry, Cons.rookie);
    x2.CreateTroop(hexMap, 10000, Cons.xiJing, Type.Infantry, Cons.rookie);
    x3.CreateTroop(hexMap, 8000, Cons.xiJing, Type.Infantry, Cons.rookie);
    x4.CreateTroop(hexMap, 4000, Cons.zhongJing, Type.Infantry, Cons.rookie);

    // create settlements
    // * tactical phase starts *
    Tile strategyBase = hexMap.GetTile(1, 1);
    Tile camp1 = hexMap.GetTile(10, 17);
    Tile camp2 = hexMap.GetTile(19, 2);
    Tile city = hexMap.GetTile(27, 17);
    // Set Route
    strategyBase.linkedTilesForCamp.Add(camp1);
    camp1.linkedTilesForCamp.Add(strategyBase);
    camp1.linkedTilesForCamp.Add(camp2);
    camp2.linkedTilesForCamp.Add(camp1);
    camp2.linkedTilesForCamp.Add(city);
    city.linkedTilesForCamp.Add(camp2);

    Settlement s = settlementMgr.BuildStrategyBase(hexMap.GetTile(1,1), hexMap.GetWarParty(Cons.Liao));
    if (s == null) {
      Util.Throw("Failed to build base at 1,1");
    }
    settlementMgr.attackerRoot = s;

    if (settlementMgr.BuildCamp("虎牢关", camp1, hexMap.GetWarParty(Cons.Song), 1) == null) {
      Util.Throw("Failed to build base at 10,17");
    }
    if (settlementMgr.BuildCity("金州", camp2, hexMap.GetWarParty(Cons.Song), 2,
      34000, // male
      23889, // female
      8888, // child
      3
    ) == null) {
      Util.Throw("Failed to build base at 10,17");
    }
    s = settlementMgr.BuildCity("河间府", hexMap.GetTile(27, 17),
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

    // after settlement created, general enters campaign
    liubei.EnterCampaign(hexMap, hexMap.GetTile(27, 18));
    zhaoyun.EnterCampaign(hexMap, hexMap.GetTile(28, 18));
    guanyu.EnterCampaign(hexMap, hexMap.GetTile(27, 17));
    machao.EnterCampaign(hexMap, hexMap.GetTile(27, 17));
    zhangfei.EnterCampaign(hexMap, hexMap.GetTile(28, 17));
    hexMap.GetWarParty(liubei.faction).commanderGeneral = liubei;

    // * AI *
    caocao.EnterCampaign(hexMap, hexMap.GetTile(1, 1));
    abc.EnterCampaign(hexMap, hexMap.GetTile(2, 2));
    xuchu.EnterCampaign(hexMap, hexMap.GetTile(2, 5));
    x1.EnterCampaign(hexMap, hexMap.GetTile(2, 3));
    x2.EnterCampaign(hexMap, hexMap.GetTile(2, 4));
    x3.EnterCampaign(hexMap, hexMap.GetTile(1, 1));
    x4.EnterCampaign(hexMap, hexMap.GetTile(1, 1));
    hexMap.GetWarParty(caocao.faction).commanderGeneral = caocao;
    SettlementMgr.Ready4Refresh = true;
    FoW.Init(hexMap);
    Unit commander = hexMap.GetPlayerParty().commanderGeneral.commandUnit.onFieldUnit;
    View view;
    if (commander.IsCamping()) {
      view = hexMap.GetTileView(commander.tile);
    } else {
      view = hexMap.GetUnitView(commander);
    }
    cameraKeyboardController.FixCameraAt(view.transform.position);
  }
}
