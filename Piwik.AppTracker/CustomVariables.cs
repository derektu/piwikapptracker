using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Piwik.AppTracker
{
    /**
     * You can track up to 5 custom variables for each user to your app,
     * and up to 5 custom variables for each screen view.
     * <p/>
     * Desired json output:
     * {
     * "1":["OS","iphone 5.0"],
     * "2":["Piwik Mobile Version","1.6.2"],
     * "3":["Locale","en::en"],
     * "4":["Num Accounts","2"],
     * "5":["Level","over9k"]
     * }
     */


    /// <summary>
    /// Helper class to hold piwik custom variables.
    /// </summary>
    public class CustomVariables
    {
        private static int MAX_VARS = 5;
        protected Dictionary<String, String[]> m_vars = new Dictionary<string, string[]>(MAX_VARS);

        /// <summary>
        /// Set custom variables.
        /// </summary>
        /// <param name="index">zero-based index. Must be 0..4</param>
        /// <param name="key">custom variable name, e.g. "OS"</param>
        /// <param name="value">custom variable value, e.g. "Win7"</param>
        public void set(int index, string key, string value)
        {
            if (index < 0 || index >= MAX_VARS)
                throw new IndexOutOfRangeException("Invalid index.");

            /*
             * Since the format of custom variables is a json object:
             * {
             * "1":["OS","iphone 5.0"],
             * "2":["Piwik Mobile Version","1.6.2"],
             * "3":["Locale","en::en"],
             * "4":["Num Accounts","2"],
             * "5":["Level","over9k"]
             * }
             * 
             * So we convert each key/value to an array of (json) string, and convert the numeric index as string.
             */ 
            m_vars[(index+1).ToString()] = new string[] { key, value };
        }

        /// <summary>
        /// Clear all values.
        /// </summary>
        public void clear()
        {
            m_vars.Clear();
        }

        /// <summary>
        /// Convert to JSON string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(m_vars);
        }
    }
}
