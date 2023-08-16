using DG.Tweening;
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
        private RecordObjectLoadPool mRecordObjectLoadPool;
        private StateModel mStateModel;
        private Timer mTimer;
        protected override void Init()
        {
            mDataWriteUtility = new DataWriteUtility();
            mDataReadUtility = new DataReadUtility();
            mPathModel = new PathModel();
            mStateModel = new StateModel();
            mRecordObjectLoadPool = new RecordObjectLoadPool();
            mTimer = new Timer();

            this.RegisterUtility<DataReadUtility>(mDataReadUtility);
            this.RegisterUtility<DataWriteUtility>(mDataWriteUtility);
            this.RegisterModel<StateModel>(mStateModel);
            this.RegisterModel<PathModel>(mPathModel);
            this.RegisterModel<RecordObjectLoadPool>(mRecordObjectLoadPool);
            this.RegisterSystem<Timer>(mTimer);

            this.RegisterEvent<ObjectLoadedInRecordingEvent>((e) => {
                mRecordObjectLoadPool.AddBaseViewOnRecording(e.EventParamBaseView);
            }).UnRegisterWhenGameObjectDestroyed(Recorder.Instance);

            this.RegisterEvent<ObjectLoadedInWatchingEvent>((e) => {
                //this.SendEvent();
            });
            
        }
    }
}

