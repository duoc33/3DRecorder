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
    public class RecorderDataReader : Table<SingleObjectInfo>
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
        public IEnumerator InstantiatedByTime(int CurrentTime) 
        {
            yield return Ins(CurrentTime);
            Des(CurrentTime);
        }
        /// <summary>
        /// 当前时间节点实例出来的
        /// </summary>
        private IEnumerator Ins(int CurrentTime) 
        {
            if (InstantiatedTime2SOI.Get(CurrentTime).Where(x=>x.SingleView==null).Count() <= 0) yield break;
            foreach (SingleObjectInfo SameTimeInsItem in InstantiatedTime2SOI.Get(CurrentTime).Where(x=>x.SingleView == null))
            {
                if (SameTimeInsItem.SingleView != null) continue;
                if (SameTimeInsItem.BeInstantiatedID == 0) {
                    RecordObjectView[] views =
                          Object.Instantiate(Resources.Load<GameObject>(SameTimeInsItem.LoadPath)).GetComponentsInChildren<RecordObjectView>();
                    foreach (SingleObjectInfo SameTimeInsItem_1 in InstantiatedTime2SOI.Get(CurrentTime).Where(x => x.SingleView == null))
                    {
                        foreach (var view in views)
                        {
                            if (view.ViewID == SameTimeInsItem_1.ViewID)
                            {
                                UpdateSingleObjectInfo(view, SameTimeInsItem_1);
                            }
                        }
                        yield return null;
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
                                yield break;
                            }
                            RecordObjectView view = Object.Instantiate(item.SingleView);
                            UpdateSingleObjectInfo(view, SameTimeInsItem);
                            yield break;
                        }
                        yield return null;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
        /// <summary>
        /// 当前时间节点被销毁的
        /// </summary>
        private void Des(int CurrentTime) 
        {
            if (DestoryedTime2SOI.Get(CurrentTime).Where(x=>x.SingleView!=null).Count() <= 0) return;
            foreach (SingleObjectInfo SameTimeDesItem in DestoryedTime2SOI.Get(CurrentTime).Where(x=>x.SingleView!=null))
            {
                if (SameTimeDesItem.SingleView == null) continue;
                Object.Destroy(SameTimeDesItem.SingleView);
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
        #endregion

    }
    #endregion
}

