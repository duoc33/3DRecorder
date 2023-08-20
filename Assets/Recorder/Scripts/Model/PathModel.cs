using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Record
{
    public class PathModel : AbstractModel
    {
        public string SavePath => Path.Combine(mFilePath, mFileName).Replace("\\", "/");
        private string mFilePath;
        private string mFileName;
        protected override void OnInit()
        {
            mFilePath = Application.streamingAssetsPath;
            mFileName = "Record";
        }
        public void SetFilePath(string filePath)=>mFilePath = filePath;
        public void SetFileName(string fileName)=>mFileName = fileName;
        public void SetSavePath(string filePath,string fileName) 
        {
            mFilePath = filePath;
            mFileName = fileName;
        }
    }
}

