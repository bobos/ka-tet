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
    // Set War Region
    hexMap.warRegion = Cons.huaiWest;

    // controllers only add callbacks on initialization, will not emit initial events
    List<BaseController> controllers = new List<BaseController>();
    ActionController actionController = GameObject.FindObjectOfType<ActionController>();
    controllers.Add(actionController);
    EventDialog eventDialog = GameObject.FindObjectOfType<EventDialog>();
    controllers.Add(eventDialog);
    MouseController mouseController = GameObject.FindObjectOfType<MouseController>();
    controllers.Add(mouseController);
    WeatherGenerator weatherGenerator = GameObject.FindObjectOfType<WeatherGenerator>();
    weatherGenerator.season = Cons.summer;
    controllers.Add(weatherGenerator);
    WindGenerator windGenerator = GameObject.FindObjectOfType<WindGenerator>();
    controllers.Add(windGenerator);
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

    // controllers that will emit event on initialization, initiate it at last, as order matters
    TurnController turnController = GameObject.FindObjectOfType<TurnController>();
    controllers.Add(turnController);

    //initialization all controllers
    hexMap.PreGameInit();
    foreach(BaseController controller in controllers) {
      controller.PreGameInit(hexMap, controller);
    }

    hexMap.PostGameInit();
    foreach(BaseController controller in controllers) {
      controller.PostGameInit();
    }

    StartButton.SetActive(false);

    // TODO build dam
    Tile dam = hexMap.GetTile(15, 15);
    dam.BuildDam();
    dam = hexMap.GetTile(16, 15);
    dam.BuildDam();

    // * tactical phase starts *
    Tile strategyBase = hexMap.GetTile(1, 1);
    Tile camp1 = hexMap.GetTile(10, 17);
    //Tile camp2 = hexMap.GetTile(19, 2);
    Tile camp2 = hexMap.GetTile(27, 16);
    Tile city = hexMap.GetTile(29, 15);
    HashSet<Tile> tiles = new HashSet<Tile>();
    tiles.Add(camp1);
    tiles.Add(camp2);
    settlementMgr.SetCampableField(tiles);
    // Set Route
    strategyBase.linkedTilesForCamp.Add(camp1);
    camp1.linkedTilesForCamp.Add(strategyBase);
    camp1.linkedTilesForCamp.Add(camp2);
    camp2.linkedTilesForCamp.Add(camp1);
    camp2.linkedTilesForCamp.Add(city);
    city.linkedTilesForCamp.Add(camp2);

    hexMap.RerenderTileTxt();
    
    // step 1, init parties
    // for HeJian
    Cons.Pigeon.counterParty = Cons.Eagle;
    Cons.Eagle.counterParty = Cons.Pigeon;

    // init generals
    General liubei = new General("g_liubei", "g_liubei_d", Cons.middleEarth, new Traits[0]); 
    General guanyu = new General("g_guanyu", "g_guanyu_d", Cons.middleEarth, new Traits[0]); 
    General zhangfei = new General("g_zhangfei", "g_zhangfei_d", Cons.middleEarth, new Traits[0]); 
    General zhaoyun = new General("g_zhaoyun", "g_zhaoyun_d", Cons.riverRun, new Traits[0]); 
    General machao = new General("g_machao", "g_machao_d", Cons.riverRun, new Traits[0]); 
    liubei.JoinFaction(Cons.HeJian, Cons.Pigeon);
    guanyu.JoinFaction(Cons.HeJian, Cons.Pigeon);
    zhangfei.JoinFaction(Cons.HeJian, Cons.Pigeon);
    zhaoyun.JoinFaction(Cons.HeJian, Cons.Eagle);
    machao.JoinFaction(Cons.HeJian, Cons.Eagle);

    General caocao = new General("g_caocao", "g_caocao_d", Cons.riverNorth, new Traits[0]); 
    General xuchu = new General("g_xuchu", "g_xuchu_d", Cons.riverNorth, new Traits[0]); 
    caocao.JoinFaction(Cons.Liang, Cons.Tiger);
    xuchu.JoinFaction(Cons.Liang, Cons.Tiger);

    // step 3, assign general to units
    Troop troop = new Troop(2000, Cons.HeJian, Cons.riverRun, Type.Infantry);
    liubei.Assign(hexMap, troop);
    troop = new Troop(500, Cons.HeJian, Cons.riverRun, Type.Cavalry);
    zhaoyun.Assign(hexMap, troop);
    troop = new Troop(800, Cons.HeJian, Cons.riverRun, Type.Infantry);
    guanyu.Assign(hexMap, troop);
    troop = new Troop(1000, Cons.HeJian, Cons.middleEarth, Type.Cavalry);
    zhangfei.Assign(hexMap, troop);

    troop = new Troop(7000, Cons.Liang, Cons.riverNorth, Type.Infantry);
    caocao.Assign(hexMap, troop);
    troop = new Troop(400, Cons.Liang, Cons.riverNorth, Type.Cavalry);
    xuchu.Assign(hexMap, troop);

    // step 4, on field assignment
    hexMap.SetWarParties(
      new WarParty(false, false, Cons.HeJian),
      new WarParty(true, true, Cons.Liang)
    );

    // create settlements
    const int supply = 4 * Infantry.MaxTroopNum / 10 * Infantry.BaseSlots;
    if (!settlementMgr.BuildSettlement("梁军大本营", hexMap.GetTile(1,1),
                    Settlement.Type.strategyBase,
                    hexMap.GetAIParty(), 0, 10000, supply * 5 * 5)) {
      Util.Throw("Failed to build base at 1,1");}
    if (!settlementMgr.BuildSettlement("河间府", hexMap.GetTile(29, 15),
                    Settlement.Type.city,
                    hexMap.GetPlayerParty(), 250000, 5000, supply * 5 * 5)) {
      Util.Throw("Failed to build city at 29,12");}

    // after settlement created, general enters campaign
    liubei.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(27, 18), 0, 2000);
    zhaoyun.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(28, 18), 0);
    guanyu.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(27, 17), 0);
    zhangfei.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(28, 17), 0);

    // * AI *
    caocao.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(1, 1), 0, 6000);
    xuchu.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(2, 1), 0);
    SettlementMgr.Ready4Refresh = true;
    FoW.Init(hexMap);
  }
}
