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
        private RecorderDataCenter mDataCenter;
        private PathModel mPathModel;
        protected override void OnExecute()
        {
            mPathModel = this.GetModel<PathModel>();
            if (!Directory.Exists(mPathModel.SavePath)) {
                Debug.LogError("该路径下没有指定文件夹: "+ mPathModel.SavePath);
                return;
            }
            mStateModel = this.GetModel<StateModel>();
            mTimer = this.GetSystem<Timer>();
            mDataCenter = this.GetModel<RecorderDataCenter>();
            Recorder.Instance.StartCoroutine(InstantiatedObjectByTime());
        }
        /// <summary>
        /// 根据时间实例化物体
        /// </summary>
        /// <returns></returns>
        private IEnumerator InstantiatedObjectByTime()
        {
            yield return Recorder.Instance.StartCoroutine(mDataCenter.ParseFileAndScriptableObjectConfig(mPathModel.SavePath));
            mStateModel.SetState(StateType.Watching);//正式开始观看模式
            mTimer.Reset();
            while (mStateModel.State.Value == StateType.Watching) {
                int CurrentTime =Mathf.FloorToInt(mTimer.CurrentTimeInWatching);
                yield return mDataCenter.RcorderReader.InstantiatedByTime(CurrentTime);
                yield return new WaitForEndOfFrame();
            }
        }
        
    }
}

