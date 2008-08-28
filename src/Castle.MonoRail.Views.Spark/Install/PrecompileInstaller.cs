﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Spark;


namespace Castle.MonoRail.Views.Spark.Install
{
    public partial class PrecompileInstaller : Installer
    {
        public PrecompileInstaller()
        {
            InitializeComponent();
        }

        public string TargetAssemblyFile { get; set; }
        public string ViewPath { get; set; }

        public event DescribeBatchEventHandler DescribeBatch;

        public override void Install(IDictionary stateSaver)
        {
            // figure out all paths based on this assembly in the bin dir

            var assemblyPath = Parent.GetType().Assembly.CodeBase.Replace("file:///", "");
            var targetPath = Path.ChangeExtension(assemblyPath, ".Views.dll");

            var appBinPath = Path.GetDirectoryName(assemblyPath);
            var appBasePath = Path.GetDirectoryName(appBinPath);

            var viewPath = ViewPath;
            if (string.IsNullOrEmpty(viewPath))
                viewPath = "Views";

            if (!Directory.Exists(Path.Combine(appBasePath, viewPath)) &&
                Directory.Exists(Path.Combine(appBinPath, viewPath)))
            {
                appBasePath = appBinPath;
            }

            var webFileHack = Path.Combine(appBasePath, "web");
            var viewsLocation = Path.Combine(appBasePath, viewPath);

            if (!string.IsNullOrEmpty(TargetAssemblyFile))
                targetPath = Path.Combine(appBinPath, TargetAssemblyFile);

            // this hack enables you to open the web.config as if it was an .exe.config
            File.Create(webFileHack).Close();
            var config = ConfigurationManager.OpenExeConfiguration(webFileHack);
            File.Delete(webFileHack);

            // GetSection will try to resolve the "Spark" assembly, which the installutil appdomain needs help finding
            AppDomain.CurrentDomain.AssemblyResolve += ((sender, e) => Assembly.LoadFile(Path.Combine(appBinPath, e.Name + ".dll")));
            var settings = (ISparkSettings)config.GetSection("spark");

            var services = new StubMonoRailServices();
            services.AddService(typeof(IViewSourceLoader), new FileAssemblyViewSourceLoader(viewsLocation));
            services.AddService(typeof(ISparkViewEngine), new SparkViewEngine(settings));
            services.AddService(typeof(IControllerDescriptorProvider), services.ControllerDescriptorProvider);

            var factory = new SparkViewFactory();
            factory.Service(services);

            // And generate all of the known view/master templates into the target assembly

            var batch = new SparkBatchDescriptor(targetPath);
            if (DescribeBatch != null)
                DescribeBatch(this, new DescribeBatchEventArgs { Batch = batch });

            factory.Precompile(batch);

            base.Install(stateSaver);
        }
    }
}
