using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    public class StateModel : AbstractModel
    {
        public BindableProperty<StateType> State { get; set; } = new BindableProperty<StateType>();
        protected override void OnInit()
        {
            State.Value = StateType.None;
        }
    }
    public enum StateType 
    {
        None,
        Recording,
        Watching,
        Resume,
        Pause,
    }
}

