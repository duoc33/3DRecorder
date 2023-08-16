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
        /// ViewID = -3 ��ʾδ����ViewID
        /// </summary>
        public int ViewID = -3;
        /// <summary>
        /// Clone�����ĵڼ�����ID
        /// </summary>
        [HideInInspector]
        public int InstantiatedID = 0;
        
        public IArchitecture GetArchitecture() => RecordApp.Interface;
    }
}

