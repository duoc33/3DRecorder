using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 物体加载进场景初始指令
    /// </summary>
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

