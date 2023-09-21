using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 录制时，回放对象加载进场景的事件
    /// </summary>
    public struct ObjectLoadedInRecordingEvent
    {
        public RecordObjectView View;
    }
    /// <summary>
    /// 结束录制发生的事件
    /// </summary>
    public struct ObjectEndRecordingEvent 
    {
        public RecordObjectView View;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct ObjectLoadedInWatchingEvent 
    {  
        public RecordObjectView View; 
    }
}

