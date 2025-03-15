using System.Windows.Forms;
using System.Xml.Linq;

namespace XMLEditor.ViewModel
{
    public class CustomTreeNode : TreeNode
    {
        public XElement XElement { get; private set; }

        public CustomTreeNode(string text, XElement xelement) : base(text) //call for the TreeNode Method
        {
            XElement = xelement;
        }
        public CustomTreeNode() : base() { } //call for the TreeNode Constructor
    }
}
