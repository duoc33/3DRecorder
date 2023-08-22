using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    public class ObjectLoadedInWatchingCommand : AbstractCommand
    {
        RecordObjectView mView;
        public ObjectLoadedInWatchingCommand(RecordObjectView View) 
        {
            mView = View;
        }
        protected override void OnExecute()
        {
            this.SendEvent<ObjectLoadedInWatchingEvent>(new ObjectLoadedInWatchingEvent() { View = mView });
        }
    }
}

