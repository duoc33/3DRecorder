using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 状态管理
    /// </summary>
    public class StateModel : AbstractModel
    {
        public BindableProperty<StateType> State { get; } = new BindableProperty<StateType>();
        /// <summary>
        /// 设置当前状态,
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
        /// 开始记录
        /// </summary>
        Recording,
        /// <summary>
        /// 观看回放
        /// </summary>
        Watching,
        /// <summary>
        /// 继续录制(记录模式使用)
        /// </summary>
        Resume,
        /// <summary>
        /// 回放中暂停
        /// </summary>
        Pause,
        /// <summary>
        /// 记录中暂停
        /// </summary>
        PauseInRecording
    }
}

