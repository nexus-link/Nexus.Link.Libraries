# Nexus.Link.Libraries

All libraries currently support both .NET Standard (and thereby .NET Core) and .NET Framework.

### Nexus.Link.Libraries.Core

Core functionality that only depends on Microsoft libraries and Newtonsoft.Json. This is where you find things like application setup, error handling and logging.

See the [Documentation](src/Libraries.Core/README.md).

### Nexus.Link.Libraries.Web

REST functionality that is not dependent on neither ASP.NET WebApi, nor ASP.NET Core WebApps, i.e. functionality for outgoing REST calls. Examples of what it provides is handling of CorrelationId, logging HTTP requests and converting unsuccessful HTTP responses into relevant exceptions.

See the [Documentation](src/Libraries.Web/README.md).

### Nexus.Link.Libraries.Web.AspNet

Functionality for providing ASP.NET WebApi (based on .NET Framework) or ASP.NET Core WebApps (based on .NET Core), i.e. functionality for incoming REST calls.

### Nexus.Link.Libraries.Crud

Interfaces ("ICrud") and memory implementations of CRUD operations. Excellent for creating storage mocks in no time and sets the ground for other libraries that support ICrud.

### Nexus.Link.Libraries.Crud.WebApi

Makes it really easy to call REST API:s that are based on ICrud.

### Nexus.Link.Libraries.Crud.AspNet

With this functionality, you can write ICrud API:s with very little code and you can have a mock service up in no time.

### Nexus.Link.Libraries.SqlServer

Functionality for accessing data on a Microsoft SQL Server. Supports ICrud, so it is really easy to replace your storage mocks with storage in a relational database.

### Nexus.Link.Libraries.Azure

Functionality for accessing storage, queues and more in Azure. Supports ICrud, so that it is really easy to replace your storage mocks with storage in the 
cloud.

## Accessing symbols and source in Visual studio
1. In Visual Studio menu, go to Debug &rarr; Options &rarr; Debugging &rarr; General

2. Check the following boxes (You do not have to check any of the nested boxes):\
 :heavy_check_mark: Enable source server support\
 :heavy_check_mark: Enable Source Link support\
 :heavy_check_mark: Required source files to exactly match the original version

    ![Check the boxes](https://fulcrumresources.blob.core.windows.net/files/click-these.PNG)

3. Now navigate to Symbols (Debug &rarr; Options &rarr; Debugging &rarr; Symbols)

4. Click "New Azure Devops Symbol Server Location..".

    ![New Azure Devops Symbol Server Location](https://fulcrumresources.blob.core.windows.net/files/add-azure-devops-symbol-server.PNG)

5. Log into your account if you're not already logged in. Visual Studio will fetch connected DevOps organizations.

6. Select nexuslink.visualstudio.com in the list and press 'Connect'.

    ![Select nexuslink](https://fulcrumresources.blob.core.windows.net/files/select-nexuslink.PNG)

Visual Studio will now load symbols when you debugg.

### **If you're using ReSharper**
***Before configuring ReSharper, perform the steps above***\
Resharper can't access a private symbol server by default, we'll have to authenticate ourselves. 

1. Try to F12 (*Go To Definition*) of an object you're using in your solution that is implemented in a nuget. *This needs to be a nexus nuget since we only have nexus nuget symbols in our symbol server*.

2. ReSharper will now have tried to fetch symbols for this package and failed. So what you're seeing in your solution is probably decompiled sources.

3. Find the ReSharper notifications.
    
    ![Select nexuslink](https://fulcrumresources.blob.core.windows.net/files/resharper-notifications.PNG)

4. Identify and click the notification asking you to authenticate towards the symbol server. *Due to a ReSharper bug this notifcation does not always appear. Retry these steps until it does*.
    
    ![Select nexuslink](https://fulcrumresources.blob.core.windows.net/files/resharper-notification.PNG)

5. Authenticate yourself using your email address (name.lastname@xlent.se) and retrieve a personal access token from Azure Devops. See link.
    
    [Generate Personal Access Token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page)

    The PAT you're generating should have the following permissions: **Code *Read***.

 

All done!

## License

All libraries are provided under the [MIT license](https://choosealicense.com/licenses/mit/):

```
MIT License

Copyright (c) 2018 Nexus Link AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
