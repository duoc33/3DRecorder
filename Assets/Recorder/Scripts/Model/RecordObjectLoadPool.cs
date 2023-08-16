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
    /// �طŶ���ϼ�������ʵ������������ݵļ�¼���������ļ�������
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
        /// ��¼��ǰ�����еĻطŶ��󣬻�����ǵ�ʵ������������ʼ�����ǵ������ļ�
        /// </summary>
        /// <param name="baseView">�طŶ�������</param>
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
        /// ��ȡʱ����
        /// </summary>
        public List<int> TimeAxis => mTimeAxis;
        private List<int> mTimeAxis;
        /// <summary>
        /// ʱ��ڵ��
        /// </summary>
        private Dictionary<int,List<ViewIDToIntantiatedID>> WatchingTimeToID = new Dictionary<int, List<ViewIDToIntantiatedID>>();
        private Dictionary<int,RecordObjectView> WatchingModeDic = new Dictionary<int, RecordObjectView>();
        /// <summary>
        /// ����·�����ú�ʵ��ʱ��ڵ�
        /// </summary>
        /// <returns></returns>
        public IEnumerator ParsePathConfig() 
        {
            if (Recorder.Instance == null) {
                Debug.LogError("Recorder û��ʵ��");
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
            //                Debug.LogError("���ñ��뵱ǰ�����е�View�����������ID����Ӧ������PrefabeTool�༭�������ﰴһ�����õ�ǰ��ǰ������ע���볡�����Ʊ���һ��");
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
        /// ���»�ȡPathConfig
        /// </summary>
        public void PathConfigReset() { pathConfig = Object.Instantiate(Resources.Load<RecordObjectLoadPathConfig>("RecordConfig")); }
        /// <summary>
        /// ����ʱ��ڵ�ʵ����Ӧ�Ķ���
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
        /// ͨ�����ļ��ж�ȡ���ViewIDToIntantiatedID��ֵ������RecordObjectView
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
        /// BaseView����������ʵ�����Ĵ���MViewCount
        /// </summary>
        public struct BaseViewCount
        {
            public RecordObjectView MBaseView;
            public int MViewCount;
        }
        /// <summary>
        /// Ӳ���ļ��е�ViewID��Ӧ��IntantiatedID 
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
            /// �ļ�·��
            /// </summary>
            public string Path;
            /// <summary>
            /// �ļ�������
            /// </summary>
            public long StreamLength;
            /// <summary>
            /// ʵ����ʱ��
            /// </summary>
            public int InstantiatedTime;
            /// <summary>
            /// ����ʱ��
            /// </summary>
            public int DestoryedTime;
        }
        #endregion

        #region ���ϲ�ѯ
        /// <summary>
        /// ��ȡʱ����ĵ���������Ϣ
        /// </summary>
        public class SingleObjectInfo
        {
            /// <summary>
            /// ViewID ��¼��ʶ
            /// </summary>
            public int ViewID { get; set; }
            /// <summary>
            /// ʵ��ID ��ǰ��Cloneʵ�������ĵڼ���
            /// </summary>
            public int InstantiatedID { get; set; }
            /// <summary>
            /// RecordObjectView ����,ͨ������ȡ��������������Ϣ
            /// </summary>
            public RecordObjectView SingleView { get; set; }
            /// <summary>
            /// ��ʵ������������ʱ��
            /// </summary>
            public int InstantiatedTime { get; set; }
            /// <summary>
            /// ���ٵ�ʱ��ڵ�
            /// </summary>
            public int DestoryedTime { get; set; }
            /// <summary>
            /// ���ݶ�ȡ·��
            /// </summary>
            public string ReadPath { get; set; }
            /// <summary>
            /// �ļ�����С����
            /// </summary>
            public long StreamLength { get; set; }
            /// <summary>
            /// �������·��
            /// </summary>
            public string LoadPath { get; set; }
        }
        /// <summary>
        /// ���ݶ�ȡ�������ϲ�ѯ��
        /// </summary>
        public class RcorderDataReader : Table<SingleObjectInfo>
        {
            /// <summary>
            /// ����ViewID��SOI
            /// </summary>
            public TableIndex<int, SingleObjectInfo> ViewID2SOI = 
                new TableIndex<int, SingleObjectInfo>((SOI) => SOI.ViewID);
            /// <summary>
            /// ����ReadPath��SOI
            /// </summary>
            public TableIndex<string, SingleObjectInfo> ReadPath2SOI =
                new TableIndex<string, SingleObjectInfo>((SOI) => SOI.ReadPath);
            /// <summary>
            /// ����LoadPath��SOI
            /// </summary>
            public TableIndex<string, SingleObjectInfo> LoadPath2SOI =
                new TableIndex<string, SingleObjectInfo>((SOI)=>SOI.LoadPath);
            /// <summary>
            /// ����InstantiatedTime��SOI
            /// </summary>
            public TableIndex<int, SingleObjectInfo> InstantiatedTime2SOI =
                new TableIndex<int, SingleObjectInfo>((SOI)=>SOI.InstantiatedTime);
            /// <summary>
            /// ����DestoryedTime��SOI
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

