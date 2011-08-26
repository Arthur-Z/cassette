﻿using System;
using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class AssetRequestHandler : IHttpHandler
    {
        public AssetRequestHandler(RequestContext requestContext, Func<string, Module> getModuleForPath)
        {
            this.requestContext = requestContext;
            this.getModuleForPath = getModuleForPath;
        }

        readonly RequestContext requestContext;
        readonly Func<string, Module> getModuleForPath;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext _)
        {
            var path = "~/" + requestContext.RouteData.GetRequiredString("path");
            var response = requestContext.HttpContext.Response;
            var module = getModuleForPath(path);
            if (module == null)
            {
                NotFound(response);
                return;
            }
            
            var asset = module.FindAssetByPath(path);
            if (asset == null)
            {
                NotFound(response);
                return;
            }

            SendAsset(response, module, asset);
        }

        void SendAsset(HttpResponseBase response, Module module, IAsset asset)
        {
            response.ContentType = module.ContentType;
            using (var stream = asset.OpenStream())
            {
                stream.CopyTo(response.OutputStream);
            }
        }

        void NotFound(HttpResponseBase response)
        {
            response.StatusCode = 404;
            response.End();
        }
    }
}