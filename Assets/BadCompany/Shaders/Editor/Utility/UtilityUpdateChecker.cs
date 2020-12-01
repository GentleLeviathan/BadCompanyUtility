using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BadCompany.Shaders.Utility
{
    public static class UtilityUpdateChecker
    {
        public static string currentVersion = "V.1.0.0.1130.U-PR1.1";

        public static async Task<bool> CheckForUpdates()
        {
            UnityWebRequest www = UnityWebRequest.Get("http://raw.githubusercontent.com/GentleLeviathan/BadCompanyUtility/main/masterVersion");
            DownloadHandler handler = www.downloadHandler;
            UnityWebRequestAsyncOperation op = www.SendWebRequest();

            while (www.downloadProgress < 1.0f)
            {
                await Task.Delay(100);
            }
            if (www.isHttpError)
            {
                Debug.Log("BadCompanyUtility - There was an error checking for an update. - " + www.error);
                return false;
            }

            string masterVersion = handler.text;
            return !masterVersion.Contains(currentVersion);
        }
    }
}
