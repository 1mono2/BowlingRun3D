#if UNITY_IOS
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEditor;
using UnityEditor.Callbacks;

namespace iOS.Editor
{
    public class PostBuildProcessForIosAtt
    {
        private const string ATT_FRAMEWORK = "AppTrackingTransparency.framework";
        
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
            {
                return;
            }
            // pbxに AppTrackingTransparency.framework を追加する
            var pbxPath = PBXProject.GetPBXProjectPath(buildPath);
            var pbx = new PBXProject();
            pbx.ReadFromFile(pbxPath);
            string target = GetUnityMainTargetGuidWithCompatible(pbx);
            pbx.AddFrameworkToProject(target, ATT_FRAMEWORK, true);
            pbx.WriteToFile(pbxPath);

            // Info.plist に Privacy - Tacking Usage Description(NSUserTrackingUsageDescription)を追加する
            var path  = buildPath + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromFile(path);
            var root = plist.root;
            root.SetString( "NSUserTrackingUsageDescription", "好みに合わせた広告を表示するために使用されます。");
            plist.WriteToFile(path);
        }

        private static string GetUnityMainTargetGuidWithCompatible(PBXProject pbx)
        {
#if UNITY_2019_3_OR_NEWER
            return pbx.GetUnityFrameworkTargetGuid();
#else
            return pbx.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif
        }
    }
}
#endif
