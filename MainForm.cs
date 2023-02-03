﻿using System;
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
		ListViewItemSelectionChangedEventArgs selectedViewItem = null;
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

		private bool isIgnored(string xElementName)
		{
			switch (xElementName)
			{
				case "RibbonSeparator": return true;
				case "RibbonTabSelector": return true;
				case "RibbonPanelSourceReference": return true;
				case "DialogBoxLauncher": return true;
				case "RibbonPanelBreak": return true;
				case "RibbonTabSelectors": return true;
				default: return false;
			}
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
			if (item.SubItems[0].Text.ToLower().Equals("<text>"))
			{
				selectedTreeItem.xElement.SetValue(inputBox.Text);
			}
			else
			{
				node.xElement.SetAttributeValue(XNs + item.SubItems[0].Text, inputBox.Text);
			}
			root.xElement.Save(filePath);
			getTextFromXml(node); ;
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
	}
}
