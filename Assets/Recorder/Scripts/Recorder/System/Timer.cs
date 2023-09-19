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
        /// ��ǰʱ�� �ۿ�ģʽ��
        /// </summary>
        public float CurrentTimeInWatching => mCurrentTimeInWatching;
        /// <summary>
        /// ��¼ģʽ�� ��ȡ��ǰʱ��
        /// </summary>
        public float CurrentTime => Time.time - StartTime;
        /// <summary>
        /// ���ý���ʱ��
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
        /// ����ʱ��
        /// </summary>
        public void Reset()
        {
            StartTime = Time.time;
            mCurrentTimeInWatching = 0;
        }
        /// <summary>
        /// ��ͣʱ�䣬�ۿ�ģʽ��ͬʱ����������ͣ���ݶ�ȡ
        /// </summary>
        /// <param name="isPause"></param>
        public void SetPause(bool isPause) =>mIsPause = isPause;
        /// <summary>
        /// �ۿ�ģʽ�£�������ǰ�طŻ������ط�
        /// </summary>
        /// <param name="isForward"></param>
        public void SetForward(bool isForward)=> mIsForward = isForward;
        /// <summary>
        /// �ⲿ���ԣ���ǰ����ǰ���������
        /// </summary>
        public bool IsForward => mIsForward;
        /// <summary>
        /// �ⲿ���ԣ���ǰ�Ƿ���ͣ
        /// </summary>
        public bool IsPaused => mIsPause;
    }
}

