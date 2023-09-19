using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 写入数据的方法，定义了数据存储格式,还有一些文件初始化
    /// </summary>
    public class DataWriteUtility : IUtility
    {
        /// <summary>
        /// 初始化文件,如果存在
        /// </summary>
        /// <param name="path">路径</param>
        public void InitFile(string path) 
        {
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path,FileMode.Truncate,FileAccess.Write))
                {
                    fs.SetLength(0);
                    fs.Flush();
                    fs.Close();
                }
            }
            else
            {
                //一般都会走这里，其他文件如果在同一路径，会全部删除
                using (FileStream fs = File.Create(path))
                {
                    fs.Flush();
                    fs.Close();
                }
            }
        }
        /// <summary>
        /// 用在写的时候
        /// </summary>
        /// <param name="SavePath"></param>
        public void InitDirectory(string SavePath) 
        {
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            else {
                string[] paths = Directory.GetFiles(SavePath);
                foreach (var item in paths)
                {
                    File.Delete(item);
                }
            }
        }
        /// <summary>
        /// 写入数据，只需要字节，该数据继承自AbstractData
        /// </summary>
        /// <param name="path"></param>
        /// <param name="AbstractData"></param>
        public void WriteData(string path, byte[] AbstractData) 
        {
            if (AbstractData.Length<=0) return;
            using (FileStream fs = new FileStream(path,FileMode.Append,FileAccess.Write))
            {
                fs.Write(BitConverter.GetBytes(AbstractData.Length),0,4);
                fs.Write(AbstractData,0,AbstractData.Length);
                fs.Write(BitConverter.GetBytes(AbstractData.Length),0,4);
                fs.Flush();
                fs.Close();
            }
        }
        /// <summary>
        /// 写入实例化时间和被销毁时间，用在一开始和最后
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Time"></param>
        public void WriteHead(string path,int Time) 
        {
            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                fs.Write(BitConverter.GetBytes(Time));
                fs.Flush();
                fs.Close();
            }
        }
    }
}

