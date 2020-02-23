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
    Cons.InitRegion();
    Cons.huaiWest.ownerFaction = Cons.HeJian;
    Cons.huaiWest.ownerParty = Cons.Pigeon;
    // Set War Province
    hexMap.warProvince = Cons.huaiWest;

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
    weatherGenerator.season = Cons.summer;
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
    Tile dam = hexMap.GetTile(16, 15);
    dam.BuildDam();

    // * tactical phase starts *
    Tile strategyBase = hexMap.GetTile(1, 1);
    Tile camp1 = hexMap.GetTile(10, 17);
    camp1.SetAsCampField("虎牢关", 1);
    Tile camp2 = hexMap.GetTile(19, 2);
    camp2.SetAsCampField("飞狐口", 2);
    Tile city = hexMap.GetTile(27, 17);
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
    guanyu.JoinFaction(Cons.HeJian, Cons.Eagle);
    zhangfei.JoinFaction(Cons.HeJian, Cons.Pigeon);
    zhaoyun.JoinFaction(Cons.HeJian, Cons.Eagle);
    machao.JoinFaction(Cons.HeJian, Cons.Eagle);

    General caocao = new General("g_caocao", "g_caocao_d", Cons.riverNorth, new Traits[0]); 
    General xuchu = new General("g_xuchu", "g_xuchu_d", Cons.riverNorth, new Traits[0]); 
    General abc = new General("g_abc", "g_abc", Cons.riverNorth, new Traits[0]); 
    General x1 = new General("g_x1", "g_1", Cons.riverNorth, new Traits[0]); 
    General x2 = new General("g_x2", "g_1", Cons.riverNorth, new Traits[0]); 
    caocao.JoinFaction(Cons.Liang, Cons.Tiger);
    xuchu.JoinFaction(Cons.Liang, Cons.Tiger);
    abc.JoinFaction(Cons.Liang, Cons.Tiger);
    x1.JoinFaction(Cons.Liang, Cons.Tiger);
    x2.JoinFaction(Cons.Liang, Cons.Tiger);

    // step 3, assign general to units
    liubei.Assign(hexMap,
      new Troop(8000, Cons.HeJian, Cons.riverRun, Type.Infantry, Cons.elite)
    );
    zhaoyun.Assign(hexMap,
      new Troop(4000, Cons.HeJian, Cons.riverRun, Type.Infantry, Cons.veteran)
    );
    guanyu.Assign(hexMap,
      new Troop(4000, Cons.HeJian, Cons.middleEarth, Type.Infantry, Cons.rookie)
    );
    zhangfei.Assign(hexMap,
      new Troop(2000, Cons.HeJian, Cons.riverWest, Type.Cavalry, Cons.veteran)
    );

    caocao.Assign(hexMap,
      new Troop(10000, Cons.Liang, Cons.riverSouth, Type.Infantry, Cons.elite)
    );
    xuchu.Assign(hexMap,
      new Troop(2000, Cons.Liang, Cons.middleEarth, Type.Cavalry, Cons.elite)
    );
    abc.Assign(hexMap,
      new Troop(8000, Cons.Liang, Cons.riverSouth, Type.Infantry, Cons.veteran)
    );
    x1.Assign(hexMap,
      new Troop(1500, Cons.Liang, Cons.middleEarth, Type.Cavalry, Cons.rookie)
    );
    x2.Assign(hexMap,
      new Troop(10000, Cons.Liang, Cons.riverSouth, Type.Infantry, Cons.rookie)
    );

    // step 4, on field assignment
    hexMap.SetWarParties(
      new WarParty(false, false, Cons.HeJian),
      new WarParty(true, true, Cons.Liang)
    );

    // create settlements
    const int supply = 4 * Infantry.MaxTroopNum / 10 * Infantry.BaseSlots;
    if (!settlementMgr.BuildStrategyBase(hexMap.GetTile(1,1),
                    hexMap.GetAIParty(), supply * 5 * 5, 5000)) {
      Util.Throw("Failed to build base at 1,1");}
    if (!settlementMgr.BuildCity("河间府", hexMap.GetTile(27, 17),
                    hexMap.GetPlayerParty(),
                    2, // wallLevel
                    34000, // male
                    23889, // female
                    8888, // child
                    5000, // labor
                    supply * 5 * 5)) {
      Util.Throw("Failed to build city at 29,12");}

    settlementMgr.BuildRoad(strategyBase, camp1);
    settlementMgr.BuildRoad(camp1, camp2);
    settlementMgr.BuildRoad(camp2, city);

    // after settlement created, general enters campaign
    liubei.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(27, 18), 0, 4000);
    zhaoyun.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(28, 18), 30000, 2000);
    guanyu.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(27, 17), 0, 200);
    zhangfei.EnterCampaign(hexMap, hexMap.GetPlayerParty(), hexMap.GetTile(28, 17), 0);

    // * AI *
    //caocao.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(1, 1), 0, 6000);
    caocao.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(26, 15), 70000, 5000);
    abc.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(25, 16), 6000, 4000);
    xuchu.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(27, 15), 3000);
    x1.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(24, 15), 3000);
    x2.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(24, 17), 10000, 1000);
    //xuchu.EnterCampaign(hexMap, hexMap.GetAIParty(), hexMap.GetTile(2, 1), 0);
    SettlementMgr.Ready4Refresh = true;
    FoW.Init(hexMap);
    cameraKeyboardController.FixCameraAt(hexMap.GetUnitView(liubei.commandUnit.onFieldUnit).transform.position);
  }
}
