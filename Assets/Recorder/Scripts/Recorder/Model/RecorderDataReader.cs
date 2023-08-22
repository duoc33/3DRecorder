using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

namespace Record
{
    #region ���ϲ�ѯ
    /// <summary>
    /// ���ݶ�ȡ�������ϲ�ѯ��
    /// </summary>
    public class RecorderDataReader : Table<SingleObjectInfo>,IModel
    {
        #region ����

        #endregion

        #region DataStruct
        /// <summary>
        /// ����ViewID��SOI
        /// </summary>
        public TableIndex<int, SingleObjectInfo> ViewID2SOI =
            new TableIndex<int, SingleObjectInfo>((SOI) => SOI.ViewID);
        /// <summary>
        /// ����InstantiatedTime��SOI
        /// </summary>
        public TableIndex<int, SingleObjectInfo> InstantiatedTime2SOI =
            new TableIndex<int, SingleObjectInfo>((SOI) => SOI.InstantiatedTime);
        /// <summary>
        /// ����DestoryedTime��SOI
        /// </summary>
        public TableIndex<int, SingleObjectInfo> DestoryedTime2SOI =
            new TableIndex<int, SingleObjectInfo>((SOI) => SOI.DestoryedTime);
        #endregion

        #region ��д����
        protected override void OnAdd(SingleObjectInfo item)
        {
            ViewID2SOI.Add(item);
            InstantiatedTime2SOI.Add(item);
            DestoryedTime2SOI.Add(item);
        }

        protected override void OnClear()
        {
            ViewID2SOI.Clear();
            InstantiatedTime2SOI.Clear();
            DestoryedTime2SOI.Clear();
        }

        protected override void OnDispose()
        {
            ViewID2SOI.Dispose();
            InstantiatedTime2SOI.Dispose();
            DestoryedTime2SOI.Dispose();
        }
        protected override void OnRemove(SingleObjectInfo item)
        {
            ViewID2SOI.Remove(item);
            InstantiatedTime2SOI.Remove(item);
            DestoryedTime2SOI.Remove(item);
        }

        public override IEnumerator<SingleObjectInfo> GetEnumerator()
        {
            return ViewID2SOI.Dictionary.Values.SelectMany(s => s).GetEnumerator();
        }
        #endregion

        /// <summary>
        /// �߼����ڸ���
        /// </summary>
        /// <param name="CurrentTime"></param>
        public void InstantiatedByTime(int CurrentTime,bool IsForward) 
        {
            //InsForward(CurrentTime);
            //DesForward(CurrentTime);
            if (IsForward)
            {
                InsForward(CurrentTime);
                DesForward(CurrentTime);
            }
            else
            {
                DesBackward(CurrentTime);
                InsBackWard(CurrentTime);
            }
        }
        public SingleObjectInfo FindSoleSingleObjectInfoByView(RecordObjectView view)
        {
            foreach (var item in ViewID2SOI.Get(view.ViewID).Where(x => x.InstantiatedID == view.InstantiatedID))
            {
                return item;
            }
            return null;
        }
        public void GetCurrentTimeObject(int CurrentTime)
        {
            InsForward(CurrentTime);
            DesForward(CurrentTime);
            foreach (var Infos in ViewID2SOI.Dictionary.Values)
            {
                foreach (var item in Infos)
                {
                    if (item.SingleView == null) continue;
                    if (item.DestoryedTime < CurrentTime)
                    {
                        Object.Destroy(item.SingleView.gameObject);
                        item.CurrentIndexInStream = item.StreamLength - 4;
                    }
                }
            }
        }
        /// <summary>
        /// ��ǰʱ��ڵ�ʵ��������
        /// </summary>
        private void InsForward(int CurrentTime) 
        {
            if (InstantiatedTime2SOI.Get(CurrentTime).Count() <= 0) return;
            foreach (SingleObjectInfo SameTimeInsItem in InstantiatedTime2SOI.Get(CurrentTime).Where(x => x.LoadPath.Equals(string.Empty))) 
            {
                if (SameTimeInsItem.BeInstantiatedID==0)
                {
                    SameTimeInsItem.SingleView.gameObject.SetActive(true);
                    SameTimeInsItem.CurrentIndexInStream = 4;
                }
            }
            foreach (SingleObjectInfo SameTimeInsItem in InstantiatedTime2SOI.Get(CurrentTime).Where(x => x.SingleView == null))
            {
                if (SameTimeInsItem.SingleView != null) continue;
                if (SameTimeInsItem.BeInstantiatedID == 0)
                {
                    RecordObjectView[] views =
                          Object.Instantiate(Resources.Load<GameObject>(SameTimeInsItem.LoadPath)).GetComponentsInChildren<RecordObjectView>();
                    foreach (SingleObjectInfo SameTimeInsItem_1 in InstantiatedTime2SOI.Get(CurrentTime).Where(x => x.SingleView == null))
                    {
                        foreach (var view in views)
                        {
                            if (view.ViewID == SameTimeInsItem_1.ViewID)
                            {
                                UpdateSingleObjectInfo(view, SameTimeInsItem_1);
                                SameTimeInsItem_1.CurrentIndexInStream = 4;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in ViewID2SOI.Get(SameTimeInsItem.ViewID))
                    {
                        if (SameTimeInsItem.BeInstantiatedID == item.InstantiatedID)
                        {
                            if (item.SingleView == null)
                            {
                                Debug.LogError("���� ��ʵ������Ҫ��Clone�Ķ��󱻴ݻ�");
                                return;
                            }
                            RecordObjectView view = Object.Instantiate(item.SingleView);
                            UpdateSingleObjectInfo(view, SameTimeInsItem);
                            SameTimeInsItem.CurrentIndexInStream = 4;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// ��ǰʱ��ڵ㱻���ٵ�
        /// </summary>
        private void DesForward(int CurrentTime) 
        {
            if (DestoryedTime2SOI.Get(CurrentTime).Count() <= 0) return;
            foreach (SingleObjectInfo SameTimeDesItem in DestoryedTime2SOI.Get(CurrentTime).Where(x=>x.SingleView!=null))
            {
                if (SameTimeDesItem.SingleView == null) continue;
                if (SameTimeDesItem.LoadPath.Equals(string.Empty)) {
                    if (SameTimeDesItem.BeInstantiatedID == 0)
                    {
                        SameTimeDesItem.SingleView.gameObject.SetActive(false);
                    }
                    else
                    {
                        Object.Destroy(SameTimeDesItem.SingleView.gameObject);
                    }
                }
                else
                {
                    Object.Destroy(SameTimeDesItem.SingleView.gameObject);
                }
                SameTimeDesItem.CurrentIndexInStream = SameTimeDesItem.StreamLength - 4;
            }
        }
        private void DesBackward(int CurrentTime) 
        {
            if (DestoryedTime2SOI.Get(CurrentTime).Count() <= 0) return;
            foreach (var SameTimeDesItem in DestoryedTime2SOI.Get(CurrentTime).Where(x => x.LoadPath.Equals(string.Empty)))
            {
                if (SameTimeDesItem.BeInstantiatedID == 0)
                {
                    SameTimeDesItem.SingleView.gameObject.SetActive(true);
                    SameTimeDesItem.CurrentIndexInStream = SameTimeDesItem.StreamLength - 4;
                }
            }
            foreach (var SameTimeDesItem in DestoryedTime2SOI.Get(CurrentTime).Where(x => x.SingleView == null))
            {
                if (SameTimeDesItem.SingleView != null) continue;
                if (SameTimeDesItem.BeInstantiatedID == 0)
                {
                    RecordObjectView[] views =
                          Object.Instantiate(Resources.Load<GameObject>(SameTimeDesItem.LoadPath)).GetComponentsInChildren<RecordObjectView>();
                    foreach (SingleObjectInfo SameTimeDesItem_1 in DestoryedTime2SOI.Get(CurrentTime).Where(x => x.SingleView == null))
                    {
                        foreach (var view in views)
                        {
                            if (view.ViewID == SameTimeDesItem_1.ViewID)
                            {
                                UpdateSingleObjectInfo(view, SameTimeDesItem_1);
                                SameTimeDesItem_1.CurrentIndexInStream = SameTimeDesItem_1.StreamLength -4;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in ViewID2SOI.Get(SameTimeDesItem.ViewID))
                    {
                        if (SameTimeDesItem.BeInstantiatedID == item.InstantiatedID)
                        {
                            if (item.SingleView == null)
                            {
                                Debug.LogError("���� ��ʵ������Ҫ��Clone�Ķ��󱻴ݻ�");
                                return;
                            }
                            RecordObjectView view = Object.Instantiate(item.SingleView);
                            UpdateSingleObjectInfo(view, SameTimeDesItem);
                            SameTimeDesItem.CurrentIndexInStream = SameTimeDesItem.StreamLength - 4;
                        }
                    }
                }
            }
        }
        private void InsBackWard(int CurrentTime) 
        {
            if (InstantiatedTime2SOI.Get(CurrentTime).Count() <= 0) return;
            foreach (SingleObjectInfo SameTimeInsItem in InstantiatedTime2SOI.Get(CurrentTime).Where(x => x.SingleView != null))
            {
                if (SameTimeInsItem.SingleView == null) continue;
                if (SameTimeInsItem.LoadPath.Equals(string.Empty))
                {
                    if (SameTimeInsItem.BeInstantiatedID == 0)
                    {
                        SameTimeInsItem.SingleView.gameObject.SetActive(false);
                    }
                    else
                    {
                        Object.Destroy(SameTimeInsItem.SingleView.gameObject);
                    }
                }
                else
                {
                    Object.Destroy(SameTimeInsItem.SingleView.gameObject);
                }
                SameTimeInsItem.CurrentIndexInStream = 4;
            }
        }

        /// <summary>
        /// ������ʵ����RecordObjectView ����SingleObjectInfo��RecordObjectView����
        /// </summary>
        /// <param name="view"></param>
        /// <param name="singleObjectInfo"></param>
        private void UpdateSingleObjectInfo(RecordObjectView view, SingleObjectInfo singleObjectInfo) 
        {
            singleObjectInfo.SingleView = view;
            singleObjectInfo.SingleView.InstantiatedID = singleObjectInfo.InstantiatedID;
            singleObjectInfo.SingleView.ObjectSavePath = singleObjectInfo.ReadPath;
            singleObjectInfo.SingleView.FileLength = singleObjectInfo.StreamLength;
        }

        public void SetArchitecture(IArchitecture architecture)
        {
            mArchitecture = architecture;
        }
        private IArchitecture mArchitecture;
        public IArchitecture GetArchitecture()
        {
            return mArchitecture;
        }

        public void Init()
        {
            Debug.Log("��ʼ����");
        }
    }
    /// <summary>
    /// ��ȡʱ����ĵ���������Ϣ
    /// </summary>
    public class SingleObjectInfo
    {
        #region ���ݴ����ļ�ֱ�ӿɵ�
        /// <summary>
        /// ViewID ��¼��ʶ
        /// </summary>
        public int ViewID { get; set; }
        /// <summary>
        /// ʵ��ID ��ǰ��Cloneʵ�������ĵڼ���
        /// </summary>
        public int InstantiatedID { get; set; }
        /// <summary>
        /// ��˭Clone������ʵ��ID
        /// </summary>
        public int BeInstantiatedID { get; set; }
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
        #endregion

        #region ��Ҫ��ScriptableObject���ñ��л�ȡ��Ϣ,��ʵ����ֵ
        /// <summary>
        /// �������·��
        /// </summary>
        public string LoadPath { get; set; }
        /// <summary>
        /// RecordObjectView ����,ͨ������ȡ��������������Ϣ
        /// </summary>
        public RecordObjectView SingleView { get; set; }
        /// <summary>
        /// ��ǰ�������������е�λ��
        /// </summary>
        public long CurrentIndexInStream;
        /// <summary>
        /// ��ȡЭ��
        /// </summary>
        public Coroutine ReadCoroutine { get; set; }
        #endregion

    }
    #endregion
}

