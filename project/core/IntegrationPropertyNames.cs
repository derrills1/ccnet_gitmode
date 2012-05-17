using System;
using System.Collections.Generic;
using System.Text;

namespace ThoughtWorks.CruiseControl.Core
{
    /// <summary>
    /// Holds all the integration property names, 
    /// so no need to use the string themselves throughout the code
    /// </summary>
    public class IntegrationPropertyNames
    {
        /// <summary>
        /// The name of the CCNet project that is being integrated.
        /// </summary>
        public const string CCNetProject = "CCNetProject";        
        /// <summary>
        /// The URL where the project is located 
        /// </summary>
        public const string CCNetProjectUrl = "CCNetProjectUrl";
        /// <summary>
        /// The project working directory (as an absolute path) 
        /// </summary>
        public const string CCNetWorkingDirectory = "CCNetWorkingDirectory";
        /// <summary>
        /// The project artifact directory (as an absolute path) 
        /// </summary>
        public const string CCNetArtifactDirectory = "CCNetArtifactDirectory";
        /// <summary>
        /// The status of the current integration. Could be Success, Failure, Exception or Unknown 
        /// </summary>
        public const string CCNetIntegrationStatus = "CCNetIntegrationStatus";
        /// <summary>
        /// The label used to identify the CCNet build. This label is generated by the CCNet labeller. 
        /// </summary>
        public const string CCNetLabel = "CCNetLabel";
        /// <summary>
        /// The condition used to trigger the build, indicating if the build was triggered by new modifications or if it was forced. Legal values are: "IfModificationExists" or "ForceBuild"
        /// </summary>
        public const string CCNetBuildCondition = "CCNetBuildCondition";
        /// <summary>
        /// Contains the label as an integer if conversion is possible, otherwise zero. 
        /// </summary>
        public const string CCNetNumericLabel = "CCNetNumericLabel";
        /// <summary>
        /// The date of the build (in yyyy-MM-dd format) 
        /// </summary>
        public const string CCNetBuildDate = "CCNetBuildDate";
        /// <summary>
        /// The time of the start of the build (in HH:mm:ss format) 
        /// </summary>
        public const string CCNetBuildTime = "CCNetBuildTime";
        /// <summary>
        /// The status of the previous integration. Could be Success, Failure, Exception or Unknown 
        /// </summary>
        public const string CCNetLastIntegrationStatus = "CCNetLastIntegrationStatus";
        /// <summary>
        /// The file used by CCNet to read the progress of external tools
        /// <see href="http://confluence.public.thoughtworks.org/display/CCNETCOMM/Viewing+build+progress+with+Nant+and+MSBuild"/>
        /// </summary>
        public const string CCNetListenerFile = "CCNetListenerFile";
        /// <summary>
        /// The list of users who have contributed modifications to a sequence of builds that has failed. 
        /// </summary>
        public const string CCNetFailureUsers = "CCNetFailureUsers";
        /// <summary>
        /// The list of users who have contributed to the current build only 
        /// </summary>
        public const string CCNetModifyingUsers = "CCNetModifyingUsers";
        /// <summary>
        /// The source of the integration request; this will generally be the name of the trigger that raised the request. 
        /// </summary>
        public const string CCNetRequestSource = "CCNetRequestSource";
        /// <summary>
        /// The user who forced the build. If security is off, or the build is not forced, then this will not be set. 
        /// </summary>
        public const string CCNetUser = "CCNetUser";

    }
}
