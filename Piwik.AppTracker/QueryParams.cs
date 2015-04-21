using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Piwik.AppTracker
{
    /// <summary>
    /// Define piwik HTTP API內的欄位
    /// </summary>
    public static class QueryParams
    {
        //-------------------------------------------------------------
        // Required parameters
        //-------------------------------------------------------------

        /**
         * The ID of the website we're tracking a visit/action for.
         * (required)
         */
        public static string SITE_ID = "idsite";

        /**
         * Required for tracking, must be set to one, eg, &rec=1.
         * (required)
         */
        public static string RECORD = "rec";

        /**
         * The full URL for the current action.
         * (required)
         */
        public static string URL_PATH = "url";


        //-------------------------------------------------------------
        // Recommended parameters
        //-------------------------------------------------------------

        /**
         * The title of the action being tracked.
         * It is possible to use slashes / to set one or several categories for this action.
         * For example, Help / Feedback will create the Action Feedback in the category Help.
         * (recommended)
         */
        public static string ACTION_NAME = "action_name";

        /**
         * The unique visitor ID, must be a 16 characters hexadecimal string.
         * Every unique visitor must be assigned a different ID and this ID must not change after it is assigned.
         * If this value is not set Piwik will still track visits, but the unique visitors metric might be less accurate.
         * (recommended)
         */
        public static string VISITOR_ID = "_id";

        /**
         * Meant to hold a random value that is generated before each request.
         * Using it helps avoid the tracking request being cached by the browser or a proxy.
         * (recommended)
         */
        public static string RANDOM_NUMBER = "rand";

        /**
         * The parameter &apiv=1 defines the api version to use (currently always set to 1)
         * (recommended)
         */
        public static string API_VERSION = "apiv";


        // Optional User info

        /**
         * The full HTTP Referrer URL.
         * This value is used to determine how someone got to your website (ie, through a website, search engine or campaign).
         */
        public static string REFERRER = "urlref";

        /**
         * Visit scope <a href="http://piwik.org/docs/custom-variables/">custom variables</a>.
         * This is a JSON encoded string of the custom variable array.
         */
        public static string VISIT_SCOPE_CUSTOM_VARIABLES = "_cvar";

        /**
         * The current count of visits for this visitor.
         * To set this value correctly, it would be required to store the value for each visitor in your application (using sessions or persisting in a database).
         * Then you would manually increment the counts by one on each new visit or "session", depending on how you choose to define a visit.
         * This value is used to populate the report Visitors > Engagement > Visits by visit number.
         */
        public static string TOTAL_NUMBER_OF_VISITS = "_idvc";

        /**
         * The UNIX timestamp of this visitor's previous visit.
         * This parameter is used to populate the report Visitors > Engagement > Visits by days since last visit.
         */
        public static string PREVIOUS_VISIT_TIMESTAMP = "_viewts";

        /**
         * The UNIX timestamp of this visitor's first visit.
         * This could be set to the date where the user first started using your software/app, or when he/she created an account.
         * This parameter is used to populate the Goals > Days to Conversion report.
         */
        public static string FIRST_VISIT_TIMESTAMP = "_idts";

        /**
         * The Campaign name (see <a href="http://piwik.org/docs/tracking-campaigns/">Tracking Campaigns</a>).
         * Used to populate the Referrers > Campaigns report.
         * Note: this parameter will only be used for the first pageview of a visit.
         */
        public static string CAMPAIGN_NAME = "_rcn";

        /**
         * The Campaign Keyword (see <a href="http://piwik.org/docs/tracking-campaigns/">Tracking Campaigns</a>).
         * Used to populate the Referrers > Campaigns report (clicking on a campaign loads all keywords for this campaign).
         * Note: this parameter will only be used for the first pageview of a visit.
         */
        public static string CAMPAIGN_KEYWORD = "_rck";

        /**
         * The resolution of the device the visitor is using, eg 1280x1024.
         */
        public static string SCREEN_RESOLUTION = "res";

        /**
         * The current hour (local time).
         */
        public static string HOURS = "h";

        /**
         * The current minute (local time).
         */
        public static string MINUTES = "m";

        /**
         * The current second (local time).
         */
        public static string SECONDS = "s";

        /**
         * An override value for the User-Agent HTTP header field.
         * The user agent is used to detect the operating system and browser used.
         */
        public static string USER_AGENT = "ua";

        /**
         * An override value for the Accept-Language HTTP header field.
         * This value is used to detect the visitor's country if <a href="http://piwik.org/faq/troubleshooting/#faq_65">GeoIP</a> is not enabled.
         */
        public static string LANGUAGE = "lang";

        /**
         * Defines the User ID for this request.
         * User ID is any non empty unique string identifying the user (such as an email address or a username).
         * To access this value, users must be logged-in in your system so you can fetch this user ID from your system, and pass it to Piwik.
         * The User ID appears in the visitor log, the Visitor profile, and you can Segment reports for one or several User ID (userId segment).
         * When specified, the User ID will be "enforced". This means that if there is no recent visit with this User ID, a new one will be created.
         * If a visit is found in the last 30 minutes with your specified User ID, then the new action will be recorded to this existing visit.
         */
        public static string USER_ID = "uid";

        /**
         * If set to 1, will force a new visit to be created for this action.
         */
        public static string SESSION_START = "new_visit";

        // Optional Action info (measure Page view, Outlink, Download, Site search)

        /**
         * Page scope <a href="http://piwik.org/docs/custom-variables/">custom variables</a>.
         * This is a JSON encoded string of the custom variable array.
         */
        public static string SCREEN_SCOPE_CUSTOM_VARIABLES = "cvar";

        /**
         * An external URL the user has opened.
         * Used for tracking outlink clicks. We recommend to also set the url parameter to this same value.
         */
        public static string LINK = "link";

        /**
         * URL of a file the user has downloaded.
         * Used for tracking downloads. We recommend to also set the url parameter to this same value.
         */
        public static string DOWNLOAD = "download";

        /**
         * The Site Search keyword.
         * When specified, the request will not be tracked as a normal pageview but will instead be tracked as a <a href="http://piwik.org/docs/site-search/">Site Search</a> request.
         */
        public static string SEARCH_KEYWORD = "search";

        /**
         * When SEARCH_KEYWORD is specified, you can optionally specify a search category with this parameter.
         */
        public static string SEARCH_CATEGORY = "search_cat";

        /**
         * When SEARCH_KEYWORD is specified, we also recommend to set this to the number of search results.
         */
        public static string SEARCH_NUMBER_OF_HITS = "search_count";

        /**
         * If specified, the tracking request will trigger a conversion for the goal of the website being tracked with this ID.
         */
        public static string GOAL_ID = "idgoal";

        /**
         * A monetary value that was generated as revenue by this goal conversion.
         * Only used if {@link #GOAL_ID} is specified in the request.
         */
        public static string REVENUE = "revenue";

        /**
         * An override value for the country.
         * Should be set to the two letter country code of the visitor (lowercase), eg fr, de, us.
         */
        public static string COUNTRY = "country";

        /**
         * An override value for the visitor's latitude, eg 22.456.<p>
         */
        public static string LATITUDE = "lat";

        /**
         * An override value for the visitor's longitude, eg 22.456.<p>
         */
        public static string LONGITUDE = "long";

        /**
         * Override for the datetime of the request (normally the current time is used).
         * This can be used to record visits and page views in the past.
         * The expected format is: 2011-04-05 00:11:42 (remember to URL encode the value!).
         * The datetime must be sent in UTC timezone.
         * Note: if you record data in the past, you will need to <a href="http://piwik.org/faq/how-to/#faq_59">force Piwik to re-process reports for the past dates.</a>
         */
        public static string DATETIME_OF_REQUEST = "cdt";

        /**
         * The name of the content. For instance 'Ad Foo Bar'
         *
         * @see <a href="http://piwik.org/docs/content-tracking/">Content Tracking</a>
         */
        public static string CONTENT_NAME = "c_n";

        /**
         * The actual content piece. For instance the path to an image, video, audio, any text
         *
         * @see <a href="http://piwik.org/docs/content-tracking/">Content Tracking</a>
         */
        public static string CONTENT_PIECE = "c_p";

        /**
         * The target of the content. For instance the URL of a landing page
         *
         * @see <a href="http://piwik.org/docs/content-tracking/">Content Tracking</a>
         */
        public static string CONTENT_TARGET = "c_t";

        /**
         * The name of the interaction with the content. For instance a 'click'
         *
         * @see <a href="http://piwik.org/docs/content-tracking/">Content Tracking</a>
         */
        public static string CONTENT_INTERACTION = "c_i";

        /**
         * The event category. Must not be empty. (eg. Videos, Music, Games...)
         *
         * @see <a href="http://piwik.org/docs/event-tracking/">Event Tracking</a>
         */
        public static string EVENT_CATEGORY = "e_c";

        /**
         * The event action. Must not be empty. (eg. Play, Pause, Duration, Add Playlist, Downloaded, Clicked...)
         *
         * @see <a href="http://piwik.org/docs/event-tracking/">Event Tracking</a>
         */
        public static string EVENT_ACTION = "e_a";

        /**
         * The event name. (eg. a Movie name, or Song name, or File name...)
         *
         * @see <a href="http://piwik.org/docs/event-tracking/">Event Tracking</a>
         */
        public static string EVENT_NAME = "e_n";

        /**
         * The event value. Must be a float or integer value (numeric), not a string.
         *
         * @see <a href="http://piwik.org/docs/event-tracking/">Event Tracking</a>
         */
        public static string EVENT_VALUE = "e_v";

        /**
         * If set to 0 (send_image=0) Piwik will respond with a HTTP 204 response code instead of a GIF image.<p>
         * This improves performance and can fix errors if images are not allowed to be obtained directly (eg Chrome Apps). Available since Piwik 2.10.0
         */
        public static string SEND_IMAGE = "send_image";
    }
}
