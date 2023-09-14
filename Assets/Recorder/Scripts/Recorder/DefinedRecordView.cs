using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 自定义回放检测对象的View组件
    /// </summary>
    public class DefinedRecordView : RecordObjectView
    {
        /// <summary>
        /// 检测间隔
        /// </summary>
        [HideInInspector]
        public float mDetectInterval = 0;
        protected override void Start() => base.Start();
        /// <summary>
        /// 开始记录
        /// </summary>
        protected override void EnterRecording() => DetectInterval = mDetectInterval;
        /// <summary>
        /// 开始观看
        /// </summary>
        protected override void EnterWatching() { }
        /// <summary>
        /// 退出记录
        /// </summary>
        protected override void ExitRecording() { }
        /// <summary>
        /// 指明记录的数据信息类型
        /// </summary>
        /// <returns></returns>
        protected override AbstractRecordData DefineDataType()=> new DefinedRecordData();
        /// <summary>
        /// 指明反序列化赋值方式
        /// </summary>
        /// <param name="Json"></param>
        /// <returns></returns>
        protected override AbstractRecordData DeserializeType(string Json) => JsonUtility.FromJson<DefinedRecordData>(Json);
    }
}

