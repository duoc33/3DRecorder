using System;
using System.Globalization;
using UnityEngine;

namespace Record
{
    /// <summary>
    /// �Զ������ݣ��ȽϷ�ʽ����ȡ��ʽ��Ӧ�÷�ʽ
    /// </summary>
    [Serializable]
    public class DefinedRecordData : AbstractRecordData
    {
        //��ɫHex�ַ���
        public string Color; 
        /// <summary>
        /// ��ֵ��ʽ
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="other"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void AssignData(AbstractRecordData origin, AbstractRecordData other) 
        {
            (origin as DefinedRecordData).Color = (other as DefinedRecordData).Color;
        }
        /// <summary>
        /// �������ݱȽϷ�ʽ
        /// </summary>
        /// <param name="other">��һ������</param>
        /// <param name="transform">��ǰ�طŶ���Trans���</param>
        /// <returns></returns>
        protected override bool Comparer(AbstractRecordData origin, AbstractRecordData other) 
        {
            if((origin as DefinedRecordData).Color.Equals((other as DefinedRecordData).Color) )
                return true;
            return false;
        }
        /// <summary>
        /// �������ӵ����ݶ���recordData�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void GetRecordData(AbstractRecordData recordData, Transform transform) 
        {
            Color color = transform.GetComponent<MeshRenderer>().sharedMaterial.color;
            (recordData as DefinedRecordData).Color = ColorToHex(color);
        }
        /// <summary>
        /// �������ӵ����ݶ���Transform�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void SetRecordData(Transform transform,AbstractRecordData recordData) 
        {
            transform.GetComponent<MeshRenderer>().sharedMaterial.color = HexToColor((recordData as DefinedRecordData).Color);
        }
        /// <summary>
        /// ��ɫת16�����ַ���
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private string ColorToHex(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        }
        /// <summary>
        /// 16�����ַ���ת��ɫ
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", string.Empty);
            hex = hex.Replace("#", string.Empty);
            byte a = byte.MaxValue;
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }
    }
}

