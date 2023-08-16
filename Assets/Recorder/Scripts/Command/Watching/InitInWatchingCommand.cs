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
        private RecordObjectLoadPool mRecordObjectLoadPool;
        private PathModel mPathModel;
        protected override void OnExecute()
        {
            mPathModel = this.GetModel<PathModel>();
            if (!File.Exists(mPathModel.SavePath)) {
                Debug.LogError("��·����û��ָ���ļ�: "+ mPathModel.SavePath);
                return;
            }
            mStateModel = this.GetModel<StateModel>();
            mTimer = this.GetSystem<Timer>();
            mRecordObjectLoadPool = this.GetModel<RecordObjectLoadPool>();
            Recorder.Instance.StartCoroutine(InstantiatedObjectByTime());
        }
        /// <summary>
        /// ����ʱ��ʵ��������
        /// </summary>
        /// <returns></returns>
        private IEnumerator InstantiatedObjectByTime()
        {
            yield return Recorder.Instance.StartCoroutine(mRecordObjectLoadPool.ParsePathConfig());
            mStateModel.State.Value = StateType.Watching;//��ʽ��ʼ�ۿ�ģʽ
            mTimer.Reset();
        }
        
    }
}

