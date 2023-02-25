// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;

namespace AdventureBot.Services
{
    public interface IGraphClientService
    {
        //public GraphServiceClient? GetUserGraphClient(string userAssertion);
        public GraphServiceClient? GetAppGraphClient();
    }
}
