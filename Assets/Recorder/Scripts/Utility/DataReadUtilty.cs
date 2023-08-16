using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static Record.RecordObjectLoadPool;

namespace Record
{
    public class DataReadUtility : IUtility
    {
        /// <summary>
        /// ͨ�������ļ����ViewID�����ǲ�ͬʵ�������ݶ�ȡ·���ֵ�
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public Dictionary<int, List<ViewIDToIntantiatedID>> ReadAllFileName(string directoryPath) 
        {
            long tempIndex = 0;
            Dictionary<int, List<ViewIDToIntantiatedID>> tempDic = new Dictionary<int, List<ViewIDToIntantiatedID>>();
            ViewIDToIntantiatedID viewIDToIntantiatedID;
            string[] files = Directory.GetFiles(directoryPath);
            if (files.Length <= 0) {
                Debug.LogError(directoryPath+" ��û���ҵ��ļ�");
                return null;
            }
            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;
                string temp = string.Empty;
                temp = file.Replace("\\", "/");
                string tempRead = temp;
                temp = temp.Substring(directoryPath.Length + 1);
                string[] tempGroup = temp.Split('_');
                
                long length = 0;
                int viewID = int.Parse(tempGroup[0]);
                int instantiatedID = int.Parse(tempGroup[1].Split(".")[0]);
                int tempTime = Mathf.FloorToInt(ReadFirstData(tempRead, ref tempIndex).TheDataBeAddedTime);
                using (FileStream fs = new FileStream(tempRead,FileMode.Open,FileAccess.Read))
                {
                    length = fs.Length;
                    fs.Flush();
                    fs.Close();
                }
                if (tempDic.ContainsKey(tempTime))
                {
                    viewIDToIntantiatedID = new ViewIDToIntantiatedID() { ID = viewID, IntantiatedIndex = instantiatedID, Path = tempRead,StreamLength = length};
                    tempDic[tempTime].Add(viewIDToIntantiatedID);
                }
                else {
                    tempDic[tempTime] = new List<ViewIDToIntantiatedID>();
                    viewIDToIntantiatedID = new ViewIDToIntantiatedID() { ID = viewID, IntantiatedIndex = instantiatedID, Path = tempRead, StreamLength = length };
                    tempDic[tempTime].Add(viewIDToIntantiatedID);
                }
            }
            return tempDic;
        }
        /// <summary>
        /// �����ļ�����Ϣ����ȡRcorderDataReader
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="singleObjectInfos"></param>
        public void ReadFileConfigTable(string directoryPath,ref RcorderDataReader DataReader) 
        {
            string[] files = Directory.GetFiles(directoryPath);
            if (files.Length <= 0)
            {
                Debug.LogError(directoryPath + " ��û���ҵ��ļ�");
                return;
            }
            long tempIndex = 0;
            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;
                string temp = string.Empty;
                temp = file.Replace("\\", "/");
                string tempRead = temp;
                temp = temp.Substring(directoryPath.Length + 1);
                string[] tempGroup = temp.Split('_');

                long length;
                int viewID = int.Parse(tempGroup[0]);
                int instantiatedID = int.Parse(tempGroup[1].Split(".")[0]);
                using (FileStream fs = new FileStream(tempRead, FileMode.Open, FileAccess.Read))
                {
                    length = fs.Length;
                    fs.Flush();
                    fs.Close();
                }
                if (length<=100) 
                {
                    Debug.Log("���齫���Ϊ -- "+ viewID +" --�ĸ���������Ϊ\"��¼\"��̬");
                }
                int tempInsTime = Mathf.FloorToInt(ReadFirstData(tempRead, ref tempIndex).TheDataBeAddedTime);
                int tempDesTime = Mathf.CeilToInt(ReadLastData(tempRead, ref tempIndex).TheDataBeAddedTime);
                DataReader.Add(
                    new SingleObjectInfo() {
                        ViewID= viewID,
                        InstantiatedID = instantiatedID,
                        StreamLength = length,
                        InstantiatedTime = tempInsTime,
                        DestoryedTime = tempDesTime,
                        ReadPath = tempRead,
                    }
                );
            }
        }
        public AbstractRecordData ReadNextData(string path,ref long currentStreamIndex) 
        {
            if (!JudgeFile(path)) return null;
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length <= 0) return null;
                if (currentStreamIndex >= fs.Length - 4) {currentStreamIndex = fs.Length - 4; return null;}
                fs.Seek(currentStreamIndex, SeekOrigin.Begin);
                byte[] len = new byte[4];
                fs.Read(len);
                int Length = BitConverter.ToInt32(len);
                data = new byte[Length];
                fs.Read(data);
                currentStreamIndex = fs.Seek(4, SeekOrigin.Current);
            }
            return JsonUtility.FromJson<AbstractRecordData>(Encoding.UTF8.GetString(data));
        }
        public AbstractRecordData ReadPreData(string path,ref long currentStreamIndex)
        {
            if (!JudgeFile(path)) return null;
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length <= 0) return null;
                if (currentStreamIndex<=0) { currentStreamIndex = 0; return null; }
                byte[] len = new byte[4];
                fs.Seek(currentStreamIndex, SeekOrigin.Begin);
                fs.Seek(-4, SeekOrigin.Current);
                fs.Read(len);
                int Length = BitConverter.ToInt32(len);
                data = new byte[Length];
                fs.Seek(-4 - Length, SeekOrigin.Current);
                fs.Read(data);
                currentStreamIndex = fs.Seek(-4 - 4 - Length, SeekOrigin.Current);
            }
            return JsonUtility.FromJson<AbstractRecordData>(Encoding.UTF8.GetString(data));
        }
        public AbstractRecordData ReadFirstData(string path,ref long currentStreamIndex)
        {
            if (!JudgeFile(path)) return null;
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length <= 0) return null;
                byte[] len = new byte[4];
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(len);
                int Length = BitConverter.ToInt32(len);
                data = new byte[Length];
                fs.Read(data);
                currentStreamIndex =fs.Position+4;
                fs.Flush();
                fs.Close();
            }
            return JsonUtility.FromJson<AbstractRecordData>(Encoding.UTF8.GetString(data));
        }
        public AbstractRecordData ReadLastData(string path, ref long currentStreamIndex)
        {
            if(!JudgeFile(path)) return null;
            byte[] data;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if(fs.Length<=0)return null;
                byte[] len = new byte[4];
                fs.Seek(-4, SeekOrigin.End);
                fs.Read(len);
                int length = BitConverter.ToInt32(len);
                data = new byte[length];
                fs.Seek(-4-length, SeekOrigin.End);
                fs.Read(data);
                currentStreamIndex = fs.Position + 4;
                fs.Flush();
                fs.Close();
            }
            return JsonUtility.FromJson<AbstractRecordData>(Encoding.UTF8.GetString(data));
        }
        
        private bool JudgeFile(string path) 
        {
            if (!File.Exists(path)) return false;
            else return true;
        }
    }
}

