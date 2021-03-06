namespace ThoughtWorks.CruiseControl.Core.Tasks
{
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using ThoughtWorks.CruiseControl.Core.Config;
    using ThoughtWorks.CruiseControl.Core.Util;
    using System;

    /// <summary>
    /// Base task to execute external functionality.
    /// </summary>
    public abstract class BaseExecutableTask
        : TaskBase, IConfigurationValidation
	{
		protected ProcessExecutor executor;
		protected BuildProgressInformation buildProgressInformation;

		protected abstract string GetProcessFilename();
		protected abstract string GetProcessArguments(IIntegrationResult result);
		protected abstract string GetProcessBaseDirectory(IIntegrationResult result);
        protected abstract ProcessPriorityClass GetProcessPriorityClass();
		protected abstract int GetProcessTimeout();

		protected virtual int[] GetProcessSuccessCodes()
		{
			return null;
		}

		protected virtual ProcessInfo CreateProcessInfo(IIntegrationResult result)
		{
			ProcessInfo info = new ProcessInfo(GetProcessFilename(), GetProcessArguments(result), GetProcessBaseDirectory(result),GetProcessPriorityClass(), GetProcessSuccessCodes());
			info.TimeOut = GetProcessTimeout();

			IDictionary properties = result.IntegrationProperties;
			foreach (string key in properties.Keys)
			{
				info.EnvironmentVariables[key] = StringUtil.IntegrationPropertyToString(properties[key]);
			}

			return info;
		}

		protected ProcessResult TryToRun(ProcessInfo info, IIntegrationResult result)
		{
			buildProgressInformation = result.BuildProgressInformation;

			try
			{
				// enable Stdout monitoring
				executor.ProcessOutput += ProcessExecutor_ProcessOutput;

				return executor.Execute(info);
			}
			catch (IOException e)
			{
				throw new BuilderException(
                    this, 
                    string.Format("Unable to execute: {0} {1}\n{2}", info.FileName, info.PublicArguments, e), e);
			}
			finally
			{
				// remove Stdout monitoring
				executor.ProcessOutput -= ProcessExecutor_ProcessOutput;
			}
		}

		private void ProcessExecutor_ProcessOutput(object sender, ProcessOutputEventArgs e)
		{
			if (buildProgressInformation == null)
				return;

			// ignore error output in the progress information
			if (e.OutputType == ProcessOutputType.ErrorOutput)
				return;

			buildProgressInformation.AddTaskInformation(e.Data);
        }

        #region Public properties
        #region IOSystem
        /// <summary>
        /// Gets or sets the IO system to use.
        /// </summary>
        /// <value>The IO system.</value>
        public IFileSystem IOSystem { get; set; }
        #endregion

        #region IOSystemActual
        /// <summary>
        /// Gets the actual IO system to use.
        /// </summary>
        /// <value>The IO system.</value>
        public IFileSystem IOSystemActual
        {
            get
            {
                if (this.IOSystem == null)
                {
                    this.IOSystem = new SystemIoFileSystem();
                }

                return this.IOSystem;
            }
        }
        #endregion
        #endregion

        #region Public methods
        #region Validate()
        /// <summary>
        /// Checks the internal validation of the item.
        /// </summary>
        /// <param name="configuration">The entire configuration.</param>
        /// <param name="parent">The parent item for the item being validated.</param>
        /// <param name="errorProcesser">The error processer to use.</param>
        public void Validate(IConfiguration configuration, ConfigurationTrace parent, IConfigurationErrorProcesser errorProcesser)
        {
            // Get the name of the executable
            var canCheck = true;
            var fileName = this.GetProcessFilename();
            if (!Path.IsPathRooted(fileName))
            {
                var project = parent.GetAncestorValue<Project>();
                if (project != null)
                {
                    var result = ConfigurationValidationUtils.GenerateResultForProject(project);
                    var directory = this.GetProcessBaseDirectory(result);
                    fileName = Path.Combine(directory, fileName);
                }
                else
                {
                    // Can't generate the path, therefore can't check for the file
                    canCheck = false;
                }
            }

            // See if it exists - shortcut the process if there is an exact match (the remainder uses pattern matching to try and find the file)
            if (canCheck && !this.IOSystemActual.FileExists(fileName))
            {
                var fileExists = false;

                // This needs to be from the filename, just in case the path is a relative path and it has been joined to the base directory
                var directory = Path.GetDirectoryName(fileName);
                if (this.IOSystemActual.DirectoryExists(directory))
                {
                    // Get all the files and check each file
                    var files = this.IOSystemActual.GetFilesInDirectory(directory);
                    var executableName1 = Path.GetFileNameWithoutExtension(fileName);
                    var executableName2 = Path.GetFileName(fileName);
                    foreach (var file in files)
                    {
                        // Strip the extension so we can compare partial file names (since Windows does not need the .exe extension)
                        var fileToTest = Path.GetFileNameWithoutExtension(file);

                        // Need to perform two checks here as some Windows names have two multiple dots - therefore GetFileNameWithoutExtension will strip the last part, 
                        // whether or not it is an extension
                        if (string.Equals(fileToTest, executableName1, StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(fileToTest, executableName2, StringComparison.InvariantCultureIgnoreCase))
                        {
                            fileExists = true;
                            break;
                        }
                    }
                }

                if (!fileExists)
                {
                    errorProcesser.ProcessWarning("Unable to find executable '" + fileName + "'");
                }
            }
        }
        #endregion
        #endregion
    }
}
