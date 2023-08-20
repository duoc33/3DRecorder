using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// �ۿ�ģʽ��ʼ��
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
                Debug.LogError("��·����û��ָ���ļ���: "+ mPathModel.SavePath);
                return;
            }
            mStateModel = this.GetModel<StateModel>();
            mTimer = this.GetSystem<Timer>();
            mDataCenter = this.GetModel<RecorderDataCenter>();
            Recorder.Instance.StartCoroutine(InstantiatedObjectByTime());
        }
        /// <summary>
        /// ����ʱ��ʵ��������
        /// </summary>
        /// <returns></returns>
        private IEnumerator InstantiatedObjectByTime()
        {
            yield return Recorder.Instance.StartCoroutine(mDataCenter.ParseFileAndScriptableObjectConfig(mPathModel.SavePath));
            mStateModel.SetState(StateType.Watching);//��ʽ��ʼ�ۿ�ģʽ
            mTimer.Reset();
            while (mStateModel.State.Value == StateType.Watching) {
                int CurrentTime =Mathf.FloorToInt(mTimer.CurrentTimeInWatching);
                yield return mDataCenter.RcorderReader.InstantiatedByTime(CurrentTime);
                yield return new WaitForEndOfFrame();
            }
        }
        
    }
}

