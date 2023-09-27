using System;
using System.Globalization;
using UnityEngine;

namespace Record
{
    /// <summary>
    /// 自定义数据，比较方式、获取方式、应用方式
    /// </summary>
    [Serializable]
    public class DefinedRecordData : AbstractRecordData
    {
        //颜色Hex字符串
        public string Color; 
        /// <summary>
        /// 赋值方式
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="other"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void AssignData(AbstractRecordData origin, AbstractRecordData other) 
        {
            (origin as DefinedRecordData).Color = (other as DefinedRecordData).Color;
        }
        /// <summary>
        /// 增加数据比较方式
        /// </summary>
        /// <param name="other">另一个数据</param>
        /// <param name="transform">当前回放对象Trans组件</param>
        /// <returns></returns>
        protected override bool Comparer(AbstractRecordData origin, AbstractRecordData other) 
        {
            if((origin as DefinedRecordData).Color.Equals((other as DefinedRecordData).Color) )
                return true;
            return false;
        }
        /// <summary>
        /// 给新增加的数据定义recordData的赋值方式
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void GetRecordData(AbstractRecordData recordData, Transform transform) 
        {
            Color color = transform.GetComponent<MeshRenderer>().sharedMaterial.color;
            (recordData as DefinedRecordData).Color = ColorToHex(color);
        }
        /// <summary>
        /// 给新增加的数据定义Transform的赋值方式
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void SetRecordData(Transform transform,AbstractRecordData recordData) 
        {
            transform.GetComponent<MeshRenderer>().sharedMaterial.color = HexToColor((recordData as DefinedRecordData).Color);
        }
        /// <summary>
        /// 颜色转16进制字符串
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private string ColorToHex(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        }
        /// <summary>
        /// 16进制字符串转颜色
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

