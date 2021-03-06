using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HikariImporter : AssetPostprocessor
{
    public bool IsActive = false;

    void OnPreprocessAsset()
    {
        if(!IsActive)
            return;
        TextureImporter texImporter = assetImporter as TextureImporter;

        if (texImporter != null)
        {
            TextureImporterSettings texSettings = new TextureImporterSettings();

            texImporter.ReadTextureSettings(texSettings);
            texSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            texImporter.SetTextureSettings(texSettings);
            texImporter.spritePivot = new Vector2(0.5f, 0.175f); //0.5f 0.175f

            Debug.Log("Asset Done : " + assetPath);
        }
    }
}
