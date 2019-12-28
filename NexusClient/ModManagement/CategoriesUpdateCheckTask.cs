﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nexus.Client.BackgroundTasks;
using Nexus.Client.ModRepositories;
using Nexus.Client.Mods;
using Nexus.Client.UI;

namespace Nexus.Client.ModManagement
{
	public class CategoriesUpdateCheckTask : ThreadedBackgroundTask
	{
		private string CurrentGameModeModDirectory = string.Empty;

		#region Properties

		/// <summary>
		/// Gets the AutoUpdater.
		/// </summary>
		/// <value>The AutoUpdater.</value>
		protected AutoUpdater AutoUpdater { get; private set; }

		/// <summary>
		/// Gets the current mod repository.
		/// </summary>
		/// <value>The current mod repository.</value>
		protected IModRepository ModRepository { get; private set; }

		/// <summary>
		/// Gets the current profile manager.
		/// </summary>
		/// <value>The current profile manager.</value>
		protected IProfileManager ProfileManager { get; private set; }


		/// <summary>
		/// Gets the current profile manager.
		/// </summary>
		/// <value>The current profile manager.</value>
		protected CategoryManager CategoryManager { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with its dependencies.
		/// </summary>
		/// <param name="p_AutoUpdater">The AutoUpdater.</param>
		/// <param name="p_ModRepository">The current mod repository.</param>
		/// <param name="p_lstModList">The list of mods we need to update.</param>
		/// <param name="p_booOverrideCategorySetup">Whether to force a global update.</param>
		public CategoriesUpdateCheckTask(CategoryManager p_cmCategoryManager, IProfileManager p_prmProfileManager, IModRepository p_ModRepository, string p_strCurrentGameModeModDirectory)
		{
			ModRepository = p_ModRepository;
			ProfileManager = p_prmProfileManager;
			CategoryManager = p_cmCategoryManager;
			CurrentGameModeModDirectory = p_strCurrentGameModeModDirectory;
		}

		#endregion

		#region Event Raising

		/// <summary>
		/// Raises the <see cref="IBackgroundTask.TaskEnded"/> event.
		/// </summary>
		/// <param name="e">A <see cref="TaskEndedEventArgs"/> describing the event arguments.</param>
		protected override void OnTaskEnded(TaskEndedEventArgs e)
		{
			base.OnTaskEnded(e);
		}
		#endregion

		/// <summary>
		/// Starts the update.
		/// </summary>
		/// <param name="p_camConfirm">The delegate to call to confirm an action.</param>
		public void Update(ConfirmActionMethod p_camConfirm)
		{
			Start(p_camConfirm);
		}
	    
	    /// <summary>
		/// The method that is called to start the backgound task.
		/// </summary>
		/// <param name="args">Arguments to for the task execution.</param>
		/// <returns>Always <c>null</c>.</returns>
		protected override object DoWork(object[] args)
		{
			var ModList = new List<string>();
			var ModCheck = new List<IMod>();
			var camConfirm = (ConfirmActionMethod)args[0];

			var ModInstallDirectory = CurrentGameModeModDirectory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; 

			OverallMessage = "Updating categories info: setup search..";
			OverallProgress = 0;
			OverallProgressStepSize = 1;
			ShowItemProgress = false;
			OverallProgressMaximum = 2;

			OverallMessage = "Retrieving the categories list... 1/2";
			StepOverallProgress();
			try
			{

				var lstCategories = ModRepository.GetCategories(ModRepository.GameDomainName);

				var i = 1;

                if (lstCategories.Count > 0)
				{
					foreach(var category in lstCategories)
					{
						OverallMessage = "Saving the categories list... " + i + "/" + lstCategories.Count();
						StepOverallProgress();

						var modCategory = CategoryManager.FindCategory(category.Id);
						if (modCategory != null && modCategory.Id != 0)
                        {
                            CategoryManager.RenameCategory(modCategory.Id, category.Name);
                        }
                        else
                        {
                            CategoryManager.AddCategory(new ModCategory(category.Id, category.Name, category.Name));
                        }
                    }
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

			return null;
		}
	}
}
