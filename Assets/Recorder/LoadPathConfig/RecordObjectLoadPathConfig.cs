using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Record
{
    [CreateAssetMenu(fileName = "RecordConfig",menuName = "CreateRecordConfig")]
    public class RecordObjectLoadPathConfig : ScriptableObject
    {
        [SerializeField]
        public List<RecordObjectInfo> RecordObjects = new List<RecordObjectInfo>();
    }
    [Serializable]
    public class RecordObjectInfo
    {
        public string LoadPath;
        public List<int> ViewIDs = new List<int>();
        private bool mIsInstantiated = false;
        public bool IsInstantiated => mIsInstantiated;
        public bool ViewIDIsBelong(int id) 
        {
            foreach (var item in this.ViewIDs)
            {
                if (item == id) {
                    this.mIsInstantiated = true;
                    return true;
                }
            }
            return false;
        }
    }
}

