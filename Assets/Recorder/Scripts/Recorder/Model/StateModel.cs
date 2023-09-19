using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// ״̬����
    /// </summary>
    public class StateModel : AbstractModel
    {
        public BindableProperty<StateType> State { get; } = new BindableProperty<StateType>();
        /// <summary>
        /// ���õ�ǰ״̬,
        /// </summary>
        /// <param name="state"></param>
        public void SetState(StateType state) => State.Value = state;
        protected override void OnInit()
        {
            State.Value = StateType.None;
        }
    }
    public enum StateType 
    {
        None,
        /// <summary>
        /// ��ʼ��¼
        /// </summary>
        Recording,
        /// <summary>
        /// �ۿ��ط�
        /// </summary>
        Watching,
        /// <summary>
        /// ����¼��(��¼ģʽʹ��)
        /// </summary>
        Resume,
        /// <summary>
        /// �ط�����ͣ
        /// </summary>
        Pause,
        /// <summary>
        /// ��¼����ͣ
        /// </summary>
        PauseInRecording
    }
}

