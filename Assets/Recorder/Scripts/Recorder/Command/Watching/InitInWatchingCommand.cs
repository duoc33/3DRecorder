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
        private DataReadUtility mDataReadUtility;
        protected override void OnExecute()
        {
            mDataReadUtility = this.GetUtility<DataReadUtility>();
            mPathModel = this.GetModel<PathModel>();
            if (!Directory.Exists(mPathModel.SavePath)) {
                Debug.LogError("该路径下没有指定文件夹: "+ mPathModel.SavePath);
                return;
            }
            mStateModel = this.GetModel<StateModel>();
            mTimer = this.GetSystem<Timer>();
            mTimer.EndTime = mDataReadUtility.ReadInfo(mPathModel.RecordInfoPath);
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
                if (tempTime == CurrentTime) {
                    //优化，防止过多的检测
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                tempTime = CurrentTime;
                mDataCenter.RecorderReader.InstantiatedByTime(CurrentTime,mTimer.IsForward);
                yield return new WaitForEndOfFrame();
            }
        }
        private int tempTime = 0;
    }
}

