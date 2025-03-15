using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace XMLEditor
{
	public partial class MainForm : Form
	{
		// Varibales:
		private const int MAX_HISTORY = 50;
		private string filePath = string.Empty;
		private CustomTreeNode root = new CustomTreeNode();
		private CustomTreeNode selectedTreeItem = null;
		private ListViewItemSelectionChangedEventArgs selectedViewItem = null;
		private Stack<XElement> undoStack = new Stack<XElement>();
		private Stack<XElement> redoStack = new Stack<XElement>();

		private static readonly HashSet<string> IgnoredElements = new HashSet<string>
		{
			"RibbonSeparator", "RibbonTabSelector", "RibbonPanelSourceReference",
			"DialogBoxLauncher", "RibbonPanelBreak", "RibbonTabSelectors"
		};

        // Classes:
        public class CustomTreeNode : TreeNode
		{
			public XElement xElement;
			/*
			public CustomTreeNode(String text) : base(text)
			{

			}
			public CustomTreeNode(XElement xelement) : base()
			{
				this.xElement = xelement;
			}
			*/
			public CustomTreeNode(String text, XElement xelement) : base(text) //call for the TreeNode Method
			{
				this.xElement = xelement;
			}
			public CustomTreeNode() : base() { } //call for the TreeNode Constructor
		}

		public MainForm()
		{
			InitializeComponent();
		}

		private bool isIgnored(string xElementName) => IgnoredElements.Contains(xElementName);

		private void makeXmlTree(string filePath)
		{
			using (XmlReader reader = XmlReader.Create(filePath))
			{
				XElement xmlDoc = XElement.Load(reader);
				treeView.Nodes.Clear();
				root = new CustomTreeNode(xmlDoc.Name.LocalName, xmlDoc);
				getEveryNode(root, xmlDoc);
				treeView.Nodes.Add(root);
				root.Expand();
			}
		}

		private void getEveryNode(CustomTreeNode root, XElement element)
		{
			CustomTreeNode node;
			foreach (XElement item in element.Elements())
			{
				//                ListItems.Items.Add(item.Value, "TRUE".Equals(item.Attribute("checked").Value.ToUpper()));
				if (!isIgnored(item.Name.ToString()))
				{
					node = new CustomTreeNode(item.Name.LocalName.ToString(), item);
					root.Nodes.Add(node);
					getEveryNode(node, item);
				}
			}
		}

		private void openXmlToolStripMenuItem_Click(object sender, EventArgs e)
		{

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				//Get the path of specified file
				filePath = openFileDialog.FileName;
				makeXmlTree(filePath);
			}
		}

		private void EditBtn_Click(object sender, EventArgs e)
		{
			CustomTreeNode node = selectedTreeItem;
			XNamespace XNs = node.xElement.Name.Namespace;
			ListViewItem item = selectedViewItem.Item;

			undoStack.Push(new XElement(node.xElement));
			if (undoStack.Count > MAX_HISTORY)
			{
				undoStack = new Stack<XElement>(undoStack.Reverse().Skip(1).Reverse());
			}
			redoStack.Clear();

			if (item.SubItems[0].Text.ToLower().Equals("<text>"))
			{
				selectedTreeItem.xElement.SetValue(inputBox.Text);
			}
			else
			{
				node.xElement.SetAttributeValue(XNs + item.SubItems[0].Text, inputBox.Text);
			}

			root.xElement.Save(filePath);
			getTextFromXml(node);
			listView.Focus();
		}

		private void getTextFromXml(CustomTreeNode selectedTreeItem)
		{
			listView.Items.Clear();
			CustomTreeNode selected = selectedTreeItem;
			ListViewItem item = null;
			if (!selected.xElement.HasElements)
			{
				string[] text = { "<Text>", selected.xElement.Value };
				if (text[1] != "")
				{
					item = new ListViewItem(text);
					listView.Items.Add(item);
				}
			}
			foreach (XAttribute attrib in selected.xElement.Attributes())
			{
				string attr = attrib.Value;
				if (attrib.Name.LocalName.ToString() == "Text")
				{
					string[] text1 = { attrib.Name.LocalName.ToString(), attr };
					item = new ListViewItem(text1);
					listView.Items.Add(item);
				}
			}

		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			inputBox.Clear();
			selectedTreeItem = (CustomTreeNode)e.Node;
			getTextFromXml(selectedTreeItem);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm AboutForm = new AboutForm();
			AboutForm.ShowDialog();
		}

		private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			selectedViewItem = e;
			inputBox.Text = e.Item.SubItems[1].Text;
		}

		private void UndoBttn_Click(object sender, EventArgs e)
		{
			if (undoStack.Count > 0)
			{
				selectedTreeItem.xElement.ReplaceWith(undoStack.Pop());
				root.xElement.Save(filePath);
				getTextFromXml(selectedTreeItem);
				listView.Focus();
			}
		}

		private void RedoBttn_Click(object sender, EventArgs e)
		{
			if (redoStack.Count > 0)
			{
				undoStack.Push(new XElement(selectedTreeItem.xElement));

				if (undoStack.Count > MAX_HISTORY)
				{
					undoStack = new Stack<XElement>(undoStack.Reverse().Skip(1).Reverse());
				}

				XElement nextState = redoStack.Pop();
				selectedTreeItem.xElement.ReplaceWith(nextState);

				root.xElement.Save(filePath);
				getTextFromXml(selectedTreeItem);
				listView.Focus();
			}
		}
	}
}
