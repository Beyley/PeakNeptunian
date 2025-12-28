using TMPro;
using UnityEngine;

namespace PeakNeptunian;

public class FontCacheComponent : MonoBehaviour
{
    public bool cached;
    public TMP_FontAsset? font;
    public float lineSpacing;
    public FontStyles fontStyle;
}