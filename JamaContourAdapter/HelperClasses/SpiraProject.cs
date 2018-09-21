using System;
using System.Collections.Generic;
using System.Text;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter
{
	internal class SpiraProject
	{
		//Static shared values.
		public const char CHAR_FIELD = '\xE';
		public const char CHAR_RECORD = '\xF';
		public const string URL_APIADD = "/Services/v5_0/SoapService.svc";

		//Class items.
		public Uri ServerURL;
		public string UserName;
		public int ProjectNum;
		public string ProjectName;
		public string UserPass;
		public int RootReq;

		//ToString overrider.
		public override string ToString()
		{
			return this.ProjectName;
		}

        #region Enumerations

        /// <summary>
        /// Importances / Priorities
        /// </summary>
        public enum Importance
        {
            Critical = 1,
            High = 2,
            Medium = 3,
            Low = 4
        }

        /// <summary>
        /// Statuses
        /// </summary>
        public enum Status
        {
            Requested = 1,
            Planned = 2,
            InProgress = 3,
            Completed = 4,
            Accepted = 5,
            Rejected = 6,
            Evaluated = 7,
            Obsolete = 8
        }

        public enum RequirementType
        {
            Package = -1,
            Need = 1,
            Feature = 2,
            UseCase = 3,
            UserStory = 4,
            Quality = 5,
            DesignElement = 6
        }

        /// <summary>
        /// Statuses
        /// </summary>
        public enum ReleaseStatusEnum
        {
            Planned = 1,
            InProgress = 2,
            Completed = 3,
            Closed = 4,
            Deferred = 5,
            Cancelled = 6
        }

        /// <summary>
        /// Release Types
        /// </summary>
        public enum ReleaseTypeEnum
        {
            MajorRelease = 1,
            MinorRelease = 2,
            Iteration = 3,
            Phase = 4
        }


        #endregion

        #region Static Members
        /// <summary>Converts an encoded string into a SpiraProject. Uses \xE for field seperation.</summary>
		/// <param name="inString">The string to decode.</param>
		/// <returns>A SpiraProject based on the given string, or null if error.</returns>
		static internal SpiraProject GenerateFromString(string inString)
		{
			//Get the values.
			string[] values = inString.Split(SpiraProject.CHAR_FIELD);

			try
			{
				SpiraProject retProject = new SpiraProject();

				retProject.UserName = values[0];
				retProject.UserPass = values[1];
				retProject.ServerURL = new Uri(values[2]);
				retProject.ProjectNum = int.Parse(values[3]);
				retProject.ProjectName = values[4];

				return retProject;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		/// <summary>Returns a savable string from a project object.</summary>
		/// <param name="inProject">The SpiraProject to convert.</param>
		/// <returns>A string of the fields seperated by the field seperator.</returns>
		static internal string GenerateToString(SpiraProject inProject)
		{
			if (inProject != null)
			{
                return inProject.UserName + SpiraProject.CHAR_FIELD +
                    inProject.UserPass + SpiraProject.CHAR_FIELD +
                    inProject.ServerURL.AbsoluteUri + SpiraProject.CHAR_FIELD +
                    inProject.ProjectNum.ToString() + SpiraProject.CHAR_FIELD +
                    inProject.ProjectName + SpiraProject.CHAR_FIELD;
			}
			else
			{
				return null;
			}
		}

		/// <summary>Compares to SpiraProjects and returns whether they are equal or not.</summary>
		/// <param name="inProject1">One of the SpiraProjects to compare.</param>
		/// <param name="inProject2">Another SpiraProject to compare.</param>
		/// <returns>True if the settings are the same, false if not.</returns>
		public bool IsEqual(SpiraProject inProject1, SpiraProject inProject2)
		{
			if (inProject2.ServerURL.AbsoluteUri.Trim() == inProject1.ServerURL.AbsoluteUri.Trim() &&
				inProject2.UserName == inProject1.UserName &&
				inProject2.ProjectNum == inProject1.ProjectNum)
				return true;
			else
				return false;
		}

		#endregion

	}
}
