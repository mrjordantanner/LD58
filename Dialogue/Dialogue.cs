using DG.Tweening.Plugins;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Story/Dialogue")]
public class Dialogue : ScriptableObject
{
    public string speakerName;
    public SoundEffect audioClip;

    [TextArea(3, 10)]
    public string[] dialogueLines;


}

