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
        public Dictionary<int, List<ViewInfoInRecording>> RecordModeDic => RecordingModeDic;
        
        /// <summary>
        /// ��¼ģʽ�¼�¼��ǰ����ĸ���
        /// </summary>
        private Dictionary<int, List<ViewInfoInRecording> > RecordingModeDic = new Dictionary<int, List<ViewInfoInRecording>>();
        /// <summary>
        /// ��¼��ǰ�����еĻطŶ��󣬻�����ǵ�ʵ������������ʼ�����ǵ������ļ�
        /// </summary>
        /// <param name="baseView">�طŶ�������</param>
        public ViewInfoInRecording AddBaseViewOnRecording(RecordObjectView baseView)
        {
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
        public RecorderDataReader RecorderReader => mDataReader;
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
            pathConfig =Object.Instantiate(Resources.Load<RecordObjectLoadPathConfig>("RecorderConfig"));
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

        #endregion
    }
    /// <summary>
    /// ¼��ģʽ�еĽṹ
    /// </summary>
    public class ViewInfoInRecording
    {
        public RecordObjectView View;
        public int InstantiatedID;
        public Coroutine WriteCoroutine;
    }
}

