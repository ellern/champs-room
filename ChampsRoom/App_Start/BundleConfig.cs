﻿using System.Web;
using System.Web.Optimization;

namespace ChampsRoom
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/scripts/js").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/application.js"));

            bundles.Add(new StyleBundle("~/content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.css",
                "~/Content/application.css"));
        }
    }
}
