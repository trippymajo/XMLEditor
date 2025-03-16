using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XMLEditor.Model;
using XMLEditor.View;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


namespace XMLEditor.Controller
{
    public class XmlController
    {
        private const short MAX_UR_HISTORY = 50;

        private readonly XmlModel _model;
        private readonly MainForm _view;

        private Stack<UndoRedo> _undoStack = new Stack<UndoRedo>();
        private Stack<UndoRedo> _redoStack = new Stack<UndoRedo>();


        public XmlController(MainForm view)
        {
            _view = view;
            _model = new XmlModel();
        }

        /// <summary>
        /// Open current XML document, load model and load into tree view
        /// </summary>
        /// <param name="fileName">Path to XML document</param>
        public void OpenXmlDoc(string fileName)
        {
            _model.LoadXml(fileName);
            _view.LoadXmlTreeView(_model.GetRoot());
        }

        /// <summary>
        /// Update text value of the selected element,
        /// store undo history and force to refresh the view
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="newText">New text to paste in the element</param>
        public void UpdateTextVal(XElement element, string newText)
        {
            XElement parent = element.Parent;
            if (parent == null)
                return;

            int index = parent.Elements().ToList().IndexOf(element);

            _undoStack.Push(new UndoRedo(new XElement(element), parent, index));

            if (_undoStack.Count > MAX_UR_HISTORY)
            {
                _undoStack = new Stack<UndoRedo>(_undoStack.Reverse().Skip(1).Reverse());
            }

            _redoStack.Clear(); // Clear redo after new change


            _model.UpdateTextVal(element, newText);
            _model.SaveXml(_model._filePath);
            _view.RefreshText();
        }

        /// <summary>
        /// Update text attribute of the selected element,
        /// store undo history and force to refresh the view
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="name">Name of the namespace attribute</param>
        /// <param name="newText">New text to paste in the element</param>
        public void UpdateTextAttr(XElement element, XName name, string newText)
        {
            XElement parent = element.Parent;
            if (parent == null)
                return;

            int index = parent.Elements().ToList().IndexOf(element);

            _undoStack.Push(new UndoRedo(new XElement(element), parent, index));

            if (_undoStack.Count > MAX_UR_HISTORY)
            {
                _undoStack = new Stack<UndoRedo>(_undoStack.Reverse().Skip(1).Reverse());
            }

            _redoStack.Clear(); // Clear redo after new change


            _model.UpdateTextAttr(element, name, newText);
            _model.SaveXml(_model._filePath);
            _view.RefreshText();
        }

        /// <summary>
        /// Undo the last change in the whole Xml tree, refresh the view
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                UndoRedo lastAction = _undoStack.Pop();
                _redoStack.Push(new UndoRedo(lastAction.Parent.Elements().ElementAt(lastAction.Index),
                                lastAction.Parent, lastAction.Index));

                lastAction.Parent.Elements().ElementAt(lastAction.Index).Remove();
                lastAction.Parent.Add(new XElement(lastAction.Node));

                _model.SaveXml(_model._filePath);
                _view.RefreshTree(_model.GetRoot());
            }
        }

        /// <summary>
        /// Redo the last change in the whole Xml tree, refresh the view
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                UndoRedo lastAction = _redoStack.Pop();
                _undoStack.Push(new UndoRedo(lastAction.Parent.Elements().ElementAt(lastAction.Index),
                                lastAction.Parent, lastAction.Index));

                lastAction.Parent.Elements().ElementAt(lastAction.Index).Remove();
                lastAction.Parent.Add(new XElement(lastAction.Node));

                _model.SaveXml(_model._filePath);
                _view.RefreshTree(_model.GetRoot());
            }
        }
    }

    public class UndoRedo
    {
        public XElement Node { get; }
        public XElement Parent { get; }
        public int Index { get; }

        public UndoRedo(XElement node, XElement parent, int index)
        {
            Node = new XElement(node);
            Parent = parent;
            Index = index;
        }
    }
}
