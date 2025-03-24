using System;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using XMLEditor.Model;
using XMLEditor.Controller;
using XMLEditor.View;

namespace XMLEditor
{
	static class XMLEditorMain
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var services = new ServiceCollection();
            services.AddSingleton<IXmlModel, XmlModel>();
            services.AddSingleton<IXmlController, XmlController>();
            services.AddTransient<MainForm>();

            var serviceProvider = services.BuildServiceProvider();

			Application.Run(serviceProvider.GetRequiredService<MainForm>());
		}

	}
}
