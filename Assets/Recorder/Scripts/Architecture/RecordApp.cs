using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Record
{
    /// <summary>
    /// ¼Ü¹¹ÖÐÐÄ
    /// </summary>
    public class RecordApp : Architecture<RecordApp>
    {
        private DataReadUtility mDataReadUtility;
        private DataWriteUtility mDataWriteUtility;
        private PathModel mPathModel;
        private RecorderDataCenter mDataCenter;
        private StateModel mStateModel;
        private Timer mTimer;
        protected override void Init()
        {
            mDataWriteUtility = new DataWriteUtility();
            mDataReadUtility = new DataReadUtility();
            mPathModel = new PathModel();
            mStateModel = new StateModel();
            mDataCenter = new RecorderDataCenter();
            mTimer = new Timer();

            this.RegisterUtility<DataReadUtility>(mDataReadUtility);
            this.RegisterUtility<DataWriteUtility>(mDataWriteUtility);
            this.RegisterModel<StateModel>(mStateModel);
            this.RegisterModel<PathModel>(mPathModel);
            this.RegisterModel<RecorderDataCenter>(mDataCenter);
            this.RegisterSystem<Timer>(mTimer);


        }
    }
}

