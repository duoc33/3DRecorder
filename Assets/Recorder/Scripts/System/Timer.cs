using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// ʱ�书��,ʱ��ϵͳ
    /// </summary>
    public class Timer : AbstractSystem
    {
        /// <summary>
        /// �ۿ��ط�ʹ�õ�ʱ��
        /// </summary>
        public float MasterTime
        {
            get {
                if (mIsPause) {
                    return CurrentTimeInWatching;
                }
                if (mIsForward)
                {
                    CurrentTimeInWatching += Time.deltaTime;
                    return CurrentTimeInWatching;
                }
                else
                {
                    CurrentTimeInWatching -= Time.deltaTime;
                    if (CurrentTimeInWatching < 0)
                    {
                        CurrentTimeInWatching = 0;
                        return 0;
                    }
                    else
                    {
                        CurrentTimeInWatching -= Time.deltaTime;
                        return CurrentTimeInWatching;
                    }
                }
            }
            set {
                CurrentTimeInWatching = value;
            }
        }
        public float GetCurrentTimeInWatching() => CurrentTimeInWatching;
        /// <summary>
        /// ��¼ģʽ�»�ȡ��ǰʱ��
        /// </summary>
        public float CurrentTime => Time.time - StartTime;
        private float CurrentTimeInWatching;
        private float StartTime;
        private bool mIsPause;
        private bool mIsForward;

        protected override void OnInit()
        {
            mIsPause = false; 
            mIsForward = true;
            MasterTime = 0;
            CurrentTimeInWatching = 0;
            Reset();
        }
        public void Reset()
        {
            StartTime = Time.time;
            CurrentTimeInWatching = 0;
        }
        public void SetPause(bool isPause) =>mIsPause = isPause;
        public void SetForward(bool isForward)=> mIsForward = isForward;
    }
}

