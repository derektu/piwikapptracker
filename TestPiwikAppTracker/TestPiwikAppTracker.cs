﻿using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Piwik.AppTracker;

namespace TestPiwikAppTracker
{
    [TestClass]
    public class TestPiwikAppTracker
    {
        private string PiwikAppUrl = "http://192.168.33.16/piwik/piwik.php";
        private int PiwikSiteId = 4;
        private string AppDomain = "xq";

        [TestMethod]
        public void Test_trackLogin()
        {
            var appTracker = new PiwikAppTracker(PiwikAppUrl, PiwikSiteId, AppDomain);
            appTracker.setUserId("derektu");

            // TODO: 設定 使用者的資料 (as page custom variables)
            //
            appTracker.setScreenCustomVariable(0, "Login.OS", "Win7");
            appTracker.setScreenCustomVariable(1, "Login.AppVer", "5.50");

            appTracker.trackScreenView("/app/login", "login");
        }

        [TestMethod]
        public void Test_trackLogin_DifferentOS()
        {
            var appTracker = new PiwikAppTracker(PiwikAppUrl, PiwikSiteId, AppDomain);
            appTracker.setUserId("derek_onxp");

            // TODO: 設定 使用者的資料 (as page custom variables)
            //
            appTracker.setScreenCustomVariable(0, "Login.OS", "XP");
            appTracker.setScreenCustomVariable(1, "Login.AppVer", "5.40");

            appTracker.trackScreenView("/app/login", "login");
        }

        [TestMethod]
        public void Test_trackPageView()
        {
            var appTracker = new PiwikAppTracker(PiwikAppUrl, PiwikSiteId, AppDomain);
            appTracker.setUserId("derektu8");

            appTracker.trackScreenView("/home", "首頁");
            appTracker.trackScreenView("/大盤/台股大盤分析/上市加權指數及成交量分析", "上市加權指數及成交量分析");
            appTracker.trackScreenView("/報價/戰情中心", "戰情中心");
        }

        [TestMethod]
        public void Test_trackEvent()
        {
            var appTracker = new PiwikAppTracker(PiwikAppUrl, PiwikSiteId, AppDomain);
            appTracker.setUserId("derektu");

            appTracker.trackScreenView("/策略/權證策略/以權追股", "以權追股");
            appTracker.trackEvent("Action", "相關權證過濾");
            appTracker.trackEvent("Action", "最佳下單");
            appTracker.trackEvent("Action", "勾選下單");
        }

        [TestMethod]
        public void Test_trackPageCustomVariables()
        {
            var appTracker = new PiwikAppTracker(PiwikAppUrl, PiwikSiteId, AppDomain);
            appTracker.setUserId("derektu8");

            appTracker.setScreenCustomVariable(0, "Page.Via", "bookmark");
            appTracker.trackScreenView("/syspage/大盤", "大盤");

            appTracker.setScreenCustomVariable(0, "Page.Via", "id");
            appTracker.trackScreenView("/syspage/個股走勢", "個股走勢");

            appTracker.setScreenCustomVariable(0, "Trade.Source", "統一");
            appTracker.setScreenCustomVariable(1, "Trade.ID", "2330.TW");
            appTracker.setScreenCustomVariable(2, "Trade.Via", "下單bar");
            appTracker.trackScreenView("/app/trade/stock", "證劵下單");
        }

    }
}
