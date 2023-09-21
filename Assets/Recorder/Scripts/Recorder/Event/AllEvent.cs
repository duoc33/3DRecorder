using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// ¼��ʱ���طŶ�����ؽ��������¼�
    /// </summary>
    public struct ObjectLoadedInRecordingEvent
    {
        public RecordObjectView View;
    }
    /// <summary>
    /// ����¼�Ʒ������¼�
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

