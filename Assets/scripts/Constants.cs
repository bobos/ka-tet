﻿using NatureNS;
using TextNS;
using CourtNS;

public class Cons {
  
  public static bool HighlyLikely() {
    return Util.Rand(0f, 1f) < 0.8f;
  }

  public static bool MostLikely() {
    return Util.Rand(0f, 1f) < 0.65f;
  }

  public static bool FiftyFifty() {
    return Util.Rand(0f, 1f) < 0.5f;
  }

  public static bool EvenChance() {
    return Util.Rand(0f, 1f) < 0.45f;
  }

  public static bool FairChance() {
    return Util.Rand(0f, 1f) < 0.25f;
  }

  public static bool SlimChance() {
    return Util.Rand(0f, 1f) < 0.1f;
  }

  public static Season spring = new Spring();
  public static bool IsSpring(Season season) {
    return Util.eq<Season>(season, spring);
  }

  public static Season summer = new Summer();
  public static bool IsSummer(Season season) {
    return Util.eq<Season>(season, summer);
  }

  public static Season autumn = new Autumn();
  public static bool IsAutumn(Season season) {
    return Util.eq<Season>(season, autumn);
  }

  public static Season winter = new Winter();
  public static bool IsWinter(Season season) {
    return Util.eq<Season>(season, winter);
  }

  public static Weather cloudy = new Cloudy();
  public static bool IsCloudy(Weather weather) {
    return Util.eq<Weather>(weather, cloudy);
  }
  public static Weather rain = new Rain();
  public static bool IsRain(Weather weather) {
    return Util.eq<Weather>(weather, rain);
  }
  public static Weather heavyRain = new HeavyRain();
  public static bool IsHeavyRain(Weather weather) {
    return Util.eq<Weather>(weather, heavyRain);
  }
  public static Weather heat = new Heat();
  public static bool IsHeat(Weather weather) {
    return Util.eq<Weather>(weather, heat);
  }
  public static Weather dry = new Dry();
  public static bool IsDry(Weather weather) {
    return Util.eq<Weather>(weather, dry);
  }
  public static Weather snow = new Snow();
  public static bool IsSnow(Weather weather) {
    return Util.eq<Weather>(weather, snow);
  }
  public static Weather blizard = new Blizard();
  public static bool IsBlizard(Weather weather) {
    return Util.eq<Weather>(weather, blizard);
  }

  public static Current nowind = new Nowind();
  public static bool IsNowind(Current current) {
    return Util.eq<Current>(current, nowind);
  }

  public static Current wind = new Wind();
  public static bool IsWind(Current current) {
    return Util.eq<Current>(current, wind);
  }

  public static Current gale = new Gale();
  public static bool IsGale(Current current) {
    return Util.eq<Current>(current, gale);
  }

  public enum Direction {
    dueNorth,
    dueSouth,
    southEast,
    southWest,
    northEast,
    northWest
  }

  public static string DirectionDisplay(Direction direction) {
    TextLib textLib = GetTextLib();
    if (direction == Direction.dueNorth) return textLib.get("d_dueNorth");
    if (direction == Direction.dueSouth) return textLib.get("d_dueSouth");
    if (direction == Direction.northEast) return textLib.get("d_northEast");
    if (direction == Direction.northWest) return textLib.get("d_northWest");
    if (direction == Direction.southEast) return textLib.get("d_southEast");
    return textLib.get("d_southWest");
  }

  public static TextLib textLib;
  public static TextLib GetTextLib () {
    // TODO: use strategy pattern
    if (textLib == null) {
      textLib = new TextLibChn();
    }
    return textLib;
  }

  //* regions */
  public static Region riverRun = new RiverRun();
  public static Region middleEarth = new MiddleEarth();
  public static Region moutainBeyond = new MountainBeyond();

  //* factions */
  public static Faction Jin = new Jin(true, 2000000);
  public static Faction Liang = new Liang(true, 5000000);
  public static Faction HeJian = new HeJian(false, 100000);
  public static Faction GF = new Ghost();

  //* parties */
  //* HeJian */
  public static Party Pigeon = new Party(HeJian, "p_pigeonParty", "p_pigeonParty_d");
  public static Party Eagle = new Party(HeJian, "p_eagleParty", "p_eagleParty_d");

  //* Liang */
  public static Party Tiger = new Party(Liang, "p_yanParty", "p_yanParty_d");
}