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
        protected override AbstractRecordData InitRecordDataType()
        {
            return new DefinedRecordData();
        }
        protected override void RecordObjectEnterPause()
        {
            base.RecordObjectEnterPause();
        }
        protected override void RecordObjectEnterRecording()
        {
            DetectInterval = mDetectInterval;
            base.RecordObjectEnterRecording();
        }
        protected override void RecordObjectEnterResume()
        {
            base.RecordObjectEnterResume();
        }
        protected override void RecordObjectEnterWatching()
        {
            base.RecordObjectEnterWatching();
        }
    }
}

