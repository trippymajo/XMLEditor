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

namespace XMLEditor
{
	public partial class MainForm : Form
	{
		// Varibale:
		public string filePath = string.Empty;
		public ListBox listBox = new ListBox();

		public MainForm()
		{
			InitializeComponent();

			//Making ListBox:
			listBox.FormattingEnabled = true;
			listBox.Location = new System.Drawing.Point(12, 50);
			listBox.Name = "listBox";
			listBox.Size = new System.Drawing.Size(768, 381);
			listBox.TabIndex = 1;
			Controls.Add(listBox);

		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm AboutForm = new AboutForm();
			AboutForm.ShowDialog();
		}

		private void ReadXml(string filePath)
		{
			XmlDocument xmlDoc = new XmlDocument();
			// Loading the file
			xmlDoc.Load(filePath);
			XmlElement xmlRoot = xmlDoc.DocumentElement;
			if (xmlRoot != null)
			{
				//reading the elements
				foreach (XmlElement xmlNode in xmlRoot)
				{
					//Get text from Attribute Text=
					XmlNode attr = xmlNode.Attributes.GetNamedItem("Text");
					if (attr != null)
						listBox.Items.Add(attr.Value);
					//Get text from child nodes
					foreach (XmlNode childNode in xmlNode.ChildNodes)
					{
						attr = childNode.Attributes.GetNamedItem("Text");
						if (attr != null)
							listBox.Items.Add(attr.Value.ToString());
					}
					/*
					// обходим все дочерние узлы элемента user
					foreach (XmlNode childNode in xmlNode.ChildNodes)
					{

						// если узел - company
						if (childNode.Name == "company")
						{
							Console.WriteLine($"Company: {childnode.InnerText}");
						}
						// если узел age
						if (childNode.Name == "age")
						{
							Console.WriteLine($"Age: {childnode.InnerText}");
						}
					}
					*/

				}
				//Console.ReadKey();
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
				listBox.BeginUpdate();
				ReadXml(filePath);
				listBox.EndUpdate();
			}
		}
	}
}
