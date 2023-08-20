using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    public class DefinedRecordView : RecordObjectView
    {
        /// <summary>
        /// ¼ì²â¼ä¸ô
        /// </summary>
        private float mDetectInterval = 0;
        protected override void Start()
        {
            base.Start();
        }
        protected override AbstractRecordData DefineDataType()
        {
            return new DefinedRecordData();
        }

        protected override void EnterRecording()
        {
            DetectInterval = mDetectInterval;
        }

        protected override void EnterWatching()
        {
        }

        protected override void ExitRecording()
        {
        }
    }
}

