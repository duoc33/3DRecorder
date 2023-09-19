using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// ·�����ݹ��� Model
    /// </summary>
    public class PathModel : AbstractModel
    {
        /// <summary>
        /// �ط����ݻ�����Ϣ·�����洢�˻طſ�ʼ���������Ϣ
        /// </summary>
        public string RecordInfoPath => SavePath + "/Info.info";
        /// <summary>
        /// �ط����ݵĴ洢�ļ���
        /// </summary>
        public string SavePath => Path.Combine(mFilePath, mFileName).Replace("\\", "/");
        private string mFilePath;
        private string mFileName;
        protected override void OnInit()
        {
            mFilePath = Application.streamingAssetsPath;//Ĭ��·��
            mFileName = "Record";//Ĭ�ϻط������ļ���
        }
        /// <summary>
        /// �����ļ�·��
        /// </summary>
        /// <param name="filePath"></param>
        public void SetFilePath(string filePath)=>mFilePath = filePath;
        /// <summary>
        /// �����ļ���
        /// </summary>
        /// <param name="fileName"></param>
        public void SetFileName(string fileName)=>mFileName = fileName;
        /// <summary>
        /// һ������
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

