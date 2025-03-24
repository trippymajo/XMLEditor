using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XMLEditor.Model;


namespace XMLEditor.Controller
{
    public interface IXmlController
    {
        /// <summary>
        /// Open current XML document, load model and load into tree view
        /// </summary>
        /// <param name="fileName">Path to XML document</param>
        /// <returns>Root of the opened document</returns>
        XElement OpenXmlDoc(string fileName);

        /// <summary>
        /// Update text value of the selected element,
        /// store undo history and force to refresh the view
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="newText">New text to paste in the element</param>
        void UpdateTextVal(XElement element, string newText);

        /// <summary>
        /// Update text attribute of the selected element,
        /// store undo history and force to refresh the view
        /// </summary>
        /// <param name="element">Current element</param>
        /// <param name="name">Name of the namespace attribute</param>
        /// <param name="newText">New text to paste in the element</param>
        void UpdateTextAttr(XElement element, XName name, string newText);

        /// <summary>
        /// Undo the last change in the whole Xml tree, refresh the view
        /// </summary>
        /// <returns>
        /// Root of the opened document. Null if nothing to Undo
        /// </returns>
        XElement Undo();

        /// <summary>
        /// Redo the last change in the whole Xml tree, refresh the view
        /// </summary>
        /// <returns>
        /// Root of the opened document. Null if nothing to Redo
        /// </returns>
        XElement Redo();
    }

    public class XmlController : IXmlController
    {
        private const short MAX_UR_HISTORY = 50;

        private readonly IXmlModel _model;

        private Stack<UndoRedo> _undoStack = new Stack<UndoRedo>();
        private Stack<UndoRedo> _redoStack = new Stack<UndoRedo>();


        public XmlController(IXmlModel model)
        {
            _model = model;
        }

        public XElement OpenXmlDoc(string fileName)
        {
            _model.LoadXml(fileName);
            return _model.GetRoot();
        }

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
        }

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
        }

        public XElement Undo()
        {
            if (_undoStack.Count > 0)
            {
                UndoRedo lastAction = _undoStack.Pop();
                _redoStack.Push(new UndoRedo(lastAction.Parent.Elements().ElementAt(lastAction.Index),
                                lastAction.Parent, lastAction.Index));

                lastAction.Parent.Elements().ElementAt(lastAction.Index).Remove();
                lastAction.Parent.Add(new XElement(lastAction.Node));

                _model.SaveXml(_model._filePath);
                return _model.GetRoot();
            }
            return null;
        }

        public XElement Redo()
        {
            if (_redoStack.Count > 0)
            {
                UndoRedo lastAction = _redoStack.Pop();
                _undoStack.Push(new UndoRedo(lastAction.Parent.Elements().ElementAt(lastAction.Index),
                                lastAction.Parent, lastAction.Index));

                lastAction.Parent.Elements().ElementAt(lastAction.Index).Remove();
                lastAction.Parent.Add(new XElement(lastAction.Node));

                _model.SaveXml(_model._filePath);
                return _model.GetRoot();
            }
            return null;
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
