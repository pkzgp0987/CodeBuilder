using System;
using System.Collections.Generic;
using System.Text;

namespace CodeHelper
{
    /// <summary>
    /// �ֶ���Ϣ
    /// </summary>
    public class ColumnInfo
    {
        private string _colorder;
        private string _columnName;
        private string _typeName = "";
        private string _length = "";
        private string _preci = "";
        private string _scale = "";
        private bool _isIdentity;
        private bool _ispk;
        private bool _cisNull;
        private string _defaultVal = "";
        private string _deText = "";

        /// <summary>
        /// ���
        /// </summary>
        public string Colorder
        {
            set { _colorder = value; }
            get { return _colorder; }
        }
        /// <summary>
        /// �ֶ���
        /// </summary>
        public string ColumnName
        {
            set { _columnName = value; }
            get { return _columnName; }
        }
        /// <summary>
        /// �ֶ�����
        /// </summary>
        public string TypeName
        {
            set { _typeName = value; }
            get { return _typeName; }
        }
        /// <summary>
        /// ����
        /// </summary>
        public string Length
        {
            set { _length = value; }
            get { return _length; }
        }
        /// <summary>
        /// ����
        /// </summary>
        public string Preci
        {
            set { _preci = value; }
            get { return _preci; }
        }
        /// <summary>
        /// С��λ��
        /// </summary>
        public string Scale
        {
            set { _scale = value; }
            get { return _scale; }
        }
        /// <summary>
        /// �Ƿ��Ǳ�ʶ��
        /// </summary>
        public bool IsIdentity
        {
            set { _isIdentity = value; }
            get { return _isIdentity; }
        }
        /// <summary>
        /// �Ƿ�������
        /// </summary>
        public bool IsPK
        {
            set { _ispk = value; }
            get { return _ispk; }
        }
        /// <summary>
        /// �Ƿ������
        /// </summary>
        public bool cisNull
        {
            set { _cisNull = value; }
            get { return _cisNull; }
        }
        /// <summary>
        /// Ĭ��ֵ
        /// </summary>
        public string DefaultVal
        {
            set { _defaultVal = value; }
            get { return _defaultVal; }
        }
        /// <summary>
        /// ��ע
        /// </summary>
        public string DeText
        {
            set { _deText = value; }
            get { return _deText; }
        }
    }
}
