using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses
{
    /// <summary>
    /// Represents a Jama Contour project
    /// </summary>
    internal class JamaProjectInfo
    {
        //Static shared values.
        public const char CHAR_FIELD = '\xE';
        public const char CHAR_RECORD = '\xF';
        public const string URL_APIADD = "/ws/v3/soap/ContourSoapService";

        //Class items.
        public string ServerURL;
        public string UserName;
        public int ProjectNum;
        public string ProjectName;
        public string UserPass;

        //ToString overrider.
        public override string ToString()
        {
            return this.ProjectName;
        }

        #region Enumerations

        public enum Priority
        {
            Unassigned = 281,
            Low = 300,
            Medium = 301,
            High = 302
        }

        public enum Status
        {
            SelectOne = 291,
            Draft = 292,
            Approved = 293,
            Completed = 294,
            Rejected = 295
        }


        public enum Type
        {
            Attachment = 22,
            Feature = 23,
            Requirement = 24,
            UseCase = 25,
            TestCase = 26,
            Bug = 27,
            ChangeRequest = 28,
            UserStory = 29,
            Component = 30,
            Set = 31,
            Folder = 32,
            Text = 33,
            Core = 34,
            TestPlan = 35,
            TestCycle = 36,
            TestRun = 37,
            Epic = 38,
            UsageScenario = 39,
            Standard = 40,
            Theme = 41
        }

        #endregion

        #region Static Members
        /// <summary>Converts an encoded string into a JamaProject. Uses \xE for field seperation.</summary>
        /// <param name="inString">The string to decode.</param>
        /// <returns>A JamaProject based on the given string, or null if error.</returns>
        static internal JamaProjectInfo GenerateFromString(StreamWriter streamWriter, string inString)
        {
            //Get the values.
            string[] values = inString.Split(JamaProjectInfo.CHAR_FIELD);

            try
            {
                JamaProjectInfo retProject = new JamaProjectInfo();

                retProject.UserName = values[0];
                retProject.UserPass = values[1];
                retProject.ServerURL = values[2];
                retProject.ProjectNum = int.Parse(values[3]);
                retProject.ProjectName = values[4];

                return retProject;
            }
            catch (Exception ex)
            {
                streamWriter.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>Returns a savable string from a project object.</summary>
        /// <param name="inProject">The JamaProject to convert.</param>
        /// <returns>A string of the fields seperated by the field seperator.</returns>
        static internal string GenerateToString(JamaProjectInfo inProject)
        {
            if (inProject != null)
            {
                return inProject.UserName + JamaProjectInfo.CHAR_FIELD +
                    inProject.UserPass + JamaProjectInfo.CHAR_FIELD +
                    inProject.ServerURL + JamaProjectInfo.CHAR_FIELD +
                    inProject.ProjectNum.ToString() + JamaProjectInfo.CHAR_FIELD +
                    inProject.ProjectName + JamaProjectInfo.CHAR_FIELD;
            }
            else
            {
                return null;
            }
        }

        /// <summary>Compares two JamaProjects and returns whether they are equal or not.</summary>
        /// <param name="inProject1">One of the JamaProjects to compare.</param>
        /// <param name="inProject2">Another JamaProject to compare.</param>
        /// <returns>True if the settings are the same, false if not.</returns>
        public bool IsEqual(JamaProjectInfo inProject1, JamaProjectInfo inProject2)
        {
            if (inProject2.ServerURL.Trim() == inProject1.ServerURL.Trim() &&
                inProject2.UserName == inProject1.UserName &&
                inProject2.ProjectNum == inProject1.ProjectNum)
                return true;
            else
                return false;
        }

        #endregion
    }
}
