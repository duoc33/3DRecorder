using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace Record
{
    public class DataWriteUtility : IUtility
    {
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
        public void WriteData(string path, AbstractRecordData recordData) 
        {
            if (recordData == null) return;
            byte[] tempData;
            using (FileStream fs = new FileStream(path,FileMode.Append,FileAccess.Write))
            {
                tempData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(recordData));
                fs.Write(BitConverter.GetBytes(tempData.Length));
                fs.Write(tempData);
                fs.Write(BitConverter.GetBytes(tempData.Length));
                fs.Flush();
                fs.Close();
            }
        }
    }
}

