using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Record
{
    public class DataReadUtility : IUtility
    {
        /// <summary>
        /// 根据文件夹信息，读取RcorderDataReader
        /// 一次性能够初始化ViewID,InstantiatedID，StreamLength，InstantiatedTime，DestoryedTime，ReadPath
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="singleObjectInfos"></param>
        public IEnumerator ReadFileConfigTable(string directoryPath, RecorderDataReader DataReader)
        {
            string[] files = Directory.GetFiles(directoryPath);
            if (files.Length <= 0)
            {
                Debug.LogError(directoryPath + " 下没有找到文件");
                yield break;
            }
            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;
                if (file.EndsWith(".info")) continue;
                string temp = string.Empty;
                temp = file.Replace("\\", "/");
                string tempRead = temp;
                temp = temp.Substring(directoryPath.Length + 1);
                string[] tempGroup = temp.Split('_');

                long length;
                int viewID = int.Parse(tempGroup[0]);
                int instantiatedID = int.Parse(tempGroup[1]);
                int beInstantiatedID = int.Parse(tempGroup[2].Split(".")[0]);
                int tempInsTime;
                int tempDesTime;
                using (FileStream fs = new FileStream(tempRead, FileMode.Open, FileAccess.Read))
                {
                    byte[] timeInfo = new byte[4];

                    length = fs.Length;

                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Read(timeInfo);
                    tempInsTime = BitConverter.ToInt32(timeInfo);

                    fs.Seek(-4, SeekOrigin.End);
                    fs.Read(timeInfo);
                    tempDesTime = BitConverter.ToInt32(timeInfo);
                    fs.Flush();
                    fs.Close();
                }
                DataReader.Add(
                    new SingleObjectInfo() {
                        ViewID = viewID,
                        InstantiatedID = instantiatedID,
                        StreamLength = length,
                        InstantiatedTime = tempInsTime,
                        BeInstantiatedID = beInstantiatedID,
                        DestoryedTime = tempDesTime,
                        ReadPath = tempRead,
                    }
                );
                yield return null;
            }
        }
        public string ReadNextData(string path,ref long mCurrentIndexInStream)
        {
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length <= 0) {
                    return null;
                }
                if (mCurrentIndexInStream >= fs.Length - 4) {
                    mCurrentIndexInStream = fs.Length - 4;
                    return null;
                }
                if (mCurrentIndexInStream < 4) {
                    mCurrentIndexInStream = 4;
                    return null;
                }
                fs.Seek(mCurrentIndexInStream, SeekOrigin.Begin);
                byte[] len = new byte[4];
                fs.Read(len, 0, 4);
                int Length = BitConverter.ToInt32(len);
                data = new byte[Length];
                fs.Read(data, 0, Length);
                fs.Seek(4, SeekOrigin.Current);
                mCurrentIndexInStream = mCurrentIndexInStream + Length + 8;
                fs.Flush();
                fs.Close();
            }
            return Encoding.UTF8.GetString(data);
        }
        public string ReadPreData(string path, ref long mCurrentIndexInStream)
        {
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length <= 0) return null;
                if (mCurrentIndexInStream <= 4) {
                    mCurrentIndexInStream = 4;
                    return null;
                }
                if (mCurrentIndexInStream > fs.Length - 4) {
                    mCurrentIndexInStream = fs.Length - 4;
                    return null;
                }
                byte[] len = new byte[4];
                fs.Seek(mCurrentIndexInStream, SeekOrigin.Begin);
                fs.Seek(-4, SeekOrigin.Current);
                fs.Read(len, 0, 4);
                int Length = BitConverter.ToInt32(len);
                fs.Seek(-4 - Length, SeekOrigin.Current);
                data = new byte[Length];
                fs.Read(data);
                mCurrentIndexInStream = mCurrentIndexInStream - 8 - Length;
                fs.Flush();
                fs.Close();
            }
            return Encoding.UTF8.GetString(data);
        }
        public string ReadFirstData(string path, ref long mCurrentIndexInStream)
        {
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length <= 0) return null;
                byte[] len = new byte[4];
                fs.Seek(4, SeekOrigin.Begin);
                fs.Read(len);
                int Length = BitConverter.ToInt32(len);
                data = new byte[Length];
                fs.Read(data);
                mCurrentIndexInStream = fs.Position + 4;
                fs.Flush();
                fs.Close();
            }
            return Encoding.UTF8.GetString(data);
        }
        public string ReadLastData(string path, ref long mCurrentIndexInStream)
        {
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length <= 0) return null;
                byte[] len = new byte[4];
                fs.Seek(-8, SeekOrigin.End);
                fs.Read(len);
                int length = BitConverter.ToInt32(len);
                data = new byte[length];
                fs.Seek(-4 - length, SeekOrigin.Current);
                fs.Read(data);
                mCurrentIndexInStream = fs.Position + 4;
                fs.Flush();
                fs.Close();
            }
            return Encoding.UTF8.GetString(data);
        }
        public void ReadByTime(string path, Func<byte[], bool> ReadByTimeHandler, ref long mCurrenIndexInStream)
        {
            mCurrenIndexInStream = 4;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] lenBytes = new byte[4];
                byte[] datas;
                while (fs.Position < fs.Length - 4)
                {
                    fs.Read(lenBytes);
                    int Length = BitConverter.ToInt32(lenBytes);
                    datas = new byte[Length];
                    fs.Read(datas);
                    if (ReadByTimeHandler == null) {
                        Debug.LogError("请写入比较方法");
                        return;
                    }
                    if (ReadByTimeHandler.Invoke(datas)) {
                        mCurrenIndexInStream = fs.Position + 4;
                        return;
                    }
                    fs.Position += 4;
                }
                mCurrenIndexInStream = fs.Position;
                fs.Flush();
                fs.Close();
            }
        }
        public int ReadInfo(string path) 
        {
            if(!File.Exists(path))
            {
                Debug.LogError("没有path路径的info: " +path);
                return 0;
            }
            byte[] bytes;
            using (FileStream fs = new FileStream(path,FileMode.Open,FileAccess.Read))
            {
                bytes = new byte[4];
                fs.Read(bytes);
                fs.Flush();
                fs.Close();
            }
            return BitConverter.ToInt32(bytes);
        }
    }
}

