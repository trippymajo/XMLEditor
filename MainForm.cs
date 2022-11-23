using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace XMLEditor
{
	public partial class MainForm : Form
	{
		// Varibales:
		string filePath = string.Empty;
		CustomTreeNode root = new CustomTreeNode();
		CustomTreeNode selectedTreeItem = null;

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

		private void makeXmlTree(string filePath)
		{
			XElement xmlDoc;
			xmlDoc = XElement.Load(filePath);
			treeView.Nodes.Clear();
			root = new CustomTreeNode(xmlDoc.Name.LocalName.ToString(), xmlDoc);
			getEveryNode(root, xmlDoc);
			treeView.Nodes.Add(root);
			root.Expand();

		}

		private void getEveryNode(CustomTreeNode root, XElement element)
		{
			CustomTreeNode node;
			foreach (XElement item in element.Elements())
			{
				//                ListItems.Items.Add(item.Value, "TRUE".Equals(item.Attribute("checked").Value.ToUpper()));
				node = new CustomTreeNode(item.Name.LocalName.ToString(), item);
				root.Nodes.Add(node);
				getEveryNode(node, item);

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
		}

		private void getTextFromXml(TreeViewEventArgs e)
		{
			listView.Items.Clear();
			CustomTreeNode selected = (CustomTreeNode)e.Node;
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
			selectedTreeItem = (CustomTreeNode)e.Node;
			getTextFromXml(e);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm AboutForm = new AboutForm();
			AboutForm.ShowDialog();
		}
	}
}
