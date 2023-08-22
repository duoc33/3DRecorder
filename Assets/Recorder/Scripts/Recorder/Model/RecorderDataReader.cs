using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

namespace Record
{
    #region 联合查询
    /// <summary>
    /// 数据读取器，联合查询器
    /// </summary>
    public class RecorderDataReader : Table<SingleObjectInfo>,IModel
    {
        #region 属性

        #endregion

        #region DataStruct
        /// <summary>
        /// 根据ViewID找SOI
        /// </summary>
        public TableIndex<int, SingleObjectInfo> ViewID2SOI =
            new TableIndex<int, SingleObjectInfo>((SOI) => SOI.ViewID);
        /// <summary>
        /// 根据InstantiatedTime找SOI
        /// </summary>
        public TableIndex<int, SingleObjectInfo> InstantiatedTime2SOI =
            new TableIndex<int, SingleObjectInfo>((SOI) => SOI.InstantiatedTime);
        /// <summary>
        /// 根据DestoryedTime找SOI
        /// </summary>
        public TableIndex<int, SingleObjectInfo> DestoryedTime2SOI =
            new TableIndex<int, SingleObjectInfo>((SOI) => SOI.DestoryedTime);
        #endregion

        #region 重写方法
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
        /// 逻辑过于复杂
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
        /// 当前时间节点实例出来的
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
                                Debug.LogError("错误： 先实例的需要被Clone的对象被摧毁");
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
        /// 当前时间节点被销毁的
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
                                Debug.LogError("错误： 先实例的需要被Clone的对象被摧毁");
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
        /// 根据新实例的RecordObjectView 更新SingleObjectInfo和RecordObjectView本身
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
            Debug.Log("初始化了");
        }
    }
    /// <summary>
    /// 读取时所需的单个对象信息
    /// </summary>
    public class SingleObjectInfo
    {
        #region 根据磁盘文件直接可得
        /// <summary>
        /// ViewID 记录标识
        /// </summary>
        public int ViewID { get; set; }
        /// <summary>
        /// 实例ID 当前被Clone实例出来的第几个
        /// </summary>
        public int InstantiatedID { get; set; }
        /// <summary>
        /// 被谁Clone出来的实例ID
        /// </summary>
        public int BeInstantiatedID { get; set; }
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
        #endregion

        #region 需要从ScriptableObject配置表中获取信息,并实例后赋值
        /// <summary>
        /// 对象加载路径
        /// </summary>
        public string LoadPath { get; set; }
        /// <summary>
        /// RecordObjectView 本身,通过它获取对象的其他组件信息
        /// </summary>
        public RecordObjectView SingleView { get; set; }
        /// <summary>
        /// 当前对象在数据流中的位置
        /// </summary>
        public long CurrentIndexInStream;
        /// <summary>
        /// 读取协程
        /// </summary>
        public Coroutine ReadCoroutine { get; set; }
        #endregion

    }
    #endregion
}

