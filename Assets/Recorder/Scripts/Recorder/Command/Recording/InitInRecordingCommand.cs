using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Record
{
    /// <summary>
    /// 开始录制的指令
    /// </summary>
    public class InitInRecordingCommand :AbstractCommand
    {
        private StateModel mStateModel;
        private DataWriteUtility mDataWriteUtility;
        private PathModel mPathModel;
        private Timer mTimer;
        protected override void OnExecute()
        {
            mStateModel = this.GetModel<StateModel>();
            mPathModel = this.GetModel<PathModel>();
            mDataWriteUtility = new DataWriteUtility();
            mTimer = this.GetSystem<Timer>();
            mDataWriteUtility.InitDirectory(mPathModel.SavePath);
            mTimer.Reset();
            mStateModel.SetState(StateType.Recording);//正式开始记录
        }
    }
}

