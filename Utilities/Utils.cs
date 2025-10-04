using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using UnityEditor;

[ExecuteInEditMode]
public static class Utils
{
    public static Vector2 GetDirectionNormalized(Vector2 playerPos, Vector2 origin)
    {
        return (playerPos - origin).normalized;
    }

    // Pythagorean Theorem
    public static float Pythag(float a, float b)
    {
        var c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        return c;
    }

    public static Vector2 TopCenter(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        Vector3 scale = obj.transform.localScale;
        Bounds bounds = new(position, scale);

        return new Vector2(bounds.center.x, bounds.max.y);
    }

    public static Vector2 BottomCenter(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        Vector3 scale = obj.transform.localScale;
        Bounds bounds = new(position, scale);

        return new Vector2(bounds.center.x, bounds.min.y);
    }

    // Calculates distance between two points but only along the Y-axis
    public static float GetDistanceAlongY(Vector3 point1, Vector3 point2)
    {
        return Mathf.Abs(point2.y - point1.y);
    }


    // Destroy and clear a list of objects whether in Edit or Play mode
    public static void ClearObjects<T>(List<T> objects) where T : MonoBehaviour
    {
        if (objects != null)
        {
            foreach (var obj in objects)
            {
                if (obj == null) continue;
                if (!Application.isPlaying)
                    UnityEngine.Object.DestroyImmediate(obj.gameObject);
                else
                    UnityEngine.Object.Destroy(obj.gameObject);
            }
            objects.Clear();
        }
    }

    // Iterate through a list of items backward and destroy each item
    public static void DestroyListOfItems<T>(List<T> list) where T : UnityEngine.Object
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            T item = list[i];
            UnityEngine.Object.Destroy(item);
            list.RemoveAt(i);
        }
    }


    // Method to create a texture with the specified color and size
    public static Texture2D MakeTexture(int width, int height, Color color)
    {
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        Texture2D texture = new (width, height);
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    public static Color SamplePrimaryColor(Sprite sprite)
    {
        Texture2D texture = sprite.texture;
        if (texture == null) return Color.white;

        Color[] pixels = texture.GetPixels();
        Dictionary<Color, int> colorCounts = new ();

        foreach (Color pixelColor in pixels)
        {
            if (colorCounts.ContainsKey(pixelColor))
            {
                colorCounts[pixelColor]++;
            }
            else
            {
                colorCounts[pixelColor] = 1;
            }
        }

        // Find the color with the highest occurrence
        Color primaryColor = Color.white;
        int maxCount = 0;
        foreach (KeyValuePair<Color, int> kvp in colorCounts)
        {
            if (kvp.Value > maxCount)
            {
                primaryColor = kvp.Key;
                maxCount = kvp.Value;
            }
        }

        return primaryColor;
    }

    public static List<T> ShuffleList<T>(List<T> originalList)
    {
        return originalList.OrderBy(x => Guid.NewGuid()).ToList();
    }

    public static void DeleteAllTiles(Tilemap tilemap)
    {
        tilemap.ClearAllTiles();
        // Comment out due to build fail
        //tilemap.ClearAllEditorPreviewTiles();

        // Comment out due to build fail
        //EditorUtility.SetDirty(tilemap);
    }

    public static void DrawDebugMapLabel(Vector2 center, float lineLength, Color color, string text, int fontSize = 12, bool leftSide = false)
    {
        Gizmos.color = color;

        var position = leftSide ? center - new Vector2(lineLength, 0f) : center + new Vector2(lineLength, 0f);
        Gizmos.DrawLine(center, position);

        var style = new GUIStyle();

        style.normal.textColor = color;
        style.fontSize = fontSize;
        var pos = new Vector2(position.x, position.y);

        // Comment out due to build fail
        //Handles.Label(pos, text, style);

    }

    public static void SavePrefab(GameObject obj, string path, string prefabName = null)
    {
        if (obj != null)
        {
            var name = !string.IsNullOrEmpty(prefabName) ? prefabName : obj.name;
            var prefabPath = $"{path}/{name}.prefab";


            // Comment out due to build fail
            //PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            Debug.Log($"Utils: Prefab {name} created at {path}.");
        }
        else
        {
            Debug.LogError("Utils: Unable to save Prefab -- no gameObject found.");
        }
    }

    // Editor button shorthand
    public static bool Btn(string text, int height = 40)
    {
        return GUILayout.Button(text, GUILayout.Height(height));
    }

    public static bool IsIndexWithinBounds(int index, List<object> list)
    {
        if (list == null)
        {
            Debug.LogError("List is null.");
            return false;
        }

        return index >= 0 && index < list.Count;
    }

    public static bool IsIndexWithinBounds(int index, object[] array)
    {
        if (array == null)
        {
            Debug.LogError("Array is null.");
            return false;
        }

        return index >= 0 && index < array.Length;
    }

    public static BoundsInt CalculateCellBounds(Tile[,] tileArray)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        int width = tileArray.GetLength(0);
        int height = tileArray.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tileArray[x, y] != null)
                {
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        if (minX <= maxX && minY <= maxY)
        {
            Vector3Int min = new Vector3Int(minX, minY, 0);
            Vector3Int max = new Vector3Int(maxX, maxY, 0);

            return new BoundsInt(min, max - min + Vector3Int.one);
        }
        else
        {
            return new BoundsInt(Vector3Int.zero, Vector3Int.zero);
        }
    }


    public static string ApplyStatDisplayFormatting(float valueToFormat, int decimalPlaces, float displayMultiplier, bool showPercentSign)
    {
        var decimalFormat = $"F{decimalPlaces}";
        var displayValue = displayMultiplier > 0 ? valueToFormat * displayMultiplier : valueToFormat;
        var displayText = showPercentSign ? $"{displayValue.ToString(decimalFormat)}%" : displayValue.ToString(decimalFormat);

        return displayText;
    }

    public static string FormatCurrencyText(int number)
    {
        const int roundingThreshold = 10000;

        if (number < roundingThreshold)
        {
            return number.ToString();
        }

        // Round to the nearest hundred and append 'k'
        int roundedNumber = (int)Math.Round((double)number / 100) * 100;
        return $"{roundedNumber / 100.0:F1}k";
    }

    //public static IEnumerator WaitFor(object target, float timeout)
    //{
    //    var interval = 0.1f;
    //    var waitTime = timeout * interval;
    //    float initTimer = 0;
    //    while (target == null)
    //    {
    //        initTimer += interval;
    //        yield return new WaitForSecondsRealtime(interval);
    //        if (initTimer >= waitTime)
    //        {
    //            yield break;
    //        }
    //    }
    //}

    public static IEnumerator WaitFor(bool value, float timeout)
    {
        var interval = 0.1f;
        var waitTime = timeout * interval;
        float initTimer = 0;
        while (!value)
        {
            initTimer += interval;
            yield return new WaitForSecondsRealtime(interval);
            if (initTimer >= waitTime)
            {
                yield break;
            }
        }
    }

    public static float CalculateSliderValue(float current, float max)
    {
        return (float)current / max;
    }

    public static float ConvertPercentageToDecibels(float percentage)
    {
        return percentage < 0.01 ? -80f : Mathf.Log10(percentage) * 20;
    }

    public static string FormatPercent(float value, int decimalPlaces = 0)
    {
        var percent = value * 100;
        var valueToRound = decimalPlaces > 0 ? (percent * Mathf.Pow(10.0f, decimalPlaces) / Mathf.Pow(10.0f, decimalPlaces)) : percent;
        float roundedValue = Mathf.Round(valueToRound);
        return $"{roundedValue}%";
    }

    // NOTE: Sprite must be marked as read/write enabled in import settings, and this affects performance
    public static Sprite CreateInvertedColorSprite(Sprite sprite)
    {
        Texture2D texture = sprite.texture;
        Color32[] pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(1f - pixels[i].r, 1f - pixels[i].g, 1f - pixels[i].b, pixels[i].a);
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public static bool ClickOrTap()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        return false;
    }

    public static bool CoinFlip()
    {
        return Random.value <= 0.5f;
    }

    public static KeyValuePair<TKey, TValue> GetRandomFromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        System.Random random = new();
        int randomIndex = random.Next(0, dictionary.Count);
        return dictionary.ElementAt(randomIndex);
    }

    public static T GetRandomFromArray<T>(T[] array)
    {
        if (array.Any())
        {
            return array[Random.Range(0, array.Length)];
        }

        Debug.LogError("Couldn't return random item from array because it was null.");
        return default;
    }

    public static T GetRandomEnumValue<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, values.Length));
    }

    public static string FormatNumberWithCommas(long number)
    {
        string formattedNumber = string.Format("{0:n0}", number);
        return formattedNumber;
    }

    public static object ArrayIsValid(object[] objectArray)
    {
        var arrayIsValid = objectArray.Length > 0 && objectArray[0] != null;

        if (arrayIsValid)
        {
            return arrayIsValid;
        }

        return false;
    }

    public static string TimeFormat(float clock)
    {
        return string.Format("{0:0}:{1:00}", Mathf.FloorToInt(clock / 60F), Mathf.FloorToInt(clock - (Mathf.FloorToInt(clock / 60F)) * 60));
    }

    public static string TimeFormatHundreds(float clock)
    {
        int minutes = Mathf.FloorToInt(clock / 60F);
        int seconds = Mathf.FloorToInt(clock % 60F);
        int hundreds = Mathf.FloorToInt((clock * 100F) % 100F);

        return string.Format("{0:0}:{1:00}.{2:00}", minutes, seconds, hundreds);
    }


    public static float CalculatePercentage(float current, float max)
    {
        return current / max * 100;
    }

    public static float RandomizeValueWithinRange(float value, float valueRange)
    {
        return Random.Range(value * (1 + valueRange), value * (1 - valueRange));
    }

    public static Vector2 GetRandomDirection()
    {
        var x = Random.insideUnitCircle;
        if (x == Vector2.zero) x = GetRandomDirection();
        return x.normalized;
    }

    public static float GetDistance(Vector3 position1, Vector3 position2)
    {
        var heading = position1 - position2;

        var distanceSquared = heading.x * heading.x + heading.y * heading.y + heading.z * heading.z;
        var distance = Mathf.Sqrt(distanceSquared);

        return distance;
    }

    public static Vector2 GetDirection(Vector3 position1, Vector3 position2)
    {
        Vector3 direction;
        var heading = position1 - position2;

        var distanceSquared = heading.x * heading.x + heading.y * heading.y + heading.z * heading.z;
        var distance = Mathf.Sqrt(distanceSquared);

        direction.x = heading.x / distance;
        direction.y = heading.y / distance;
        direction.z = heading.z / distance;

        return direction.normalized;
    }

    public static Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public static string ShortName(string name)
    {
        if (name.Contains("Clone"))
        {
            return name.Remove(name.Length - 7);
        }
        // Remove "(#)" from name
        else if (name.Contains("("))
        {
            return name.Remove(name.Length - 4);
        }

        return name;
    }

    public static float GetRandomRoundedValue(float min, float max)
    {
        var rand = Random.Range(min, max);
        var roundedValue = Math.Round(rand * 2) / 2.0; // Rounds to the nearest 0.5
        return (float)roundedValue;
    }

    public static bool ValueIsBetween(float value, float min, float max)
    {
        if (value <= max && value >= min)
        {
            return true;
        }

        return false;
    }

}

