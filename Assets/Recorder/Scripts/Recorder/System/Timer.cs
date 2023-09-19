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
                    if (mCurrentTimeInWatching >= mEndTime)
                    {
                        mCurrentTimeInWatching = mEndTime;
                    }
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
        /// <summary>
        /// 当前时间 观看模式下
        /// </summary>
        public float CurrentTimeInWatching => mCurrentTimeInWatching;
        /// <summary>
        /// 记录模式下 获取当前时间
        /// </summary>
        public float CurrentTime => Time.time - StartTime;
        /// <summary>
        /// 设置结束时间
        /// </summary>
        public float EndTime { set => mEndTime = value; }
        private float mCurrentTimeInWatching;
        private float StartTime;
        private bool mIsPause;
        private bool mIsForward;
        private float mEndTime;
        protected override void OnInit()
        {
            mIsPause = false; 
            mIsForward = true;
            MasterTime = 0;
            mCurrentTimeInWatching = 0;
            mEndTime = 0;
            Reset();
        }
        /// <summary>
        /// 重置时间
        /// </summary>
        public void Reset()
        {
            StartTime = Time.time;
            mCurrentTimeInWatching = 0;
        }
        /// <summary>
        /// 暂停时间，观看模式，同时会连带的暂停数据读取
        /// </summary>
        /// <param name="isPause"></param>
        public void SetPause(bool isPause) =>mIsPause = isPause;
        /// <summary>
        /// 观看模式下，设置向前回放还是向后回放
        /// </summary>
        /// <param name="isForward"></param>
        public void SetForward(bool isForward)=> mIsForward = isForward;
        /// <summary>
        /// 外部属性，当前是向前看还是向后看
        /// </summary>
        public bool IsForward => mIsForward;
        /// <summary>
        /// 外部属性，当前是否暂停
        /// </summary>
        public bool IsPaused => mIsPause;
    }
}

