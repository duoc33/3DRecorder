using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

namespace Record
{
    /// <summary>
    /// 回放对象合集及它们实例次数相关数据的记录中心、并进行文件创建等
    /// </summary>
    public class RecorderDataCenter : AbstractModel
    {
        /// <summary>
        /// 读取数据的方法类
        /// </summary>
        private DataReadUtility mDataReadUtility;
        /// <summary>
        /// 写入数据的方法，定义了数据存储格式
        /// </summary>
        private DataWriteUtility mDataWriteUtility;
        /// <summary>
        /// 配置表
        /// </summary>
        private RecordObjectLoadPathConfig pathConfig;
        protected override void OnInit()
        {
            mDataReadUtility = this.GetUtility<DataReadUtility>();
            mDataWriteUtility = this.GetUtility<DataWriteUtility>();
        }

        #region RecordingMode
        /// <summary>
        /// 当前场景所有记录对象，内部有可能已经被销毁了
        /// </summary>
        public Dictionary<int, List<ViewInfoInRecording>> RecordModeDic => RecordingModeDic;
        
        /// <summary>
        /// 记录模式下记录当前物体的个数
        /// </summary>
        private Dictionary<int, List<ViewInfoInRecording> > RecordingModeDic = new Dictionary<int, List<ViewInfoInRecording>>();
        
        /// <summary>
        /// 记录当前场景中的回放对象，获得他们的实例化次数，初始化它们的数据文件
        /// </summary>
        /// <param name="baseView">回放对象特性</param>
        public ViewInfoInRecording AddBaseViewOnRecording(RecordObjectView baseView)
        {
            //记录那个对象实例出来的
            int temBeInstantiatedID = baseView.InstantiatedID;
            if (RecordingModeDic.ContainsKey(baseView.ViewID))
            {
                ViewInfoInRecording bvc = new ViewInfoInRecording();
                int instantiatedID = RecordingModeDic[baseView.ViewID].Last().InstantiatedID;
                instantiatedID++;
                bvc.InstantiatedID = instantiatedID;
                baseView.InstantiatedID = bvc.InstantiatedID;
                bvc.View = baseView;
                baseView.ObjectSavePath = baseView.SavePath + "_" + bvc.InstantiatedID.ToString() + "_" + temBeInstantiatedID.ToString() + ".rd";
                mDataWriteUtility.InitFile(baseView.ObjectSavePath);
                RecordingModeDic[baseView.ViewID].Add(bvc);
                return bvc;
            }
            else
            {
                ViewInfoInRecording bvc = new ViewInfoInRecording() { View = baseView, InstantiatedID = 1 };
                baseView.InstantiatedID = bvc.InstantiatedID;
                RecordingModeDic[baseView.ViewID] = new List<ViewInfoInRecording>();
                baseView.ObjectSavePath = baseView.SavePath + "_" + bvc.InstantiatedID.ToString() + "_" + temBeInstantiatedID.ToString() + ".rd"; ;
                mDataWriteUtility.InitFile(baseView.ObjectSavePath);
                RecordingModeDic[baseView.ViewID].Add(bvc);
                return bvc;
            }
        }
        #endregion

        #region WatchingMode
        /// <summary>
        /// 观看模式下，供外部使用的获取/查找回放数据的中心(联合查询器)
        /// </summary>
        public RecorderDataReader RecorderReader => mDataReader;
        /// <summary>
        /// 联合查询器
        /// </summary>
        private RecorderDataReader mDataReader;

        /// <summary>
        /// 解析文件，获得联合查询表
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public IEnumerator ParseFileAndScriptableObjectConfig(string directoryPath) 
        {
            mDataReader = new RecorderDataReader();
            yield return mDataReadUtility.ReadFileConfigTable(directoryPath,mDataReader);
            pathConfig =Object.Instantiate(Resources.Load<RecordObjectLoadPathConfig>("RecorderConfig"));
            yield return ParseScriptableObjectConfig(pathConfig);
            
        }
        /// <summary>
        /// 根据路径配置 再初始化 联合查询器
        /// </summary>
        /// <param name="pathConfig"></param>
        /// <returns></returns>
        private IEnumerator ParseScriptableObjectConfig(RecordObjectLoadPathConfig pathConfig) 
        {
            foreach (RecordObjectInfo roi in pathConfig.RecordObjects)
            {
                if (roi.LoadPath.Equals(string.Empty))
                {
                    RecordObjectView[] Views = GameObject.FindObjectsOfType<RecordObjectView>();
                    foreach (var view in Views)
                    {
                        if (!roi.ViewIDIsBelong(view.ViewID)) { 
                            Debug.LogError("配置与当前场景中存在的ViewID是不匹配的,更改预制件请重新配置一下");
                            yield break;
                        }
                        if (mDataReader.ViewID2SOI.Get(view.ViewID).Count() <= 0) continue;
                        //LoadPath为null的在场景中就加了 RecordObjectView，所以可以先配置InstantiatedID为1的RecordObjectView
                        foreach (SingleObjectInfo mySOI in mDataReader.ViewID2SOI.Get(view.ViewID))
                        {
                            mySOI.LoadPath = string.Empty;
                            if (mySOI.BeInstantiatedID==0){
                                mySOI.SingleView = view;
                                mySOI.SingleView.InstantiatedID = mySOI.InstantiatedID;
                                mySOI.SingleView.ObjectSavePath = mySOI.ReadPath;
                                mySOI.SingleView.FileLength = mySOI.StreamLength;
                            }
                        }
                    }
                }
                else
                {
                    roi.ViewIDs.ForEach(id =>
                    {
                        foreach (SingleObjectInfo item in mDataReader.ViewID2SOI.Get(id))
                        {
                            if (item.BeInstantiatedID == 0)
                            {
                                item.LoadPath = roi.LoadPath;
                            }
                        }
                    });
                }
                yield return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// 录制模式中的结构
    /// </summary>
    public class ViewInfoInRecording
    {
        /// <summary>
        /// 回放组件View
        /// </summary>
        public RecordObjectView View;
        /// <summary>
        /// 实例ID，ViewID对象clone实例化出来的ID，如果场景中它是第一个该ID默认为0
        /// </summary>
        public int InstantiatedID;
        /// <summary>
        /// 写入该对象数据的协程,方便统一销魂
        /// </summary>
        public Coroutine WriteCoroutine;
    }
}

