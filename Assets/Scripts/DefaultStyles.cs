using UnityEngine;
using Google.Maps.Feature.Style;
using Google.Maps;

public static class DefaultStyles
{

    public static GameObjectOptions getDefaultStyles()
    {
        Shader standardShader = Shader.Find("Google/Maps/Shaders/Standard");

        if (standardShader == null)
        {
            // Try to find the Unity Standard Shader as a backup.
            standardShader = Shader.Find("Standard");

            if (standardShader == null)
            {
                // Try to find the Legacy Diffuse Shader as a backup-backup.
                standardShader = Shader.Find("Diffuse");
            }

            if (standardShader == null)
            {
                return null;
            }
        }

        // Find BaseMaps Shader. Note that this Shader does not have a backup, as it has unique
        // behaviour needed for BaseMap level geometry to show in the correct render order.
        Shader baseMapShader = Shader.Find("Google/Maps/Shaders/BaseMap Color");

        if (baseMapShader == null)
        {
            return null;
        }

        // Create default materials for use by buildings, as well as other materials for use by water,
        // ground, roads, etc.
        // Material wallMaterial = new Material(standardShader) { color = new Color(1f, 0.75f, 0.5f) };
        Material wallMaterial = Resources.Load("DummyMaterials/dummyMaterial") as Material;

        Material roofMaterial = new Material(standardShader) { color = new Color(1f, 0.8f, 0.6f) };   

        Material regionMaterial = new Material(baseMapShader)
        {
            color = new Color(0.5f, 0.7f, 0.5f),
        };
        regionMaterial.SetFloat("_Glossiness", 1f);

        Material waterMaterial = new Material(baseMapShader)
        {
            color = new Color(0.0f, 1.0f, 1.0f),
        };
        waterMaterial.SetFloat("_Glossiness", 1f);

        /*
        Material segmentMaterial = new Material(baseMapShader)
        {
            color = new Color(0.5f, 0.5f, 0.5f),
        };
        segmentMaterial.SetFloat("_Glossiness", 0.5f);
        */
        Material segmentMaterial = Resources.Load("DummyMaterials/dummyMaterialRoad") as Material;

        Material intersectionMaterial = new Material(baseMapShader)
        {
            color = new Color(0.4f, 0.4f, 0.4f),
        };
        intersectionMaterial.SetFloat("_Glossiness", 0.5f);

        // Create style for buildings made from extruded shapes (most buildings).
        ExtrudedStructureStyle extrudedStructureStyle =
            new ExtrudedStructureStyle
                .Builder
            { WallMaterial = wallMaterial, RoofMaterial = roofMaterial }
                .Build();

        // Create style for buildings with detailed vertex/triangle data (such as the Statue of
        // Liberty).
        ModeledStructureStyle modeledStructureStyle =
            new ModeledStructureStyle.Builder { Material = wallMaterial }.Build();

        // Create style for regions (such as parks).
        RegionStyle regionStyle = new RegionStyle.Builder { FillMaterial = regionMaterial }.Build();

        // Create style for bodies of water (such as oceans).
        AreaWaterStyle areaWaterStyle =
            new AreaWaterStyle.Builder { FillMaterial = waterMaterial }.Build();

        // Create style for lines of water (such as narrow rivers).
        LineWaterStyle lineWaterStyle =
            new LineWaterStyle.Builder { Material = waterMaterial }.Build();

        // Create style for segments (such as roads).
        SegmentStyle segmentStyle =
            new SegmentStyle.Builder
            {
                Material = segmentMaterial,
                IntersectionMaterial = intersectionMaterial,
                Width = 7.0f
            }.Build();

        // Collect styles into a form that can be given to map loading function.
        return new GameObjectOptions
        {
            ExtrudedStructureStyle = extrudedStructureStyle,
            ModeledStructureStyle = modeledStructureStyle,
            RegionStyle = regionStyle,
            AreaWaterStyle = areaWaterStyle,
            LineWaterStyle = lineWaterStyle,
            SegmentStyle = segmentStyle,
        };
    }
}
