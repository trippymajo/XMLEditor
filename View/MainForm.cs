using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using XMLEditor.Controller;
using XMLEditor.ViewModel;

namespace XMLEditor.View
{
	public partial class MainForm : Form
	{
		private readonly IXmlController _controller;
		private CustomTreeNode selectedTreeItem = null;
		private ListViewItemSelectionChangedEventArgs selectedListViewItem = null;

		private static readonly HashSet<string> IgnoredElements = new HashSet<string>
		{
			"RibbonSeparator", "RibbonTabSelector", "RibbonPanelSourceReference",
			"DialogBoxLauncher", "RibbonPanelBreak", "RibbonTabSelectors"
		};

		public MainForm(IXmlController controller)
		{
			_controller = controller ?? throw new ArgumentNullException(nameof(controller));
			InitializeComponent();
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

		private void LoadXmlTreeView(XElement elemRoot)
		{
			if (elemRoot == null)
				return;

			selectedTreeItem = null;
			selectedListViewItem = null;
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
				if (_controller == null)
					throw new ArgumentNullException(nameof(_controller));

				var root = _controller.OpenXmlDoc(openFileDialog.FileName);
				LoadXmlTreeView(root);
			}
		}

		private void EditBtn_Click(object sender, EventArgs e)
		{
			if (_controller == null)
				throw new ArgumentNullException(nameof(_controller));

			if (selectedListViewItem == null)
			{
				inputBox.Clear();
				return;
			}

			if (selectedListViewItem.Item.SubItems[0].Text.ToLower().Equals("<text>"))
			{
				_controller.UpdateTextVal(selectedTreeItem.XElement, inputBox.Text);
				RefreshText();
			}
			else
			{
				XName namespaceNameAttr = null;
				XNamespace XNs = selectedTreeItem.XElement.Name.Namespace;

				if (string.IsNullOrEmpty(XNs.NamespaceName))
					namespaceNameAttr = XNs + selectedListViewItem.Item.SubItems[0].Text;
				else
					namespaceNameAttr = selectedListViewItem.Item.SubItems[0].Text;

				_controller.UpdateTextAttr(selectedTreeItem.XElement, namespaceNameAttr, inputBox.Text);
				RefreshText();
			}
		}

		private void RefreshText()
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

		/// <summary>
		/// DFS search of the Node in treeView by provided path array
		/// </summary>
		/// <param name="nodes">Node to search in</param>
		/// <param name="pathArray">Path to the node to find</param>
		/// <param name="level">Current level of the Tree</param>
		/// <returns>TreeNode found by path</returns>
		/// <return>[null] - TreeNode was not found</return>
		private TreeNode FindNode(TreeNodeCollection nodes, string[] pathArray, int level)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Text == pathArray[level])
				{
					if (level == pathArray.Length - 1)
						return node;
					else
						return FindNode(node.Nodes, pathArray, ++level);
				}
			}
			return null;
		}

		private void RefreshTree(XElement elemRoot)
		{
			string nodeFullPath = null;

			if (selectedTreeItem != null)
				nodeFullPath = selectedTreeItem.FullPath;

			LoadXmlTreeView(elemRoot);

			// Restore selection
			if (nodeFullPath != null)
			{
				string[] pathArray = nodeFullPath.Split(treeView.PathSeparator.ToCharArray());
				CustomTreeNode nodeToSelect = (CustomTreeNode)FindNode(treeView.Nodes, pathArray, 0);

				if (nodeToSelect != null)
				{
					treeView.SelectedNode = nodeToSelect;
					selectedTreeItem = (CustomTreeNode)treeView.SelectedNode;
					nodeToSelect.Expand();
					RefreshText();
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
				throw new ArgumentNullException(nameof(_controller));

			var root = _controller.Undo();

			if (root != null)
				RefreshTree(root);
		}

		private void RedoBttn_Click(object sender, EventArgs e)
		{
			if (_controller == null)
				throw new ArgumentNullException(nameof(_controller));

			var root = _controller.Redo();

			if (root != null)
				RefreshTree(root);
		}
	}
}
