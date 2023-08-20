using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 时间功能,时间系统
    /// </summary>
    public class Timer : AbstractSystem
    {
        /// <summary>
        /// 观看回放使用的时间
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
        /// 记录模式下获取当前时间
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

