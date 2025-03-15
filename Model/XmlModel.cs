using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace XMLEditor.Model
{
    public class XmlModel
    {
        public string _filePath { get; private set; }
        private XElement _xmlDocument;

        /// <summary>
        /// Load XML from file
        /// </summary>
        /// <param name="filePath">File path of the xml to load</param>
        public void LoadXml(string filePath)
        {
            _xmlDocument = XElement.Load(filePath);
            _filePath = filePath;
        }

        /// <summary>
        /// Get XML root
        /// </summary>
        /// <returns>Current root of the xml element</returns>
        public XElement GetRoot()
        {
            return _xmlDocument;
        }

        /// <summary>
        /// Update text attribute of the selected element
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="name">Name of the namespace attribute</param>
        /// <param name="newText">New text to paste in the element</param>
        public void UpdateTextAttr(XElement element, XName name, string newText)
        {
            element.SetAttributeValue(name, newText);
        }

        /// <summary>
        /// Update text value of the selected element
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="newText">New text to paste in the element</param>
        public void UpdateTextVal(XElement element, string newText)
        {
            element.SetValue(newText);
        }

        /// <summary>
        /// Save the whole XML Document
        /// </summary>
        /// <param name="filePath">Where to save the document</param>
        public void SaveXml(string filePath)
        {
            _xmlDocument.Save(filePath);
        }
    }
}
