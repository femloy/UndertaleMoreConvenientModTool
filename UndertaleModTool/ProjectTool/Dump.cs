﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UndertaleModTool.ProjectTool.Resources;
using UndertaleModTool.ProjectTool.Resources.GMOptions;
using UndertaleModTool.ProjectTool.Resources.Options;

namespace UndertaleModTool.ProjectTool
{
    public partial class Dump
    {
		// Hey burnedpopcorn or other Pizza Tower goons stop stealing my shit thanks bro

		public string BasePath;
		private DumpOptions _options;
        TextureWorker _worker = new();
		private GMProject _project = new();

		public Dump(MainWindow theWindowInQuestion)
		{
			MainWindow = theWindowInQuestion;
			_options = new();
		}

		public async Task DumpAsset<A, B>(string name, IList<B> list) where A : ISaveable where B : UndertaleNamedResource
		{
			SetupProgress(name, list.Count);

			await Task.Run(() => Parallel.ForEach(list, (source) =>
			{
				UpdateStatus(source.Name.Content);

				var asset = (A)Activator.CreateInstance(typeof(A), new object[] { source });
				asset.Save();

				IncrementProgress();
			}));
		}

        public async Task Start()
        {
			Files.Init();
			Constants.Init();
			TpageAlign.Clear();

			if (Options.asset_texturegroups) // GMProject relies on this so it's placed earlier
				TpageAlign.Init();

			#region Dump Resources

			if (Options.asset_project)
				_project = new GMProject(Data);

			if (Options.asset_options)
			{
				new GMMainOptions(Data).Save();
				new GMWindowsOptions(Data).Save();

				if (Options.options_other_platforms)
				{
					// TODO
				}
			}

			if (Options.asset_sounds)
				await DumpAsset<GMSound, UndertaleSound>("Sounds", Data.Sounds);
			if (Options.asset_sprites)
				await DumpAsset<GMSprite, UndertaleSprite>("Sprites", Data.Sprites);

			#endregion

			if (TpageAlign.ConsoleGroup)
				_project.TextureGroups.Add(new GMTextureGroup() { name = TpageAlign.CONSOLE_GROUP_NAME, targets = GMTarget.None });

			_project.AddIncludedFiles();
			Files.Save();

			_project.Save();
		}

        public void Dispose()
        {
            _worker.Dispose();
            _worker = null;

            GC.SuppressFinalize(this);
        }

		public static void AddFolder(string nameAndPath)
		{
			var project = Current?._project;
			if (project == null) return;
			project.Folders.Add(new GMFolder(nameAndPath) { order = project.Folders.Count });
		}

		public static MainWindow MainWindow;
        public static Dump Current;
        public static TextureWorker TexWorker => Current?._worker;
		public static DumpOptions Options => Current?._options;
    }
}
