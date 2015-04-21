using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Globalization;
using System.Web;

using DJLibrary.Log;

namespace Piwik.AppTracker
{
    /// <summary>
    /// Reimplement PiwikTracker. Ref: https://github.com/piwik/piwik-dotnet-tracker/ 
    /// - 原本的設計無法extend, API的style也不是很乾淨
    /// - 缺少cid的欄位, 也沒有辦法override
    /// - 原本的設計有一些是depends on HttpContext (running inside web). 有些處理的邏輯不合適desktop app.
    /// 
    /// 新的設計方式follow Android SDK的形式, 請參考 https://github.com/piwik/piwik-sdk-android
    /// </summary>
    public class PiwikAppTracker
    {
        private static string API_VERSION = "1";
        private static string RECORD_VALUE = "1";
        private static int QUERYPARAM_CAPACITY = 15;
        private static int LENGTH_VISITOR_ID = 16;
        private static string USER_AGENT = "PiwikAppTracker";

        // These are defined as static variables (since it is bound to the local installation)
        //
        protected static String s_userAgent;
        protected static String s_userLanguage;

        /// <summary>
        /// The ID of the website we're tracking a visit/action for
        /// </summary>
        protected int m_siteId;

        /// <summary>
        /// Tracking HTTP API endpoint, for example, http://your-piwik-domain.tld/piwik.php
        /// </summary>
        protected string m_apiUrl;

        /// <summary>
        /// 定義application的domain. 系統所記錄的url的格式會是 http://appDomain/pageurl. 
        /// </summary>
        protected String m_appDomain;

        /// <summary>
        /// Defines the User ID for this request.
        /// User ID is any non empty unique string identifying the user (such as an email address or a username).
        /// To access this value, users must be logged-in in your system so you can
        /// fetch this user ID from your system, and pass it to Piwik.
        /// When specified, the User ID will be "enforced".
        /// This means that if there is no recent visit with this User ID, a new one will be created.
        /// If a visit is found in the last 30 minutes with your specified User ID,
        /// then the new action will be recorded to this existing visit.
        /// </summary>
        protected String m_userId;

        /// <summary>
        /// The unique visitor ID, must be a 16 characters hexadecimal string.
        /// Every unique visitor must be assigned a different ID and this ID must not change after it is assigned.
        /// If this value is not set Piwik will still track visits, but the unique visitors metric might be less accurate.
        /// </summary>
        protected String m_visitorId;

        /// <summary>
        /// Keep track of parameters. Key = QueryParam, Value = it's value
        /// </summary>
        protected Dictionary<string, string> m_querytParams = new Dictionary<string, string>(QUERYPARAM_CAPACITY);

        /// <summary>
        /// Keep track of custom variables. There are 2 entries: 
        /// 1. QueryParam.VISIT_SCOPE_CUSTOM_VARIABLES (session level),
        /// 2. QueryParam.SCREEN_SCOPE_CUSTOM_VARIABLES (page level)
        /// </summary>
        protected Dictionary<string, CustomVariables> m_cvarMap = new Dictionary<string,CustomVariables>(2);

        // random number generator
        //
        protected Random m_rnd = new Random((int)new DateTime().Ticks);

        #region Public API

        /// <summary>
        /// Construct an instance of PiwikAppTracker.
        /// Client should hold on to this object during the session.
        /// </summary>
        /// <param name="apiUrl">Piwik service url, e.g. http://your-piwik-domain.tld/ </param>
        /// <param name="siteId">Piwik site id.</param>
        /// <param name="appDomain">Application的domain name. 這個是用來組成完整的url. 如果傳入"xq"的話, 則記錄的url就會是 http://xq/page/.. </param>
        public PiwikAppTracker(string apiUrl, int siteId, string appDomain)
        {
            if (String.IsNullOrEmpty(apiUrl))
                throw new ArgumentNullException("apiUrl");

            if (siteId <= 0)
                throw new ArgumentNullException("siteId");

            if (String.IsNullOrEmpty(appDomain))
                throw new ArgumentNullException("appDomain");

            setAPIUrl(apiUrl);
            m_siteId = siteId;
            m_visitorId = getRandomVisitorId();
            m_appDomain = appDomain;

            setUserAgent(USER_AGENT); 
            setLanguage(CultureInfo.CurrentCulture.Name);
        }

        /// <summary>
        /// Generic method to set a parameter with string value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PiwikAppTracker set(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
                m_querytParams[key] = value;

            return this;
        }

        /// <summary>
        /// Generic method to set a parameter with int value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PiwikAppTracker set(string key, int value)
        {
            m_querytParams[key] = value.ToString();
            return this;
        }

        /// <summary>
        /// Generic method to query a parameter's value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Value for the specified parameter. null if not found.</returns>
        public string get(string key)
        {
            string value;
            if (!m_querytParams.TryGetValue(key, out value))
                return null;
            else
                return value;
        }

        /// <summary>
        /// Set user ID. This is called after application has login.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public PiwikAppTracker setUserId(string uid)
        {
            if (String.IsNullOrEmpty(uid))
                throw new ArgumentNullException("uid cannot be empty or null.");

            this.m_userId = uid;

            // TODO: 如果有了userid, 則visitorId是否也該設成一樣的, 來確保這是同一個session ?
            // (還是piwik自己會判斷 ? for example 有了userid之後就ignore visitorId ?)
            //
            var encodedGuidBytes = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.Default.GetBytes(m_userId));
            m_visitorId = BitConverter.ToString(encodedGuidBytes).Replace("-", "").Substring(0, LENGTH_VISITOR_ID).ToLower();

            return this;
        }

        public string getUserId()
        {
            return m_userId;
        }


        /// <summary>
        /// Set visitor ID. This is usually not necessary.
        /// </summary>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        public PiwikAppTracker setVisitorId(string visitorId)
        {
            if (!isValidVisitorId(visitorId))
                throw new ArgumentException("visitorId的格式不正確.");

            m_visitorId = visitorId;
            return this;
        }

        public string getVisitorId()
        {
            return m_visitorId;
        }

        /// <summary>
        /// Set screen resolution.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public PiwikAppTracker setResolution(int width, int height)
        {
            var value = string.Format("{0}x{1}", width, height);
            return set(QueryParams.SCREEN_RESOLUTION, value);
        }

        /// <summary>
        /// Set user's custom variables. 
        /// 這個是'global scope', 用來定義擴充欄位. 最多只能有5個 (index=0..4).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PiwikAppTracker setUserCustomVariable(int index, string name, string value)
        {
            return setCustomVariable(QueryParams.VISIT_SCOPE_CUSTOM_VARIABLES, index, name, value);
        }

        /// <summary>
        /// Set screen (page)'s custom variables. 
        /// 這個是'page scope', 用來定義擴充欄位 for 每一頁. 最多只能有5個 (index=0..4).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public PiwikAppTracker setScreenCustomVariable(int index, string name, string value)
        {
            return setCustomVariable(QueryParams.SCREEN_SCOPE_CUSTOM_VARIABLES, index, name, value);
        }

        /// <summary>
        /// Set screen的名稱. 這個會對應到piwik的action_name欄位.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public PiwikAppTracker setScreenTitle(string title)
        {
            return set(QueryParams.ACTION_NAME, title);
        }

        /// <summary>
        /// Set userAgent的名稱. 這個只需要call一次就可以了(static)
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public PiwikAppTracker setUserAgent(string userAgent)
        {
            PiwikAppTracker.s_userAgent = userAgent;
            HttpRequester.UserAgent = userAgent;
            return this;
        }

        public string getUserAgent()
        {
            return PiwikAppTracker.s_userAgent;
        }

        public PiwikAppTracker setLanguage(string lang)
        {
            PiwikAppTracker.s_userLanguage = lang;
            HttpRequester.AcceptLanguage = lang; 
            return this;
        }

        public string getLanguage()
        {
            return PiwikAppTracker.s_userLanguage;
        }

        /// <summary>
        /// 記錄一個screen view. 
        /// </summary>
        /// <param name="path">Url for the screen. 格式是 "/abc/def" </param>
        /// <returns></returns>
        public PiwikAppTracker trackScreenView(string path)
        {
            set(QueryParams.URL_PATH, path);
            return doTrack();
        }

        /// <summary>
        /// 記錄一個screen view. 同時設定這個screen的title.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public PiwikAppTracker trackScreenView(string path, string title)
        {
            setScreenTitle(title);
            set(QueryParams.URL_PATH, path);
            return doTrack();
        }

        /// <summary>
        /// Track event. 針對目前的頁面, 傳送需要另外追蹤的資訊. 通常可以用來記錄頁面的動作.
        /// 呼叫這個API後會送出一個記錄.
        /// </summary>
        /// <param name="category">event的分類. 例如"UIAction", or "Verb"(使用者的這個event所trigger的動作)</param>
        /// <param name="action">event的動作, 例如"Click", "DoubleClick", "Order"</param>
        /// <param name="name">這個event所作用的物件名稱, 例如 "Button1", "Button2"</param>
        /// <param name="value">用來記錄這個event的數值</param>
        /// <returns></returns>
        public PiwikAppTracker trackEvent(string category, string action, string name, int? value)
        {
            if (String.IsNullOrEmpty(category))
                throw new ArgumentNullException("category");

            if (String.IsNullOrEmpty(action))
                throw new ArgumentNullException("action");

            set(QueryParams.EVENT_CATEGORY, category);
            set(QueryParams.EVENT_ACTION, action);
            if (!String.IsNullOrEmpty(name))
                set(QueryParams.EVENT_NAME, name);

            if (value.HasValue)
                set(QueryParams.EVENT_VALUE, value.Value);

            return doTrack();
        }

        public PiwikAppTracker trackEvent(string category, string action, string name)
        {
            return trackEvent(category, action, name, null);
        }

        public PiwikAppTracker trackEvent(string category, string action)
        {
            return trackEvent(category, action, null, null);
        }

        # endregion 

        /// <summary>
        /// Return url for this page view. If url is set, will return a full url.
        /// Note: for event tracking, getUrl() will return null. 這樣子的話每個event就不會產生一個page view,
        /// 而是直接把資料記錄到event端.
        /// </summary>
        /// <returns></returns>
        protected string getUrl()
        {
            var value = get(QueryParams.URL_PATH);
            if (string.IsNullOrEmpty(value))
                return null;

            if (value.StartsWith("http://") || value.StartsWith("https://"))
                return value;
            else if (!value.StartsWith("/"))
                value = "/" + value;
            
            return getApplicationBaseURL() + value;
        }

        /// <summary>
        /// Core function to send a request to Piwik server.
        /// TODO: 目前for demo purpose在這裡會直接call server. 
        /// </summary>
        /// <returns></returns>
        protected PiwikAppTracker doTrack()
        {
            // Fill in basic information
            beforeTracking();

            // Compose url query
            //
            var requestParams = getParams();
            var url = m_apiUrl + requestParams;

            Logger.LogInfo("Request=%s", url);

            // Send request
            //
            try
            {
                byte[] responseData = HttpRequester.request(url);
                string response = Encoding.GetEncoding("UTF-8").GetString(responseData);

                Logger.LogInfo("Response=%s", response);
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }

            // Clean up 
            //
            afterTracking();

            return this;
        }

        protected void beforeTracking()
        {
            set(QueryParams.API_VERSION, API_VERSION);
            set(QueryParams.SEND_IMAGE, "0");
            set(QueryParams.SITE_ID, m_siteId);
            set(QueryParams.RECORD, RECORD_VALUE);
            set(QueryParams.RANDOM_NUMBER, m_rnd.Next(100000));

            var url = getUrl();
            if (!String.IsNullOrEmpty(url))
                set(QueryParams.URL_PATH, url);

            set(QueryParams.USER_AGENT, getUserAgent());
            set(QueryParams.LANGUAGE, getLanguage());

            set(QueryParams.VISITOR_ID, m_visitorId);
            if (!string.IsNullOrEmpty(m_userId))
                set(QueryParams.USER_ID, m_userId);

            set(QueryParams.DATETIME_OF_REQUEST, getCurrentDatetime());

            var cv = getCustomVariables(QueryParams.SCREEN_SCOPE_CUSTOM_VARIABLES);
            if (cv != null)
                set(QueryParams.SCREEN_SCOPE_CUSTOM_VARIABLES, cv.ToString());

            cv = getCustomVariables(QueryParams.VISIT_SCOPE_CUSTOM_VARIABLES);
            if (cv != null)        
                set(QueryParams.VISIT_SCOPE_CUSTOM_VARIABLES, cv.ToString());
        }

        protected void afterTracking()
        {
            clearQueryParams();
            clearAllCustomVariables();
        }

        private string getParams()
        {
            StringBuilder sb = new StringBuilder(200);
            sb.Append('?');
            foreach(KeyValuePair<string, string> entry in m_querytParams)
            {
                sb.Append(urlEncodedUTF8(entry.Key));
                sb.Append("=");
                sb.Append(urlEncodedUTF8(entry.Value));
                sb.Append("&");
            }
            return sb.ToString().Substring(0, sb.Length - 1);
        }

        private string urlEncodedUTF8(string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        private void setAPIUrl(string apiUrl)
        {
            // url 是否是piwik.php
            //
            if (apiUrl.EndsWith("piwik.php") || apiUrl.EndsWith("piwik-proxy.php"))
                m_apiUrl = apiUrl;
            else 
            {
                if (!apiUrl.EndsWith("/"))
                    apiUrl = apiUrl + "/";

                m_apiUrl = apiUrl + "piwik.php";        
            }
        }

        // Return a random generated visitor Id
        //
        private string getRandomVisitorId()
        {
            var encodedGuidBytes = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.Default.GetBytes(Guid.NewGuid().ToString()));
            return BitConverter.ToString(encodedGuidBytes).Replace("-", "").Substring(0, LENGTH_VISITOR_ID).ToLower();
        }

        // Verify is visitorId has the required format:
        // - must be exactly 16 chars
        // - '0'-'9' or 'a'-'f'
        //
        private bool isValidVisitorId(string visitorId)
        {
            if (String.IsNullOrEmpty(visitorId))
                return false;

            if (visitorId.Length != 16)
                return false;

            for (var i = 0; i < 16; i++)
            {
                var ch = visitorId[i];
                if (!('0' <= ch && ch <= '9' || 'a' <= ch && ch <= 'f'))
                    return false;
            }                    
            return true;
        }

        private PiwikAppTracker setCustomVariable(string ns, int index, string name, string value)
        {
            if (ns != QueryParams.VISIT_SCOPE_CUSTOM_VARIABLES &&
                ns != QueryParams.SCREEN_SCOPE_CUSTOM_VARIABLES)
                return this;

            CustomVariables cv = getCustomVariables(ns, true);
            if (cv != null)
                cv.set(index, name, value);

            return this;
        }

        private CustomVariables getCustomVariables(string ns, bool autoCreate = false)
        {
            CustomVariables cv;
            if (!m_cvarMap.TryGetValue(ns, out cv))
            {
                if (!autoCreate)
                    return null;

                cv = new CustomVariables();
                m_cvarMap[ns] = cv;
            }
            return cv;
        }

        private void clearAllCustomVariables()
        {
            var cv = getCustomVariables(QueryParams.SCREEN_SCOPE_CUSTOM_VARIABLES);
            if (cv != null)
                cv.clear();

            cv = getCustomVariables(QueryParams.VISIT_SCOPE_CUSTOM_VARIABLES);
            if (cv != null)
                cv.clear();
        }

        private void clearQueryParams()
        {
            m_querytParams.Clear();
        }

        private string getCurrentDatetime()
        {
            DateTime now = DateTime.Now;
            return now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string getApplicationBaseURL()
        {
            return string.Format("http://{0}", m_appDomain);
        }
    }
}
