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
    /// д�����ݵķ��������������ݴ洢��ʽ,����һЩ�ļ���ʼ��
    /// </summary>
    public class DataWriteUtility : IUtility
    {
        /// <summary>
        /// ��ʼ���ļ�,�������
        /// </summary>
        /// <param name="path">·��</param>
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
                //һ�㶼������������ļ������ͬһ·������ȫ��ɾ��
                using (FileStream fs = File.Create(path))
                {
                    fs.Flush();
                    fs.Close();
                }
            }
        }
        /// <summary>
        /// ����д��ʱ��
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
        /// д�����ݣ�ֻ��Ҫ�ֽڣ������ݼ̳���AbstractData
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
        /// д��ʵ����ʱ��ͱ�����ʱ�䣬����һ��ʼ�����
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

