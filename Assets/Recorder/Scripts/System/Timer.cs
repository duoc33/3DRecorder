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
                    return mCurrentTimeInWatching;
                }
                if (mIsForward)
                {
                    mCurrentTimeInWatching += Time.deltaTime;
                    return mCurrentTimeInWatching;
                }
                else
                {
                    mCurrentTimeInWatching -= Time.deltaTime;
                    if (mCurrentTimeInWatching < 0)
                    {
                        mCurrentTimeInWatching = 0;
                        return 0;
                    }
                    else
                    {
                        mCurrentTimeInWatching -= Time.deltaTime;
                        return mCurrentTimeInWatching;
                    }
                }
            }
            set {
                mCurrentTimeInWatching = value;
            }
        }
        public float CurrentTimeInWatching => mCurrentTimeInWatching;
        /// <summary>
        /// ��¼ģʽ�»�ȡ��ǰʱ��
        /// </summary>
        public float CurrentTime => Time.time - StartTime;
        private float mCurrentTimeInWatching;
        private float StartTime;
        private bool mIsPause;
        private bool mIsForward;

        protected override void OnInit()
        {
            mIsPause = false; 
            mIsForward = true;
            MasterTime = 0;
            mCurrentTimeInWatching = 0;
            Reset();
        }
        public void Reset()
        {
            StartTime = Time.time;
            mCurrentTimeInWatching = 0;
        }
        public void SetPause(bool isPause) =>mIsPause = isPause;
        public void SetForward(bool isForward)=> mIsForward = isForward;
        public bool IsForward => mIsForward;
        public bool IsPaused => mIsPause;
    }
}

