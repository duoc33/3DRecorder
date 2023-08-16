using Record;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 分配ViewID,包括场景中的预制件里的
    /// </summary>
    public class PrefabModifyWindow : EditorWindow
    {
        //private static Font toFont;
        //private static TMP_FontAsset tmFont;
        private static string PrefabLoadPath;
        private static int defaultSplitLength;
        private static RecordObjectLoadPathConfig RolpC;
        private static string CurrentSceneName;
        //private static Object UnityMonoComponent;

        [MenuItem("PrefabTool/ModifyPrefab")]
        public static void ShowWin()
        {
            defaultSplitLength = 17;
            RolpC = Resources.Load<RecordObjectLoadPathConfig>("RecorderConfig");
            PrefabLoadPath = "MyGameLogic";
            CurrentSceneName = "Test";
            EditorWindow.CreateInstance<PrefabModifyWindow>().Show();
        }
        private void OnGUI()
        {
            // dsa
            GUILayout.Space(10);
            PrefabLoadPath = EditorGUILayout.TextField("LoadPathDirectory: ", PrefabLoadPath);
            RolpC =
                (RecordObjectLoadPathConfig)EditorGUILayout.ObjectField(new GUIContent("PathConfig:"),
                RolpC, typeof(RecordObjectLoadPathConfig), true, GUILayout.MinWidth(100f));
            CurrentSceneName = EditorGUILayout.TextField("RecordingSceneName: ", CurrentSceneName);
            GUILayout.Space(10);
            if (GUILayout.Button("Generate RecordViewID"))
            {
                if (EditorSceneManager.GetActiveScene().name != CurrentSceneName)
                {
                    Debug.LogError("与需要记录的场景不匹配");
                    return;
                }
                if (RolpC.RecordObjects != null || RolpC.RecordObjects.Count > 0)
                {
                    RolpC.RecordObjects.Clear();
                }
                int index = 0;
                View[] views = GameObject.FindObjectsOfType<View>(true);
                if (views.Length > 0)
                {
                    AddReocrdObjectsInfo(views, string.Empty, ref index);
                }
                GeneratePrefabViewID(ref index);
            }
            AssetDatabase.SaveAssets();
        }
        private void GeneratePrefabViewID(ref int index)
        {
            Object[] allResources = Resources.LoadAll(PrefabLoadPath, typeof(Object));
            for (int i = 0; i < allResources.Length; i++)
            {
                if (allResources[i] is GameObject)
                {
                    View[] views = (allResources[i] as GameObject).GetComponentsInChildren<View>(true);
                    if (views.Length > 0)
                    {
                        string loadPath = GetLoadPath(allResources[i]);
                        AddReocrdObjectsInfo(views, loadPath, ref index);
                        EditorUtility.SetDirty(allResources[i]);
                    }
                }
            }
            EditorUtility.SetDirty(RolpC);
        }
        private string GetLoadPath(Object recordObject)
        {
            string temp = AssetDatabase.GetAssetPath(recordObject);
            temp = temp.Substring(defaultSplitLength);
            temp = temp.Split('.')[0];
            return temp;
        }
        private void AddReocrdObjectsInfo(View[] views, string loadPath, ref int index)
        {
            RecordObjectInfo recordObjectInfo = new RecordObjectInfo();
            recordObjectInfo.LoadPath = loadPath;
            foreach (View view in views)
            {
                recordObjectInfo.ViewIDs.Add(index);
                view.ID = index;
                EditorUtility.SetDirty(view);
                index++;
            }
            RolpC.RecordObjects.Add(recordObjectInfo);
        }
    }
}

