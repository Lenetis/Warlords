using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureUtilities
{
    /// Returns texture depending on the neighbouring tiles. If no neighbour condition has been matched, returns null
    public static Texture2D GetRelativeTexture(List<(Neighbours<string>, string)> relativeTextures, Neighbours<Tile> neighbouringTiles)
    {
        // todo remove code repetitions
        foreach (var tuple in relativeTextures) {
            Neighbours<string> neighboursCondition = tuple.Item1;
            string texturePath = tuple.Item2;

            bool valid = false;

            if (neighboursCondition.left != null) {
                if (neighbouringTiles.left != null && neighbouringTiles.left.tileTypes.Contains(neighboursCondition.left)) {
                    valid = true;
                } else {
                    continue;
                }
            }
            if (neighboursCondition.right != null) {
                if (neighbouringTiles.right != null && neighbouringTiles.right.tileTypes.Contains(neighboursCondition.right)) {
                    valid = true;
                } else {
                    continue;
                }
            }
            if (neighboursCondition.top != null) {
                if (neighbouringTiles.top != null && neighbouringTiles.top.tileTypes.Contains(neighboursCondition.top)) {
                    valid = true;
                } else {
                    continue;
                }
            }
            if (neighboursCondition.bottom != null) {
                if (neighbouringTiles.bottom != null && neighbouringTiles.bottom.tileTypes.Contains(neighboursCondition.bottom)) {
                    valid = true;
                } else {
                    continue;
                }
            }

            if (neighboursCondition.bottomLeft != null) {
                if (neighbouringTiles.bottomLeft != null && neighbouringTiles.bottomLeft.tileTypes.Contains(neighboursCondition.bottomLeft)) {
                    valid = true;
                } else {
                    continue;
                }
            }
            if (neighboursCondition.bottomRight != null) {
                if (neighbouringTiles.bottomRight != null && neighbouringTiles.bottomRight.tileTypes.Contains(neighboursCondition.bottomRight)) {
                    valid = true;
                } else {
                    continue;
                }
            }
            if (neighboursCondition.topLeft != null) {
                if (neighbouringTiles.topLeft != null && neighbouringTiles.topLeft.tileTypes.Contains(neighboursCondition.topLeft)) {
                    valid = true;
                } else {
                    continue;
                }
            }
            if (neighboursCondition.topRight != null) {
                if (neighbouringTiles.topRight != null && neighbouringTiles.topRight.tileTypes.Contains(neighboursCondition.topRight)) {
                    valid = true;
                } else {
                    continue;
                }
            }

            if (!valid) {
                continue;
            }

            return ResourceManager.LoadTexture(texturePath);
        }
        return null;
    }
}
