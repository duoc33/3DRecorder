using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 观看模式初始化
    /// </summary>
    public class InitInWatchingCommand : AbstractCommand
    {
        private StateModel mStateModel;
        private Timer mTimer;
        private RecordObjectLoadPool mRecordObjectLoadPool;
        private PathModel mPathModel;
        protected override void OnExecute()
        {
            mPathModel = this.GetModel<PathModel>();
            if (!File.Exists(mPathModel.SavePath)) {
                Debug.LogError("该路径下没有指定文件: "+ mPathModel.SavePath);
                return;
            }
            mStateModel = this.GetModel<StateModel>();
            mTimer = this.GetSystem<Timer>();
            mRecordObjectLoadPool = this.GetModel<RecordObjectLoadPool>();
            Recorder.Instance.StartCoroutine(InstantiatedObjectByTime());
        }
        /// <summary>
        /// 根据时间实例化物体
        /// </summary>
        /// <returns></returns>
        private IEnumerator InstantiatedObjectByTime()
        {
            yield return Recorder.Instance.StartCoroutine(mRecordObjectLoadPool.ParsePathConfig());
            mStateModel.State.Value = StateType.Watching;//正式开始观看模式
            mTimer.Reset();
        }
        
    }
}

