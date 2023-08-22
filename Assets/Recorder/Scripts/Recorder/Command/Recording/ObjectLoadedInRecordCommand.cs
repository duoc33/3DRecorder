using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace Record
{
    public class ObjectLoadedInRecordCommand : AbstractCommand
    {
        RecordObjectView mBaseView;
        public ObjectLoadedInRecordCommand() { }
        public ObjectLoadedInRecordCommand(RecordObjectView baseView)
        {
            mBaseView = baseView;
        }
        protected override void OnExecute()
        {
            this.SendEvent<ObjectLoadedInRecordingEvent>(new ObjectLoadedInRecordingEvent() { View = mBaseView });
        }
        
    }
}

