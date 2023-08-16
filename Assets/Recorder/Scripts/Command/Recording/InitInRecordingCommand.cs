using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Record
{
    public class InitInRecordingCommand :AbstractCommand
    {
        private StateModel mStateModel;
        private DataWriteUtility mDataWriteUtility;
        private PathModel mPathModel;
        private Timer mTimer;
        protected override void OnExecute()
        {
            mPathModel = this.GetModel<PathModel>();
            mDataWriteUtility = this.GetUtility<DataWriteUtility>();
            mTimer = this.GetSystem<Timer>();
            mDataWriteUtility.InitDirectory(mPathModel.SavePath);
            mTimer.Reset();
            mStateModel.State.Value = StateType.Recording;//正式开始记录
        }
    }
}

