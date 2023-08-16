using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// ÏòÇ°ÊµÀý
    /// </summary>
    public class InstantiateForwardCommand : AbstractCommand
    {
        private Timer mTimer;
        protected override void OnExecute()
        {
            mTimer= this.GetSystem<Timer>();
            mTimer.SetForward(true);
        }
    }

}
