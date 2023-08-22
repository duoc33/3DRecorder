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
        protected override AbstractRecordData DefineDataType()=> new DefinedRecordData();
        protected override AbstractRecordData DeserializeType(string Json) => JsonUtility.FromJson<DefinedRecordData>(Json);
    }
}

