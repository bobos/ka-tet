using UnityEngine;

public static class Util {
  public static int seed = (int)(System.DateTime.Now.Ticks);
  public static void SetSeed() {
    if (seed >= System.Int32.MaxValue) {
      seed = 0;
    } else {
      seed++;
    }
    Random.InitState(seed);
  }

  public static int Rand(int min, int max) {
    SetSeed();
    return Random.Range(min, max+1);
  }

  public static float Rand(float min, float max) {
    SetSeed();
    return Random.Range(min, max);
  }

  public static void Throw(string cause) {
    Debug.LogError(cause);
    throw new UnityException(cause);
  }

  public static bool eq<T>(T o1, T o2) {
    return System.Object.ReferenceEquals(o1, o2);
  }
}
