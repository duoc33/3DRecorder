using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 路径数据管理 Model
    /// </summary>
    public class PathModel : AbstractModel
    {
        /// <summary>
        /// 回放数据 基础信息路劲
        /// </summary>
        public string RecordInfoPath => SavePath + "/Info.info";
        /// <summary>
        /// 回放数据 存储文件夹
        /// </summary>
        public string SavePath => Path.Combine(mFilePath, mFileName).Replace("\\", "/");
        private string mFilePath;
        private string mFileName;
        protected override void OnInit()
        {
            mFilePath = Application.streamingAssetsPath;
            mFileName = "Record";
        }
        /// <summary>
        /// 设置文件路径
        /// </summary>
        /// <param name="filePath"></param>
        public void SetFilePath(string filePath)=>mFilePath = filePath;
        /// <summary>
        /// 设置文件名
        /// </summary>
        /// <param name="fileName"></param>
        public void SetFileName(string fileName)=>mFileName = fileName;
        /// <summary>
        /// 一起设置
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        public void SetSavePath(string filePath,string fileName) 
        {
            mFilePath = filePath;
            mFileName = fileName;
        }
    }
}

