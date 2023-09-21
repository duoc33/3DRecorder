using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// ����View�������ʱ������ָ��
    /// </summary>
    public class ObjectEndRecordCommand : AbstractCommand
    {
        RecordObjectView mView;
        public ObjectEndRecordCommand(RecordObjectView recordObjectView) 
        {
            mView = recordObjectView;
        }
        protected override void OnExecute()
        {
            this.SendEvent<ObjectEndRecordingEvent>(new ObjectEndRecordingEvent() { View = mView });
        }
    }
}

