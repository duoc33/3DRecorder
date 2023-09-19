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
        /// 回放数据基础信息路径：存储了回放开始或结束的信息
        /// </summary>
        public string RecordInfoPath => SavePath + "/Info.info";
        /// <summary>
        /// 回放数据的存储文件夹
        /// </summary>
        public string SavePath => Path.Combine(mFilePath, mFileName).Replace("\\", "/");
        private string mFilePath;
        private string mFileName;
        protected override void OnInit()
        {
            mFilePath = Application.streamingAssetsPath;//默认路劲
            mFileName = "Record";//默认回放数据文件名
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

