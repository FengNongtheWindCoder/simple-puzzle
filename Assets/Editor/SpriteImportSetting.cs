/*********************************************************************************
 *Copyright(C) 2015 by FengNongtheWindCoder
 *All rights reserved.
 *FileName:     SpriteImportSetting.cs
 *Author:       FengNongtheWindCoder
 *Version:      1.0
 *UnityVersion：5.3.0f4
 *Date:         2015-12-26
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;

public class SpriteImportSetting : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter =  assetImporter as TextureImporter;
        textureImporter.mipmapEnabled = false;
    }
}