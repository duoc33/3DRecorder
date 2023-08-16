using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{

    [DisallowMultipleComponent]
    public class BaseView : MonoBehaviour, IController
    {
        [HideInInspector]
        /// <summary>
        /// ViewID = -3 表示未设置ViewID
        /// </summary>
        public int ViewID = -3;
        /// <summary>
        /// Clone出来的第几个的ID
        /// </summary>
        [HideInInspector]
        public int InstantiatedID = 0;
        
        public IArchitecture GetArchitecture() => RecordApp.Interface;
    }
}

