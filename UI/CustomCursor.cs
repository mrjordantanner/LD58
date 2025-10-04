using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = Input.mousePosition;
    }



    void OnGUI()
    {
        GUI.DrawTexture(new Rect(Input.mousePosition.x - hotspot.x, Screen.height - Input.mousePosition.y - hotspot.y, cursorTexture.width, cursorTexture.height), cursorTexture);
    }
}
