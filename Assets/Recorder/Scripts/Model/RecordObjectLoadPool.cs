using DG.Tweening;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;
using static UnityEditor.Progress;

namespace Record
{
    /// <summary>
    /// 回放对象合集及它们实例次数相关数据的记录、并进行文件创建等
    /// </summary>
    public class RecordObjectLoadPool : AbstractModel
    {
        private PathModel mPathModel;
        private DataReadUtility mDataReadUtility;
        private DataWriteUtility mDataWriteUtility;

        private RcorderDataReader mDataReader;
        private RecordObjectLoadPathConfig pathConfig;
        protected override void OnInit()
        {
            mDataReadUtility = this.GetUtility<DataReadUtility>();
            mDataWriteUtility = this.GetUtility<DataWriteUtility>();
        }
        #region RecordingMode
        private Dictionary<int, BaseViewCount> RecordingModeDic = new Dictionary<int, BaseViewCount>();
        /// <summary>
        /// 记录当前场景中的回放对象，获得他们的实例化次数，初始化它们的数据文件
        /// </summary>
        /// <param name="baseView">回放对象特性</param>
        public void AddBaseViewOnRecording(RecordObjectView baseView)
        {
            if (RecordingModeDic.ContainsKey(baseView.ViewID))
            {
                BaseViewCount bvc = RecordingModeDic[baseView.ViewID];
                bvc.MViewCount++;
                baseView.InstantiatedID = bvc.MViewCount;
                RecordingModeDic[baseView.ViewID] = bvc;
                baseView.ObjectSavePath = baseView.SavePath + "_" + bvc.MViewCount.ToString() + ".rd";
                mDataWriteUtility.WriteData(baseView.ObjectSavePath, null);
            }
            else
            {
                BaseViewCount bvc = new BaseViewCount() { MBaseView = baseView, MViewCount = 1 };
                baseView.InstantiatedID = bvc.MViewCount;
                RecordingModeDic[baseView.ViewID] = bvc;
                baseView.ObjectSavePath = baseView.SavePath + "_" + bvc.MViewCount.ToString() + ".rd";
                mDataWriteUtility.WriteData(baseView.ObjectSavePath, null);
            }
        }
        #endregion

        #region WatchingMode
        /// <summary>
        /// 获取时间轴
        /// </summary>
        public List<int> TimeAxis => mTimeAxis;
        private List<int> mTimeAxis;
        /// <summary>
        /// 时间节点和
        /// </summary>
        private Dictionary<int,List<ViewIDToIntantiatedID>> WatchingTimeToID = new Dictionary<int, List<ViewIDToIntantiatedID>>();
        private Dictionary<int,RecordObjectView> WatchingModeDic = new Dictionary<int, RecordObjectView>();
        /// <summary>
        /// 解析路劲配置和实例时间节点
        /// </summary>
        /// <returns></returns>
        public IEnumerator ParsePathConfig() 
        {
            if (Recorder.Instance == null) {
                Debug.LogError("Recorder 没有实例");
                yield break;
            }
            mDataReader = new RcorderDataReader();
            WatchingTimeToID = mDataReadUtility.ReadAllFileName(Recorder.Instance.SavePath);
            yield return WatchingTimeToID;
            mTimeAxis = WatchingTimeToID.Keys.ToList();
            mTimeAxis.Sort();
            pathConfig =Object.Instantiate(Resources.Load<RecordObjectLoadPathConfig>("RecordConfig"));
            yield return pathConfig;
            //foreach (RecordObjectInfo roi in pathConfig.RecordObjects)
            //{
            //    if (roi.LoadPath.Equals(string.Empty)) {
            //        RecordObjectView[] Views = GameObject.FindObjectsOfType<RecordObjectView>();
            //        foreach (var item in Views)
            //        {
            //            if(roi.ViewIDIsBelong(item.ViewID)) 
            //            {
            //                mDataReader.Add(new SingleObjectInfo() { ViewID = item.ViewID,InstantiatedID = 1,SingleView = item, LoadPath = string.Empty });
            //            }
            //            else
            //            {
            //                Debug.LogError("配置表与当前场景中的View组件数量或者ID不对应，请在PrefabeTool编辑器工具里按一下配置当前当前场景，注意与场景名称保持一致");
            //                //Debug.LogException(new System.Exception());
            //            }
            //        }
            //        continue; 
            //    }
            //    roi.ViewIDs.ForEach(id => {
            //        mDataReader.Add(new SingleObjectInfo() { ViewID = id, InstantiatedID = 1, LoadPath = roi.LoadPath });
            //    });
            //}
        }
        /// <summary>
        /// 重新获取PathConfig
        /// </summary>
        public void PathConfigReset() { pathConfig = Object.Instantiate(Resources.Load<RecordObjectLoadPathConfig>("RecordConfig")); }
        /// <summary>
        /// 根据时间节点实例对应的对象
        /// </summary>
        /// <param name="TimeNode"></param>
        public void RecorderInWatchingInstantiated(int TimeNode) 
        {
            foreach (ViewIDToIntantiatedID ViewInfo in WatchingTimeToID[TimeNode])
            {
                if (WatchingModeDic.ContainsKey(ViewInfo.ID)) {
                    RecordObjectView view = Object.Instantiate(WatchingModeDic[ViewInfo.ID]);
                    view.InstantiatedID = ViewInfo.IntantiatedIndex;
                    view.ObjectSavePath = ViewInfo.Path;
                    view.FileLength = ViewInfo.StreamLength;
                }
                else
                {
                    foreach (var item in pathConfig.RecordObjects)
                    {
                        if (item.ViewIDIsBelong(ViewInfo.ID)) {
                            if (item.LoadPath.Equals(string.Empty)) {
                                RecordObjectView[] recordObjectView =  GameObject.FindObjectsOfType<RecordObjectView>(true);
                                InitRecordObjectViewByViewInfo(recordObjectView,ViewInfo);
                            }
                            else
                            {
                                GameObject go = Object.Instantiate(Resources.Load<GameObject>(item.LoadPath));
                                RecordObjectView[] views = go.GetComponentsInChildren<RecordObjectView>(true);
                                InitRecordObjectViewByViewInfo(views, ViewInfo);
                            }
                            pathConfig.RecordObjects.Remove(item);
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 通过从文件中读取获得ViewIDToIntantiatedID的值，付给RecordObjectView
        /// </summary>
        /// <param name="views"></param>
        /// <param name="ViewInfo"></param>
        private void InitRecordObjectViewByViewInfo(RecordObjectView[] views, ViewIDToIntantiatedID ViewInfo)
        {
            foreach (var view in views)
            {
                if (WatchingModeDic.ContainsKey(view.ViewID)) continue;
                if (view.ViewID == ViewInfo.ID)
                {
                    view.InstantiatedID = ViewInfo.IntantiatedIndex;
                    view.ObjectSavePath = ViewInfo.Path;
                    view.FileLength = ViewInfo.StreamLength;
                    WatchingModeDic[view.ViewID] = view;
                }
                else
                {
                    WatchingModeDic[view.ViewID] = view;
                }
            }
        }
        #endregion



        #region DataStruct
        /// <summary>
        /// BaseView和它场景中实例化的次数MViewCount
        /// </summary>
        public struct BaseViewCount
        {
            public RecordObjectView MBaseView;
            public int MViewCount;
        }
        /// <summary>
        /// 硬盘文件中的ViewID对应的IntantiatedID 
        /// </summary>
        public struct ViewIDToIntantiatedID
        {
            /// <summary>
            /// ViewID
            /// </summary>
            public int ID;
            /// <summary>
            /// IntantiatedID
            /// </summary>
            public int IntantiatedIndex;
            /// <summary>
            /// 文件路径
            /// </summary>
            public string Path;
            /// <summary>
            /// 文件流长度
            /// </summary>
            public long StreamLength;
            /// <summary>
            /// 实例化时间
            /// </summary>
            public int InstantiatedTime;
            /// <summary>
            /// 销毁时间
            /// </summary>
            public int DestoryedTime;
        }
        #endregion

        #region 联合查询
        /// <summary>
        /// 读取时所需的单个对象信息
        /// </summary>
        public class SingleObjectInfo
        {
            /// <summary>
            /// ViewID 记录标识
            /// </summary>
            public int ViewID { get; set; }
            /// <summary>
            /// 实例ID 当前被Clone实例出来的第几个
            /// </summary>
            public int InstantiatedID { get; set; }
            /// <summary>
            /// RecordObjectView 本身,通过它获取对象的其他组件信息
            /// </summary>
            public RecordObjectView SingleView { get; set; }
            /// <summary>
            /// 被实例化进场景的时间
            /// </summary>
            public int InstantiatedTime { get; set; }
            /// <summary>
            /// 销毁的时间节点
            /// </summary>
            public int DestoryedTime { get; set; }
            /// <summary>
            /// 数据读取路径
            /// </summary>
            public string ReadPath { get; set; }
            /// <summary>
            /// 文件流大小长度
            /// </summary>
            public long StreamLength { get; set; }
            /// <summary>
            /// 对象加载路径
            /// </summary>
            public string LoadPath { get; set; }
        }
        /// <summary>
        /// 数据读取器，联合查询器
        /// </summary>
        public class RcorderDataReader : Table<SingleObjectInfo>
        {
            /// <summary>
            /// 根据ViewID找SOI
            /// </summary>
            public TableIndex<int, SingleObjectInfo> ViewID2SOI = 
                new TableIndex<int, SingleObjectInfo>((SOI) => SOI.ViewID);
            /// <summary>
            /// 根据ReadPath找SOI
            /// </summary>
            public TableIndex<string, SingleObjectInfo> ReadPath2SOI =
                new TableIndex<string, SingleObjectInfo>((SOI) => SOI.ReadPath);
            /// <summary>
            /// 根据LoadPath找SOI
            /// </summary>
            public TableIndex<string, SingleObjectInfo> LoadPath2SOI =
                new TableIndex<string, SingleObjectInfo>((SOI)=>SOI.LoadPath);
            /// <summary>
            /// 根据InstantiatedTime找SOI
            /// </summary>
            public TableIndex<int, SingleObjectInfo> InstantiatedTime2SOI =
                new TableIndex<int, SingleObjectInfo>((SOI)=>SOI.InstantiatedTime);
            /// <summary>
            /// 根据DestoryedTime找SOI
            /// </summary>
            public TableIndex<int, SingleObjectInfo> DestoryedTime2SOI =
                new TableIndex<int, SingleObjectInfo>((SOI)=>SOI.DestoryedTime);
            public override IEnumerator<SingleObjectInfo> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnAdd(SingleObjectInfo item)
            {
                throw new System.NotImplementedException();
            }

            protected override void OnClear()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnDispose()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnRemove(SingleObjectInfo item)
            {
                throw new System.NotImplementedException();
            }
        }
        #endregion

    }
}

