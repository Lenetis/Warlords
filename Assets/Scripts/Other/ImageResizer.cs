using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ImageResizer
{
    // adapted from http://answers.unity.com/answers/890986/view.html
    public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight) {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        for (int i = 0; i < result.height; ++i) {
            for (int j = 0; j < result.width; ++j) {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.filterMode = FilterMode.Point;
        result.Apply();
        return result;
    }
}
