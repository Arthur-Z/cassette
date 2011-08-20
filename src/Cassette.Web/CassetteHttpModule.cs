﻿using System;
using System.Web;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
    {
        CassetteApplication application;
        
        public void Init(HttpApplication httpApplication)
        {
            application = StartUp.CassetteApplication;

            httpApplication.BeginRequest += HttpApplicationBeginRequest;
        }

        void HttpApplicationBeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);
            application.OnBeginRequest(context);
        }

        public void Dispose()
        {
        }
    }
}