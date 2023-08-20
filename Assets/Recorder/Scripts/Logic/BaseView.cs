using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    [DisallowMultipleComponent]
    public class BaseView : MonoBehaviour, IController
    {
        public IArchitecture GetArchitecture() => RecordApp.Interface;
    }
}

