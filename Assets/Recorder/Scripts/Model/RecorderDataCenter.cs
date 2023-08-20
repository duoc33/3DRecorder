using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

namespace Record
{
    /// <summary>
    /// �طŶ���ϼ�������ʵ������������ݵļ�¼���������ļ�������
    /// </summary>
    public class RecorderDataCenter : AbstractModel
    {
        private DataReadUtility mDataReadUtility;
        private DataWriteUtility mDataWriteUtility;
        private RecordObjectLoadPathConfig pathConfig;
        protected override void OnInit()
        {
            mDataReadUtility = this.GetUtility<DataReadUtility>();
            mDataWriteUtility = this.GetUtility<DataWriteUtility>();
        }

        #region RecordingMode
        /// <summary>
        /// ��ǰ�������м�¼�����ڲ��п����Ѿ���������
        /// </summary>
        public Dictionary<int, BaseViewCount> RecordModeDic => RecordingModeDic;
        /// <summary>
        /// BaseView����������ʵ�����Ĵ���MViewCount
        /// </summary>
        public struct BaseViewCount
        {
            public RecordObjectView MBaseView;
            public int MViewCount;
        }
        /// <summary>
        /// ��¼ģʽ�¼�¼��ǰ����ĸ���
        /// </summary>
        private Dictionary<int, BaseViewCount> RecordingModeDic = new Dictionary<int, BaseViewCount>();
        /// <summary>
        /// ��¼��ǰ�����еĻطŶ��󣬻�����ǵ�ʵ������������ʼ�����ǵ������ļ�
        /// </summary>
        /// <param name="baseView">�طŶ�������</param>
        public void AddBaseViewOnRecording(RecordObjectView baseView)
        {
            int temBeInstantiatedID = baseView.InstantiatedID;
            if (RecordingModeDic.ContainsKey(baseView.ViewID))
            {
                BaseViewCount bvc = RecordingModeDic[baseView.ViewID];
                bvc.MViewCount++;
                baseView.InstantiatedID = bvc.MViewCount;
                RecordingModeDic[baseView.ViewID] = bvc;
                baseView.ObjectSavePath = baseView.SavePath + "_" + bvc.MViewCount.ToString() + "_" + temBeInstantiatedID.ToString() + ".rd";
                mDataWriteUtility.InitFile(baseView.ObjectSavePath);
            }
            else
            {
                BaseViewCount bvc = new BaseViewCount() { MBaseView = baseView, MViewCount = 1 };
                baseView.InstantiatedID = bvc.MViewCount;
                RecordingModeDic[baseView.ViewID] = bvc;
                baseView.ObjectSavePath = baseView.SavePath + "_" + bvc.MViewCount.ToString() + "_" + temBeInstantiatedID.ToString() + ".rd"; ;
                mDataWriteUtility.InitFile(baseView.ObjectSavePath);
            }
        }
        #endregion

        #region WatchingMode
        public RecorderDataReader RcorderReader => mDataReader;
        /// <summary>
        /// ���ϲ�ѯ��
        /// </summary>
        private RecorderDataReader mDataReader;

        /// <summary>
        /// �����ļ���������ϲ�ѯ��
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public IEnumerator ParseFileAndScriptableObjectConfig(string directoryPath) 
        {
            mDataReader = new RecorderDataReader();
            yield return mDataReadUtility.ReadFileConfigTable(directoryPath,mDataReader);
            pathConfig =Object.Instantiate(Resources.Load<RecordObjectLoadPathConfig>("RecordConfig"));
            yield return ParseScriptableObjectConfig(pathConfig);
        }
        /// <summary>
        /// ����·������ �ٳ�ʼ�� ���ϲ�ѯ��
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
                            Debug.LogError("�����뵱ǰ�����д��ڵ�ViewID�ǲ�ƥ���,����Ԥ�Ƽ�����������һ��");
                            yield break;
                        }
                        if (mDataReader.ViewID2SOI.Get(view.ViewID).Count() <= 0) continue;
                        //LoadPathΪnull���ڳ����оͼ��� RecordObjectView�����Կ���������InstantiatedIDΪ1��RecordObjectView
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

        #region None
        //public void InstantiatedByTime(float CurrentTime)
        //{
        //    if (mDataReader.InstantiatedTime2SOI.Get(Mathf.FloorToInt(CurrentTime)).Count() <= 0) return;
        //    foreach (SingleObjectInfo item in mDataReader.InstantiatedTime2SOI.Get(Mathf.FloorToInt(CurrentTime)))
        //    {
        //        if (item.SingleView == null)
        //        {
        //            if (item.BeInstantiatedID == 0)
        //            {
        //                if (item.LoadPath.Equals(string.Empty))
        //                {

        //                }
        //                else
        //                {
        //                    RecordObjectView[] views = Object.Instantiate(Resources.Load<GameObject>(item.LoadPath)).
        //                    GetComponentsInChildren<RecordObjectView>(true);
        //                    foreach (var view in views)
        //                    {

        //                        if (view.ViewID == item.ViewID)
        //                        {
        //                            view.InstantiatedID = item.InstantiatedID;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {

        //            }
        //        }
        //        else
        //        {
        //            foreach (SingleObjectInfo temp in mDataReader.ViewID2SOI.Get(item.ViewID))
        //            {
        //                if (temp.InstantiatedID == item.BeInstantiatedID)
        //                {
        //                    if (temp.SingleView != null)
        //                    {
        //                        item.SingleView = Object.Instantiate(temp.SingleView);
        //                        item.SingleView.InstantiatedID = item.InstantiatedID;
        //                        item.SingleView.ObjectSavePath = item.ReadPath;
        //                        item.SingleView.FileLength = item.StreamLength;
        //                        break;
        //                    }
        //                    else
        //                    {

        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion

        #endregion

    }
}

