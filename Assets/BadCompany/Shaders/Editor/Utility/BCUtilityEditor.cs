using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;

/// <summary>
/// Bad Company Utility Shader Editor + UI. Contains a large number of options, features, and holds the interface for the included texture helpers.
/// Access it by creating a material with the Utility shader applied.
/// WIP!
/// </summary>

namespace BadCompany.Shaders.Utility
{
    enum UtilityBlendMode
    {
        Add, Subtract, Multiply, Divide
    }

    [CanEditMultipleObjects]
    public class BCUtilityEditor : ShaderGUI
    {
        private bool showIllumination = false;
        private bool showCustomTextures = false;
        private bool showDetailMaps = false;
        private bool showHelpers = false;
        private bool texFound = false;
        private bool performedUpdateCheck = false;
        private bool gotProperties = false;
        private bool suggestedTrueMetallic = false;
        private bool bypassLighting = true;

        private bool updateReady = false;

        private UtilityBlendMode detailBlendMode = UtilityBlendMode.Add;
        private Texture headerTex;

        private bool colorTexToggle = false;
        private bool simpleRoughnessToggle = false;
        private bool bungieRoughnessToggle = false;
        private bool isH5Toggle = false;
        private bool isH4Toggle = false;
        private bool isTrueMetallic = false;


        //properties
        MaterialProperty mainTex;
        MaterialProperty cc;
        MaterialProperty normal;
        MaterialProperty primaryColor;
        MaterialProperty secondaryColor;
        MaterialProperty tertiaryColor;
        MaterialProperty specMultiplier;
        MaterialProperty metallic;
        MaterialProperty roughness;
        MaterialProperty deepness;
        MaterialProperty simpleRoughness;
        MaterialProperty bungieRoughness;
        MaterialProperty isH5;
        MaterialProperty isH4;
        MaterialProperty trueMetallic;
        MaterialProperty colorTexture;
        MaterialProperty illum;
        MaterialProperty illuminationColor;
        MaterialProperty specTex;
        MaterialProperty metalTex;
        MaterialProperty roughTex;
        MaterialProperty parallaxTex;
        MaterialProperty reflectCube;
        MaterialProperty useCube;
        MaterialProperty occlusionTex;
        MaterialProperty detailTex;
        MaterialProperty detailNormal;
        MaterialProperty detailStrength;
        MaterialProperty parallaxStrength;


        private void InitTex()
        {
            headerTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/BadCompany/Shaders/Editor/Utility/Assets/BadCoUtilityHeader_PR1.png", typeof(Texture2D)) ?? new Texture2D(1, 1);
            texFound = true;
        }

        private async Task PerformUpdateCheck()
        {
            performedUpdateCheck = true;
            updateReady = await UtilityUpdateChecker.CheckForUpdates();
        }

        private void OnClosed()
        {
            texFound = false;
            performedUpdateCheck = false;
        }

        private void GetBooleanValues()
        {
            if (illum.textureValue != null)
            {
                showIllumination = true;
            }
            if (specTex.textureValue != null || roughTex.textureValue != null || occlusionTex.textureValue != null || metalTex.textureValue != null || reflectCube.textureValue != null || (parallaxTex.textureValue != null && parallaxStrength.floatValue > 0f))
            {
                showCustomTextures = true;
            }
            if (detailTex.textureValue != null || detailNormal.textureValue != null)
            {
                showDetailMaps = true;
            }
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (!texFound) { InitTex(); }

            if (!performedUpdateCheck) { PerformUpdateCheck(); }

            Material target = materialEditor.target as Material;

            //----------------------------------Properties-----------------------------------------
            try
            {
                GetProperties(materialEditor, properties, target);
                gotProperties = true;
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("BCUtilityEditor: " + e.Message);
                return;
            }
            if (gotProperties) { GetBooleanValues(); }

            //-----------------------------------DISPLAY-------------------------------------------
            DisplayCustomGUI(materialEditor, target);
        }

        private void GetProperties(MaterialEditor materialEditor, MaterialProperty[] properties, Material target)
        {
            //-----------Shader Variant
            switch(target.shader.name)
            {
                case "BadCompany/Bad Company Utility":
                    detailBlendMode = UtilityBlendMode.Add;
                    break;
                case "Hidden/BadCompany/Bad Company Utility (Detail Subtract)":
                    detailBlendMode = UtilityBlendMode.Subtract;
                    break;
                case "Hidden/BadCompany/Bad Company Utility (Detail Multiply)":
                    detailBlendMode = UtilityBlendMode.Multiply;
                    break;
                case "Hidden/BadCompany/Bad Company Utility (Detail Divide)":
                    detailBlendMode = UtilityBlendMode.Divide;
                    break;
            }

            //-----------Textures
            mainTex = ShaderGUI.FindProperty("_MainTex", properties);
            cc = ShaderGUI.FindProperty("_CC", properties);
            normal = ShaderGUI.FindProperty("_BumpMap", properties);

            //-----------Colors
            primaryColor = ShaderGUI.FindProperty("_Color", properties);
            secondaryColor = ShaderGUI.FindProperty("_SecondaryColor", properties);
            tertiaryColor = ShaderGUI.FindProperty("_TertiaryColor", properties);

            //-----------Values
            specMultiplier = ShaderGUI.FindProperty("_Specular", properties);
            metallic = ShaderGUI.FindProperty("_MetallicEffect", properties);
            roughness = ShaderGUI.FindProperty("_Roughness", properties);
            deepness = ShaderGUI.FindProperty("_Deepness", properties);

            //-----------Toggles
            simpleRoughness = ShaderGUI.FindProperty("_isSimpleRoughness", properties);
            bungieRoughness = ShaderGUI.FindProperty("_BungieRoughness", properties);
            isH5 = ShaderGUI.FindProperty("_isH5", properties);
            isH4 = ShaderGUI.FindProperty("_isH4", properties);
            trueMetallic = ShaderGUI.FindProperty("_trueMetallic", properties);
            colorTexture = ShaderGUI.FindProperty("_ColorTexture", properties);
            useCube = ShaderGUI.FindProperty("_cubemappedReflection", properties);

            //-----------Illum
            illum = ShaderGUI.FindProperty("_IllumTex", properties);
            illuminationColor = ShaderGUI.FindProperty("_IllumColor", properties);

            //-----------Custom Textures
            specTex = ShaderGUI.FindProperty("_SpecularTex", properties);
            metalTex = ShaderGUI.FindProperty("_MetallicTex", properties);
            roughTex = ShaderGUI.FindProperty("_RoughnessTex", properties);
            reflectCube = ShaderGUI.FindProperty("_CubeReflection", properties);
            occlusionTex = ShaderGUI.FindProperty("_Occlusion", properties);
            parallaxTex = ShaderGUI.FindProperty("_ParallaxMap", properties);
            parallaxStrength = ShaderGUI.FindProperty("_ParallaxStrength", properties);

            //-----------Detail Textures
            detailTex = ShaderGUI.FindProperty("_DetailMap", properties);
            detailNormal = ShaderGUI.FindProperty("_DetailNormal", properties);
            detailStrength = ShaderGUI.FindProperty("_DetailStrength", properties);
        }

        private void DisplayCustomGUI(MaterialEditor materialEditor, Material target)
        {
            GUILayout.Label(headerTex, GUILayout.MinWidth(240), GUILayout.MaxWidth(1500), GUILayout.MinHeight(190), GUILayout.MaxHeight(500));

            if (updateReady) { ReadyForUpdate(); }

            EditorGUI.BeginChangeCheck();

            //Textures
            GUILayout.Space(4f);
            GUILayout.Label("Main Textures", EditorStyles.boldLabel);
            materialEditor.TextureProperty(mainTex, "Diffuse", false);
            //desaturation toggle
            colorTexToggle = GUILayout.Toggle(colorTexture.floatValue > 0 ? true : false, MakeLabel("Color Texture?", "This checkbox desaturates the Diffuse texture, eliminating existing colors to improve color accuracy."));

            materialEditor.TextureProperty(cc, "Color Control (CC)", false);
            materialEditor.TextureProperty(normal, "Normal Map (DirectX)", false);
            materialEditor.TextureCompatibilityWarning(normal);
            materialEditor.TextureScaleOffsetProperty(mainTex);

            //Colors
            GUILayout.Space(4f);
            GUILayout.Label(MakeLabel("Main Colors", "Please note, black colors only appear properly with pre-343 textures."), EditorStyles.boldLabel);
            materialEditor.ColorProperty(primaryColor, "Primary Color (R)");
            materialEditor.ColorProperty(secondaryColor, "Secondary Color (G)");
            materialEditor.ColorProperty(tertiaryColor, "Tertiary Color (B)");

            //Values
            GUILayout.Space(4f);
            GUILayout.Label("Properties", EditorStyles.boldLabel);
            materialEditor.RangeProperty(specMultiplier, "Specular Multiplier");
            materialEditor.RangeProperty(metallic, "Metal Modifier");
            materialEditor.RangeProperty(roughness, "Roughness (1 - 0)");
            materialEditor.RangeProperty(deepness, "Color Depth");

            //Toggles
            GUILayout.Label("Toggles", EditorStyles.boldLabel);
            
            //New Toggles
            simpleRoughnessToggle = GUILayout.Toggle(simpleRoughness.floatValue > 0 ? true : false, MakeLabel("Use Simple Roughness?", "This checkbox forces the shader to use the Roughness property as the roughness value, with no additional calculations. (i.e., a roughness of 1 is no smoothness, roughness of 0.5 is half-smoothness)"));
            bungieRoughnessToggle = GUILayout.Toggle(bungieRoughness.floatValue > 0 ? true : false, MakeLabel("Bungie-era Alpha Roughness?", "This checkbox enables calculation of the roughness from the diffuse alpha. This is intended for pre-343 games only."));
            isH5Toggle = GUILayout.Toggle(isH5.floatValue > 0 ? true : false, MakeLabel("Halo 5 Armor??", "Enable this checkbox if the textures used are un-altered Halo 5 textures. This enables calculation of both roughness and specular from the color control texture."));
            isH4Toggle = GUILayout.Toggle(isH4.floatValue > 0 ? true : false, MakeLabel("Storm Armor? (H4)", "Enable this checkbox if the armor in question is ripped from Halo 4. This will remove your CC texture."));
            isTrueMetallic = GUILayout.Toggle(trueMetallic.floatValue > 0 ? true : false, MakeLabel("Metallic?", "Enable this checkbox to 'switch' from a Specular to a 'Metallic' highlight. This is will be enabled by default for Halo 4 and Halo 5 armors."));

            //Illumination
            if (GUILayout.Button(MakeLabel("Show Illumination", "Displays the Illumination section of the shader. This will always be visible if there are filled properties.")))
            {
                showIllumination = !showIllumination;
            }
            if (showIllumination)
            {
                GUILayout.Space(4f);
                GUILayout.Label("Illumination", EditorStyles.boldLabel);
                materialEditor.TexturePropertyWithHDRColor(MakeLabel("Texture  |  Color", "The black-and-white illumination texture (no alpha)"), illum, illuminationColor, false);
                if (GUILayout.Button(MakeLabel("Guess Illumination Color", "Attempts to create a neutral and appealing illumination color based on the chosen primary color. Loosely replicates behaviour in Halo 3."), GUILayout.Width(230f)))
                {
                    if (ValueTestApproximation(primaryColor.colorValue.r, primaryColor.colorValue.g, 0.025f) && ValueTestApproximation(primaryColor.colorValue.g, primaryColor.colorValue.b, 0.025f))
                    {
                        illuminationColor.colorValue = new Color(0.00392156863f * 82f, 0.00392156863f * 137f, 0.00392156863f * 215f) * 2f;
                    }
                    else
                    {
                        illuminationColor.colorValue = (primaryColor.colorValue * 1.25f) + (Color.white * 0.5f);
                    }
                }
                GUILayout.Space(4f);
            }

            //Custom Textures
            if (GUILayout.Button(MakeLabel("Show Custom Textures", "Displays the custom textures section (i.e., Specular Map, Roughness, Occlusion, etc) of the shader. This will always be visible if there are filled properties.")))
            {
                showCustomTextures = !showCustomTextures;
            }
            if (showCustomTextures)
            {
                GUILayout.Space(4f);
                GUILayout.Label("Custom Textures", EditorStyles.boldLabel);
                materialEditor.TextureProperty(specTex, "Specular Texture");
                materialEditor.TextureProperty(metalTex, "Metallic Texture");
                materialEditor.TextureProperty(roughTex, "Roughness Texture");
                materialEditor.TextureProperty(occlusionTex, "Occlusion Texture");
                materialEditor.TextureProperty(reflectCube, "Reflection Cubemap");
                materialEditor.TextureProperty(parallaxTex, "Height Map (R)", false);
                materialEditor.RangeProperty(parallaxStrength, "Height Strength");
                GUILayout.Space(4f);

                if(reflectCube.textureValue != null) { useCube.floatValue = 1; } else { useCube.floatValue = 0; }
            }

            //Detail maps
            if (GUILayout.Button(MakeLabel("Show Detail Maps", "Displays the detail map section of the shader. This will always be visible if there are filled properties.")))
            {
                showDetailMaps = !showDetailMaps;
            }
            if (showDetailMaps)
            {
                GUILayout.Space(4f);
                GUILayout.Label("Detail Maps", EditorStyles.boldLabel);
                materialEditor.TextureProperty(detailTex, "Detail Map", false);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Blend Mode", EditorStyles.label);
                GUILayout.Space(100f);
                GUILayout.FlexibleSpace();
                detailBlendMode = (UtilityBlendMode)EditorGUILayout.EnumPopup(detailBlendMode);
                GUILayout.EndHorizontal();

                materialEditor.TextureProperty(detailNormal, "Detail Normal (OpenGL)", false);
                materialEditor.RangeProperty(detailStrength, "Strength");
                materialEditor.TextureScaleOffsetProperty(detailTex);
            }

            //Detail maps
            if (GUILayout.Button(MakeLabel("Show Helpers", "Displays additional tools and helper functions included with the Utility shader.")))
            {
                showHelpers = !showHelpers;
            }
            if (showHelpers)
            {
                GUILayout.Space(4f);
                GUILayout.Label("Helpers and Utilities", EditorStyles.boldLabel);
                GUILayout.Space(4f);

                //DECOMPOSERS
                GUILayout.Label("Game Texture Decomposers", EditorStyles.label);
                GUILayout.Space(2f);
                if (GUILayout.Button(new GUIContent("Decompose Storm Textures", "Decomposes assigned Halo 4 'Storm' textures into traditional diffuse, metallic/specular ('technically' your choice), and Halo 5-compatible color control textures."), GUILayout.Width(250f)) && isH4.floatValue == 1)
                {
                    //run the storm decompose
                    StormDecomposer.DecomposeStormTexture(mainTex.textureValue as Texture2D, mainTex.textureValue as UnityEngine.Object);
                }
                GUILayout.Space(4f);
                if (GUILayout.Button(new GUIContent("Decompose Halo 5 Textures", "Decomposes assigned Halo 5 Forge textures into Bungie-era Diffuse RGB + Roughness Alpha, and Bungie-style color control textures."), GUILayout.Width(250f)) && isH5.floatValue == 1)
                {
                    //run the fforge decompose
                    FForgeDecomposer.DecomposeForgeTextures(mainTex.textureValue as UnityEngine.Object, cc.textureValue as UnityEngine.Object);
                }
                GUILayout.Space(4f);

                //OTHER UTILITIES
                GUILayout.Label("Other Utilities", EditorStyles.label);
                GUILayout.Space(4f);
                if (GUILayout.Button(new GUIContent("DX2GL Normal Conversion", "Creates and applies an OpenGL Normal Map from an assigned DirectX Normal or vice-versa."), GUILayout.Width(250f)))
                {
                    //run the opengl/dx conversion
                    //apply the result as the assigned texture
                    normal.textureValue = DX2GLNormalConverter.ConvertToDXOGL(normal.textureValue as Texture2D, normal.textureValue as UnityEngine.Object);
                }
                GUILayout.Space(4f);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Bake Material", "Bakes the material's output into a texture, optionally including environment lighting."), GUILayout.Width(250f)))
                {
                    //run the utilitybaker process
                    UtilityBaker.BakeMaterialAsTexture((Material)materialEditor.target, bypassLighting);
                }
                bypassLighting = GUILayout.Toggle(bypassLighting, MakeLabel("Lighting Bypass", "Bypasses the light model of the material, making it unlit."));
                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }

            CopyScaleOffsetMain();
            CopyScaleOffsetSecondary();

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.PropertiesChanged();

                gotProperties = false;
            }

            if (GUILayout.Button(new GUIContent("About...", "Displays the About pop-up for the shader, including a short FAQ and links to documentation.")))
            {
                //display about Window
                BCUtilityAboutWindow.Init();
            }

            ApplyShaderVariantsToMaterial(target);
            ApplyShaderFeatures(target);
            ApplyToggleValues(target);
        }

        private void ReadyForUpdate()
        {
            if (!updateReady) { return; }

            GUILayout.Label("An update is available!", EditorStyles.boldLabel);
            if(GUILayout.Button(MakeLabel("Open Repository", "Opens the Utility Shader GitHub Repository")))
            {
                BCUtilityAboutWindow.OpenRepository();
            }
        }

        private void CopyScaleOffsetMain()
        {
            cc.textureScaleAndOffset = mainTex.textureScaleAndOffset;
            normal.textureScaleAndOffset = mainTex.textureScaleAndOffset;
        }

        private void CopyScaleOffsetSecondary()
        {
            detailNormal.textureScaleAndOffset = detailTex.textureScaleAndOffset;
        }

        private void ApplyShaderVariantsToMaterial(Material mat)
        {
            switch (detailBlendMode)
            {
                case UtilityBlendMode.Add:
                    mat.shader = Shader.Find("BadCompany/Bad Company Utility");
                    break;
                case UtilityBlendMode.Subtract:
                    mat.shader = Shader.Find("Hidden/BadCompany/Bad Company Utility (Detail Subtract)");
                    break;
                case UtilityBlendMode.Multiply:
                    mat.shader = Shader.Find("Hidden/BadCompany/Bad Company Utility (Detail Multiply)");
                    break;
                case UtilityBlendMode.Divide:
                    mat.shader = Shader.Find("Hidden/BadCompany/Bad Company Utility (Detail Divide)");
                    break;
            }
        }

        private void ApplyShaderFeatures(Material mat)
        {
            if (parallaxTex.textureValue != null && parallaxStrength.floatValue > 0f) { mat.EnableKeyword("PARALLAX_ON"); } else { mat.DisableKeyword("PARALLAX_ON"); }
        }

        private void ApplyToggleValues(Material mat)
        {
            if (colorTexToggle) { colorTexture.floatValue = 1f; } else { colorTexture.floatValue = 0f; }
            if (simpleRoughnessToggle) { simpleRoughness.floatValue = 1f; }  else { simpleRoughness.floatValue = 0f; }
            if (bungieRoughnessToggle) { bungieRoughness.floatValue = 1f; } else { bungieRoughness.floatValue = 0f; }

            if (isH5Toggle)
            {
                isH5.floatValue = 1f;
                if (!suggestedTrueMetallic)
                {
                    isTrueMetallic = true;
                    suggestedTrueMetallic = true;
                }
            } else { isH5.floatValue = 0f; suggestedTrueMetallic = false; }

            if (isH4Toggle)
            {
                colorTexToggle = true;
                cc.textureValue = null;
                isH4.floatValue = 1f;
                if (!suggestedTrueMetallic)
                {
                    isTrueMetallic = true;
                    suggestedTrueMetallic = true;
                }
            } else { isH4.floatValue = 0f; suggestedTrueMetallic = false; }

            if (isTrueMetallic)
            {
                trueMetallic.floatValue = 1f;
            } else
            {
                trueMetallic.floatValue = 0f;
            }
        }

        private GUIContent MakeLabel(string displayName, string tooltip = null)
        {
            GUIContent staticLabel = new GUIContent();
            staticLabel.text = displayName;
            staticLabel.tooltip = tooltip;
            return staticLabel;
        }

        /// <summary>
        /// Tests if two float values are within a tolerance of each other.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="difference"></param>
        /// <returns></returns>
        private bool ValueTestApproximation(float a, float b, float difference)
        {
            return (Mathf.Abs(a - b) < difference);
        }

        private void temp(Texture2D texture)
        {
            Texture2D toBecomeHeightmap = new Texture2D(texture.width, texture.height, TextureFormat.R16, false);

            Color32[] newColors = new Color32[texture.width * texture.height];

            for (int x = 0; x < toBecomeHeightmap.width; x++)
            {
                for (int y = 0; y < toBecomeHeightmap.height; y++)
                {
                    newColors[x + (y * toBecomeHeightmap.width)] = texture.GetPixel(x, y);
                }
            }

            toBecomeHeightmap.SetPixels32(newColors);
            toBecomeHeightmap.Apply();
        }
    }
}
