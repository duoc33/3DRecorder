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
        public RecordObjectView EventParamBaseView;
    }
    /// <summary>
    /// �ط�ʱ���طŶ�����ؽ��������¼�
    /// </summary>
    public struct ObjectLoadedInWatchingEvent
    {
        public RecordObjectView EventParamBaseView;
    }
    public struct ObjectEndRecordingEvent
    {
    }
}

