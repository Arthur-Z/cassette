﻿#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    class CassetteRoute : Route
    {
        public CassetteRoute(string url, IRouteHandler routeHandler) 
            : base(
                url,
                new RouteValueDictionary(),
                new RouteValueDictionary
                {
                    {"IncomingOnlyConstraint", new OnlyMatchIncomingRequestsConstraint()}
                },
                routeHandler
            )
        {
        }

        class OnlyMatchIncomingRequestsConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                return routeDirection == RouteDirection.IncomingRequest;
            }
        }
    }
}

