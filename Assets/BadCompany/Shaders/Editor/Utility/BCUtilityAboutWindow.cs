using UnityEngine;
using UnityEditor;

namespace BadCompany.Shaders.Utility
{
    /// <summary>
    /// Bad Company Utility Shader About Window. Temporary implementation, upon PR2 will likely retrieve body content from a webserver, and will probably include an update-checker. 
    /// WIP!
    /// </summary>
    public class BCUtilityAboutWindow : EditorWindow
    {
        GUIStyle header;
        GUIStyle boldLabels;
        GUIStyle common;
        private bool setup = false;
        private int page = 0;
        private static BCUtilityAboutWindow window;

        public static void Init()
        {
            window = (BCUtilityAboutWindow)EditorWindow.GetWindow(typeof(BCUtilityAboutWindow));
            window.titleContent = new GUIContent("BCUtility - About");
            window.minSize = new Vector2(450f, 700f);
            window.Show();
        }

        private void SetupStyles()
        {
            //header
            header = new GUIStyle();
            header.alignment = TextAnchor.MiddleCenter;
            header.fontSize = 24;
            header.padding = new RectOffset(0, 0, 10, 10);


            //boldLabels
            boldLabels = new GUIStyle();
            boldLabels.alignment = TextAnchor.MiddleLeft;
            boldLabels.fontStyle = FontStyle.Bold;
            boldLabels.wordWrap = true;
            boldLabels.margin = new RectOffset(5, 5, 5, 5);
            boldLabels.padding = new RectOffset(5, 5, 5, 5);

            //common
            common = new GUIStyle();
            common.alignment = TextAnchor.MiddleLeft;
            common.wordWrap = true;
            common.padding = new RectOffset(5, 5, 2, 2);
            common.margin = new RectOffset(5, 5, 2, 2);

            setup = true;
        }

        private void OnGUI()
        {
            if (!setup) { SetupStyles(); }

            DrawHeader();

            switch (page)
            {
                case 0:
                    LandingPage();
                    break;
                case 1:
                    HelperPage();
                    break;
                case 2:
                    page = 0;
                    LandingPage();
                    break;
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Next Page"))
            {
                page++;
            }
            if (GUILayout.Button("Close"))
            {
                window.Close();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            GUILayout.Space(5f);
            GUILayout.Label("About the Utility Shader", header);
            GUILayout.Space(10f);
        }

        private void LandingPage()
        {
            GUILayout.Label("What is the purpose of the Utility Shader?", boldLabels);
            GUILayout.Space(4f);
            GUILayout.Label("The Bad Company Utility shader is designed for working with assets from and inspired by Microsoft's 'Halo' series.", common);
            GUILayout.Label("It supports armor from all the games, with a focus on pre-Halo 5 compatibility.", common);
            GUILayout.Space(4f);
            GUILayout.Label("The primary purpose of this shader is to enable working with a diverse collection of textures and meshes created in the style of Halo game assets.", common);
            GUILayout.Space(2f);
            GUILayout.Label("A quick disclaimer: This shader is provided to the public for free under the MIT License. If you purchased this package, you were ripped off. Please do not pay for free software.", common);
            GUILayout.Label("Bad Company Utility on GitHub: ", common);
            if(GUILayout.Button(new GUIContent("https://github.com/GentleLeviathan/BadCompanyUtility", "Opens the Utility Shader GitHub Repository.")))
            {
                OpenRepository();
            }

            GUILayout.Label("Metallic *and* Specular?", boldLabels);
            GUILayout.Space(4f);
            GUILayout.Label("Yes. Both properties are exposed only because they can be, not because they necessarily need to be adjusted.", common);
            GUILayout.Label("Generally the Specular Multiplier will be maxed, adjust as necessary.", common);
            GUILayout.Label("The 'Metallic' toggle allows the user to 'change' the specular highlight into what appears to be a metallic highlight.", common);
            GUILayout.Label("The 'Metal Modifier' slider controls the strength of 'metalness'. Without the 'Metallic' toggle on, this will not control the color of the specular highlight.", common);
            GUILayout.Space(4f);

            GUILayout.Label("Shader Features/Multi Compile?", boldLabels);
            GUILayout.Space(4f);
            GUILayout.Label("The shader is forced to use realtime property value checks in order to comply with a highly diverse shader environment.", common);
            GUILayout.Label("Local shader features are not available in Unity 2018.4.20f1 (the target version), but could easily be implemented.", common);
            GUILayout.Label("I may maintain two seperate versions of the shader, one implementing local shader features, and one without, at a later date.", common);
        }

        private void HelperPage()
        {
            GUILayout.Label("What is the purpose of the Helper Utilities?", boldLabels);
            GUILayout.Space(4f);
            GUILayout.Label("The Helper Utilities are a suite of tools to speed up the workflow and potentially improve the quality of game assets.", common);
            GUILayout.Label("You will find a short description of each helper utility by hovering over their buttons when inspecting a Utility material.", common);
            GUILayout.Space(4f);
            GUILayout.Label("Additional helper utilities will be implemented as the need for them arises.", common);
        }

        public static void OpenRepository()
        {
            Application.OpenURL("https://github.com/GentleLeviathan/BadCompanyUtility/releases");
        }
    }
}
