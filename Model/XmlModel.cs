using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace XMLEditor.Model
{
    public interface IXmlModel
    {
        string _filePath { get; }

        /// <summary>
        /// Update text attribute of the selected element
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="name">Name of the namespace attribute</param>
        /// <param name="newText">New text to paste in the element</param>
        void UpdateTextAttr(XElement element, XName name, string newText);

        /// <summary>
        /// Update text value of the selected element
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="newText">New text to paste in the element</param>
        void UpdateTextVal(XElement element, string newText);

        /// <summary>
        /// Load XML from file
        /// </summary>
        /// <param name="filePath">File path of the xml to load</param>
        void LoadXml(string filePath);

        /// <summary>
        /// Save the whole XML Document
        /// </summary>
        /// <param name="filePath">Where to save the document</param>
        void SaveXml(string filePath);

        /// <summary>
        /// Get XML root
        /// </summary>
        /// <returns>Current root of the xml element</returns>
        XElement GetRoot();

    }

    public class XmlModel : IXmlModel
    {
        public string _filePath { get; private set; }
        private XElement _xmlDocument;

        public void LoadXml(string filePath)
        {
            _xmlDocument = XElement.Load(filePath);
            _filePath = filePath;
        }

        public XElement GetRoot()
        {
            return _xmlDocument;
        }

        public void UpdateTextAttr(XElement element, XName name, string newText)
        {
            element.SetAttributeValue(name, newText);
        }

        public void UpdateTextVal(XElement element, string newText)
        {
            element.SetValue(newText);
        }

        public void SaveXml(string filePath)
        {
            _xmlDocument.Save(filePath);
        }
    }
}
