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
using XMLEditor.Controller;
using XMLEditor.ViewModel;

namespace XMLEditor.View
{
	public partial class MainForm : Form
	{
		private XmlController _controller = null;

		// Varibales:
		private const int MAX_HISTORY = 50;
		private string filePath = string.Empty;
		//private CustomTreeNode root = new CustomTreeNode();
		private CustomTreeNode selectedTreeItem = null;
		private ListViewItemSelectionChangedEventArgs selectedListViewItem = null;
		private Stack<XElement> undoStack = new Stack<XElement>();
		private Stack<XElement> redoStack = new Stack<XElement>();

		private static readonly HashSet<string> IgnoredElements = new HashSet<string>
		{
			"RibbonSeparator", "RibbonTabSelector", "RibbonPanelSourceReference",
			"DialogBoxLauncher", "RibbonPanelBreak", "RibbonTabSelectors"
		};

		public MainForm()
		{
			InitializeComponent();
			_controller = new XmlController(this);
		}

		private bool isIgnored(string xElementName) => IgnoredElements.Contains(xElementName);

		private void PopulateTreeView(CustomTreeNode treeNode, XElement elemNode)
		{
			foreach (XElement item in elemNode.Elements())
			{
				if (!isIgnored(item.Name.ToString()))
				{
					CustomTreeNode childNode = new CustomTreeNode(item.Name.LocalName, item);
					treeNode.Nodes.Add(childNode);
					PopulateTreeView(childNode, item);
				}
			}
		}

		public void LoadXmlTreeView(XElement elemRoot)
		{
			if (elemRoot == null)
				return;

			treeView.Nodes.Clear();

			CustomTreeNode treeRoot = new CustomTreeNode(elemRoot.Name.LocalName, elemRoot);
			treeView.Nodes.Add(treeRoot);
			PopulateTreeView(treeRoot, elemRoot);
			treeRoot.Expand();
		}

		private void openXmlToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				_controller.OpenXmlDoc(openFileDialog.FileName);
			}
		}

		private void EditBtn_Click(object sender, EventArgs e)
		{
			if (_controller == null || selectedListViewItem == null)
			{
				inputBox.Clear();
				return;
			}

			if (selectedListViewItem.Item.SubItems[0].Text.ToLower().Equals("<text>"))
			{
				_controller.UpdateTextVal(selectedTreeItem.XElement, inputBox.Text);
			}
			else
			{
				XNamespace XNs = selectedTreeItem.XElement.Name.Namespace;
				XName namespaceNameAttr = XNs.ToString();
				namespaceNameAttr += selectedListViewItem.Item.SubItems[0].Text;

				_controller.UpdateTextAttr(selectedTreeItem.XElement, namespaceNameAttr, inputBox.Text);
			}
		}

		public void RefreshText()
		{
			listView.Items.Clear();
			ListViewItem item = null;
			if (!selectedTreeItem.XElement.HasElements)
			{
				string[] text = { "<Text>", selectedTreeItem.XElement.Value };
				if (!string.IsNullOrEmpty(text[1]))
				{
					item = new ListViewItem(text);
					listView.Items.Add(item);
				}
			}
			foreach (XAttribute attrib in selectedTreeItem.XElement.Attributes())
			{
				if (attrib.Name.LocalName.ToString() == "Text")
				{
					string[] text1 = { attrib.Name.LocalName.ToString(), attrib.Value };
					item = new ListViewItem(text1);
					listView.Items.Add(item);
				}
			}
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			inputBox.Clear();
			selectedTreeItem = (CustomTreeNode)e.Node;
			RefreshText();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm AboutForm = new AboutForm();
			AboutForm.ShowDialog();
		}

		private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			selectedListViewItem = e;
			inputBox.Text = e.Item.SubItems[1].Text;
		}

		private void UndoBttn_Click(object sender, EventArgs e)
		{
			if (_controller == null)
				return;

			_controller.Undo();
		}

		private void RedoBttn_Click(object sender, EventArgs e)
		{
			if (_controller == null)
				return;

			_controller.Redo();
		}
	}
}
